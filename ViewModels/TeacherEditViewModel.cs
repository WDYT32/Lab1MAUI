using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(TeacherId), "id")]
    public class TeacherEditViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;
        private TeacherMTM _teacher;
        private bool _isNew;

        public string TeacherId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _teacher = new TeacherMTM();
                    LoadData();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _teacher = _scheduleMTM.Teachers.FirstOrDefault(t => t.Id == id) ?? new TeacherMTM();
                    LoadData();
                }
            }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _patronymic = string.Empty;
        public string Patronymic
        {
            get => _patronymic;
            set => SetProperty(ref _patronymic, value);
        }

        private int _targetHours;
        public int TargetHours
        {
            get => _targetHours;
            set => SetProperty(ref _targetHours, value);
        }

        public ConstraintFormViewModel ConstraintForm { get; private set; }
        public DisciplineSelectionViewModel DisciplineSelection { get; private set; }

        public ObservableCollection<TeacherCompetenceItem> Competences { get; } = new();
        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand AddCompetenceCommand { get; }
        public ICommand DeleteCompetenceCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private ConstraintMTM _draggedConstraint;

        public TeacherEditViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            DisciplineSelection = new DisciplineSelectionViewModel(_scheduleMTM);
            DisciplineSelection.OnDisciplineSelected += d =>
            {
                if (!Competences.Any(c => c.DisciplineId == d.Id))
                    Competences.Add(new TeacherCompetenceItem(d.Id, d.Name));
            };

            SaveCommand = new Command(async () =>
            {
                if (TargetHours > _scheduleMTM.Config.TotalCells)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Перевантаження",
                            $"Цільові години ({TargetHours}) перевищують фізично можливу кількість годин на тиждень ({_scheduleMTM.Config.TotalCells}).",
                            "ОК");
                    }
                    return;
                }

                _teacher.Name = $"{LastName} {FirstName} {Patronymic}".Trim();
                _teacher.TargetHours = TargetHours;

                _teacher.Competences.Clear();
                foreach (var c in Competences)
                    _teacher.Competences.Add(new TeacherCompetenceMTM { DisciplineId = c.DisciplineId });

                _teacher.Constraints.Clear();
                foreach (var c in Constraints) _teacher.Constraints.Add(c);

                if (_isNew)
                {
                    _scheduleMTM.Teachers.Add(_teacher);
                }
                else
                {
                    var index = _scheduleMTM.Teachers.IndexOf(_teacher);
                    if (index >= 0)
                        _scheduleMTM.Teachers[index] = _teacher;
                }
                await BaseViewModel.NavigationAction("..");
            });

            AddCompetenceCommand = new Command(() => DisciplineSelection.Open());
            DeleteCompetenceCommand = new Command<TeacherCompetenceItem>(item => { if (item != null) Competences.Remove(item); });
            DeleteConstraintCommand = new Command<ConstraintMTM>(c => { if (c != null) Constraints.Remove(c); });

            DragStartingConstraintCommand = new Command<ConstraintMTM>(c => _draggedConstraint = c);
            DropConstraintCommand = new Command<ConstraintMTM>(c =>
            {
                if (_draggedConstraint != null && _draggedConstraint != c)
                {
                    var from = Constraints.IndexOf(_draggedConstraint);
                    var to   = Constraints.IndexOf(c);
                    if (from >= 0 && to >= 0)
                        Constraints.Move(from, to);
                }
                _draggedConstraint = null;
            });
        }

        private void LoadData()
        {
            var parts = _teacher.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
            LastName   = parts.Length > 0 ? parts[0] : string.Empty;
            FirstName  = parts.Length > 1 ? parts[1] : string.Empty;
            Patronymic = parts.Length > 2 ? parts[2] : string.Empty;

            TargetHours = _teacher.TargetHours;

            Competences.Clear();
            foreach (var comp in _teacher.Competences)
            {
                var disciplineName = _scheduleMTM.Disciplines
                    .FirstOrDefault(d => d.Id == comp.DisciplineId)?.Name ?? "Невідома дисципліна";
                Competences.Add(new TeacherCompetenceItem(comp.DisciplineId, disciplineName));
            }

            Constraints.Clear();
            foreach (var c in _teacher.Constraints) Constraints.Add(c);

            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, _scheduleMTM.Config);
            ConstraintForm.OnConstraintAdded += c => Constraints.Add(c);
            OnPropertyChanged(nameof(ConstraintForm));
        }
    }

    /// <summary>
    /// Display-модель для компетенції викладача у списку редагування.
    /// </summary>
    public class TeacherCompetenceItem
    {
        public Guid DisciplineId { get; }
        public string DisciplineName { get; }

        public TeacherCompetenceItem(Guid disciplineId, string disciplineName)
        {
            DisciplineId = disciplineId;
            DisciplineName = disciplineName;
        }
    }
}
