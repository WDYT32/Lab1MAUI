using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(GroupId), "id")]
    public class GroupEditViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        private StudentGroup _group;
        private bool _isNew;

        public string GroupId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _group = new StudentGroup(Guid.NewGuid(), "", _schedule.Config.TotalCells);
                    LoadData();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _group = _schedule.Groups.FirstOrDefault(g => g.Id == id) ?? new StudentGroup(Guid.NewGuid(), "", _schedule.Config.TotalCells);
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
        public ObservableCollection<IConstraint> Constraints { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand AddRequirementCommand { get; }
        public ICommand DeleteRequirementCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand MoveUpConstraintCommand { get; }
        public ICommand MoveDownConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private IConstraint _draggedConstraint;

        public GroupEditViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            DisciplineSelection = new DisciplineSelectionViewModel(_schedule);
            DisciplineSelection.OnDisciplineSelected += d => 
            {
                if (!Requirements.Any(r => r.Model.DisciplineId == d.Id))
                {
                    Requirements.Add(new GroupRequirementItem(new GroupRequirement { DisciplineId = d.Id, RequiredHours = 2 }, _schedule));
                }
            };

            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Group, new StudentGroup(Guid.Empty, "", schedule.Config.TotalCells), _schedule);

            SaveCommand = new Command(async () => 
            {
                int totalReqHours = Requirements.Sum(r => r.RequiredHours);
                if (totalReqHours > _schedule.Config.TotalCells)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Перевантаження", 
                            $"Загальна кількість годин ({totalReqHours}) перевищує фізично можливу кількість годин на тиждень ({_schedule.Config.TotalCells}).", "ОК");
                    }
                    return;
                }

                _group.Name = GroupName;
                _group.StudentsCount = StudentsCount;
                
                _group.Requirements.Clear();
                foreach (var r in Requirements) _group.Requirements.Add(r.Model);
                
                _group.Constraints.Clear();
                foreach (var c in Constraints) _group.Constraints.Add(c);

                if (_isNew) _schedule.Groups.Add(_group);
                await BaseViewModel.NavigationAction("..");
            });

            AddRequirementCommand = new Command(() =>
            {
                DisciplineSelection.Open();
            });
            
            DeleteRequirementCommand = new Command<GroupRequirementItem>(req => { if (req != null) Requirements.Remove(req); });
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
            GroupName = _group.Name;
            
            Requirements.Clear();
            foreach (var r in _group.Requirements) Requirements.Add(new GroupRequirementItem(r, _schedule));

            Constraints.Clear();
            foreach (var c in _group.Constraints) Constraints.Add(c);

            ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Group, _group, _schedule);
            ConstraintForm.OnConstraintAdded += (c) => Constraints.Add(c);
            OnPropertyChanged(nameof(ConstraintForm));
        }
    }

    public class GroupRequirementItem : BaseViewModel
    {
        public GroupRequirement Model { get; }
        public string DisciplineName { get; }

        public int RequiredHours
        {
            get => Model.RequiredHours;
            set
            {
                if (Model.RequiredHours != value)
                {
                    Model.RequiredHours = value;
                    OnPropertyChanged();
                }
            }
        }

        public GroupRequirementItem(GroupRequirement model, Schedule schedule)
        {
            Model = model;
            DisciplineName = schedule.Disciplines.FirstOrDefault(d => d.Id == model.DisciplineId)?.Name ?? "Невідома дисципліна";
        }
    }
}
