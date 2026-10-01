using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(TeacherId), "id")]
    public class TeacherEditViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        private Teacher _teacher;
        private bool _isNew;

        public string TeacherId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _teacher = new Teacher(Guid.NewGuid(), "", _schedule.Config.TotalCells);
                    LoadData();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _teacher = _schedule.Teachers.FirstOrDefault(t => t.Id == id) ?? new Teacher(Guid.NewGuid(), "", _schedule.Config.TotalCells);
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
        public ObservableCollection<IConstraint> Constraints { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand AddCompetenceCommand { get; }
        public ICommand DeleteCompetenceCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand MoveUpConstraintCommand { get; }
        public ICommand MoveDownConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private IConstraint _draggedConstraint;

        public TeacherEditViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            DisciplineSelection = new DisciplineSelectionViewModel(_schedule);
            DisciplineSelection.OnDisciplineSelected += d => 
            {
                if (!Competences.Any(c => c.Model.DisciplineId == d.Id))
                {
                    Competences.Add(new TeacherCompetenceItem(new TeacherCompetence { DisciplineId = d.Id }, _schedule));
                }
            };
            
            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, new Teacher(Guid.Empty, "", schedule.Config.TotalCells), _schedule);

            SaveCommand = new Command(async () => 
            {
                if (TargetHours > _schedule.Config.TotalCells)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Перевантаження", 
                            $"Цільові години ({TargetHours}) перевищують фізично можливу кількість годин на тиждень ({_schedule.Config.TotalCells}).", "ОК");
                    }
                    return;
                }

                _teacher.Name = $"{LastName} {FirstName} {Patronymic}".Trim();
                _teacher.TargetHours = TargetHours;
                
                _teacher.Competences.Clear();
                foreach (var c in Competences) _teacher.Competences.Add(c.Model);
                
                _teacher.Constraints.Clear();
                foreach (var c in Constraints) _teacher.Constraints.Add(c);

                if (_isNew) _schedule.Teachers.Add(_teacher);
                await BaseViewModel.NavigationAction("..");
            });

            AddCompetenceCommand = new Command(() =>
            {
                DisciplineSelection.Open();
            });
            
            DeleteCompetenceCommand = new Command<TeacherCompetenceItem>(comp => { if (comp != null) Competences.Remove(comp); });
            DeleteConstraintCommand = new Command<IConstraint>(c => { if (c != null) Constraints.Remove(c); });

            DragStartingConstraintCommand = new Command<IConstraint>(c => _draggedConstraint = c);
            
            DropConstraintCommand = new Command<IConstraint>(c => 
            {
                if (_draggedConstraint != null && _draggedConstraint != c)
                {
                    var draggedIndex = Constraints.IndexOf(_draggedConstraint);
                    var dropIndex = Constraints.IndexOf(c);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Constraints.Move(draggedIndex, dropIndex);
                    }
                }
                _draggedConstraint = null;
            });
        }

        private void LoadData()
        {
            var parts = _teacher.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            LastName = parts.Length > 0 ? parts[0] : "";
            FirstName = parts.Length > 1 ? parts[1] : "";
            Patronymic = parts.Length > 2 ? parts[2] : "";

            TargetHours = _teacher.TargetHours;
            
            Competences.Clear();
            foreach (var c in _teacher.Competences) Competences.Add(new TeacherCompetenceItem(c, _schedule));

            Constraints.Clear();
            foreach (var c in _teacher.Constraints) Constraints.Add(c);

            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, _teacher, _schedule);
            ConstraintForm.OnConstraintAdded += (c) => Constraints.Add(c);
            OnPropertyChanged(nameof(ConstraintForm));
        }
    }

    public class TeacherCompetenceItem
    {
        public TeacherCompetence Model { get; }
        public string DisciplineName { get; }

        public TeacherCompetenceItem(TeacherCompetence model, Schedule schedule)
        {
            Model = model;
            DisciplineName = schedule.Disciplines.FirstOrDefault(d => d.Id == model.DisciplineId)?.Name ?? "Невідома дисципліна";
        }
    }
}
