using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(GroupId), "id")]
    public class GroupEditViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;
        private StudentGroupMTM _group;
        private bool _isNew;

        public string GroupId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _group = new StudentGroupMTM();
                    LoadData();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _group = _scheduleMTM.Groups.FirstOrDefault(g => g.Id == id) ?? new StudentGroupMTM();
                    LoadData();
                }
            }
        }

        private string _groupName = string.Empty;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        private int _studentsCount;
        public int StudentsCount
        {
            get => _studentsCount;
            set => SetProperty(ref _studentsCount, value);
        }

        public ConstraintFormViewModel ConstraintForm { get; private set; }
        public DisciplineSelectionViewModel DisciplineSelection { get; private set; }

        public ObservableCollection<GroupRequirementItem> Requirements { get; } = new();
        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand AddRequirementCommand { get; }
        public ICommand DeleteRequirementCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private ConstraintMTM _draggedConstraint;

        public GroupEditViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            DisciplineSelection = new DisciplineSelectionViewModel(_scheduleMTM);
            DisciplineSelection.OnDisciplineSelected += d =>
            {
                if (!Requirements.Any(r => r.DisciplineId == d.Id))
                    Requirements.Add(new GroupRequirementItem(d.Id, d.Name, 2));
            };

            SaveCommand = new Command(async () =>
            {
                int totalReqHours = Requirements.Sum(r => r.RequiredHours);
                if (totalReqHours > _scheduleMTM.Config.TotalCells)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Перевантаження",
                            $"Загальна кількість годин ({totalReqHours}) перевищує фізично можливу кількість годин на тиждень ({_scheduleMTM.Config.TotalCells}).",
                            "ОК");
                    }
                    return;
                }

                _group.Name = GroupName;
                _group.StudentsCount = StudentsCount;

                _group.Requirements.Clear();
                foreach (var r in Requirements)
                    _group.Requirements.Add(new GroupRequirementMTM { DisciplineId = r.DisciplineId, RequiredHours = r.RequiredHours });

                _group.Constraints.Clear();
                foreach (var c in Constraints) _group.Constraints.Add(c);

                if (_isNew)
                {
                    _scheduleMTM.Groups.Add(_group);
                }
                else
                {
                    var index = _scheduleMTM.Groups.IndexOf(_group);
                    if (index >= 0)
                        _scheduleMTM.Groups[index] = _group;
                }
                await BaseViewModel.NavigationAction("..");
            });

            AddRequirementCommand = new Command(() => DisciplineSelection.Open());
            DeleteRequirementCommand = new Command<GroupRequirementItem>(req => { if (req != null) Requirements.Remove(req); });
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
            GroupName = _group.Name;
            StudentsCount = _group.StudentsCount;

            Requirements.Clear();
            foreach (var r in _group.Requirements)
            {
                var disciplineName = _scheduleMTM.Disciplines
                    .FirstOrDefault(d => d.Id == r.DisciplineId)?.Name ?? "Невідома дисципліна";
                Requirements.Add(new GroupRequirementItem(r.DisciplineId, disciplineName, r.RequiredHours));
            }

            Constraints.Clear();
            foreach (var c in _group.Constraints) Constraints.Add(c);

            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Group, _scheduleMTM.Config);
            ConstraintForm.OnConstraintAdded += c => Constraints.Add(c);
            OnPropertyChanged(nameof(ConstraintForm));
        }
    }

    /// <summary>
    /// Display-модель для вимоги групи у списку редагування.
    /// Реалізує INotifyPropertyChanged для редагування RequiredHours прямо в списку.
    /// </summary>
    public class GroupRequirementItem : BaseViewModel
    {
        public Guid DisciplineId { get; }
        public string DisciplineName { get; }

        private int _requiredHours;
        public int RequiredHours
        {
            get => _requiredHours;
            set => SetProperty(ref _requiredHours, value);
        }

        public GroupRequirementItem(Guid disciplineId, string disciplineName, int requiredHours)
        {
            DisciplineId = disciplineId;
            DisciplineName = disciplineName;
            _requiredHours = requiredHours;
        }
    }
}
