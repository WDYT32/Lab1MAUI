using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    [QueryProperty(nameof(GroupId), "id")]
    public class GroupEditViewModel : BaseEditViewModel<StudentGroupUI>
    {
        public string GroupId { set => LoadEntityById(value); }

        public string Name
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public int StudentsCount
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ConstraintsEditorViewModel ConstraintsEditor { get; }
        public DisciplineSelectionViewModel DisciplineSelection { get; private set; }
        public ObservableCollection<GroupRequirementItem> Requirements { get; } = new();

        public ICommand AddRequirementCommand { get; }
        public ICommand DeleteRequirementCommand { get; }

        public GroupEditViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI, scheduleUI.Groups)
        {
            ConstraintsEditor = new ConstraintsEditorViewModel(ConstraintOwnerType.Group, scheduleUI.Config);
            DisciplineSelection = new DisciplineSelectionViewModel(ScheduleUI);
            DisciplineSelection.OnDisciplineSelected += d =>
            {
                if (!Requirements.Any(r => r.DisciplineId == d.Id))
                    Requirements.Add(new GroupRequirementItem(d.Id, d.Name, 2));
            };

            AddRequirementCommand = new Command(() => DisciplineSelection.Open());
            DeleteRequirementCommand = new Command<GroupRequirementItem>(req =>
            {
                if (req != null) Requirements.Remove(req);
            });
        }

        protected override void LoadData(StudentGroupUI entity)
        {
            Name = entity.Name;
            StudentsCount = entity.StudentsCount;

            Requirements.Clear();
            foreach (var r in entity.Requirements)
            {
                var disciplineName = ScheduleUI.Disciplines
                    .FirstOrDefault(d => d.Id == r.DisciplineId)?.Name ?? "Невідома дисципліна";
                Requirements.Add(new GroupRequirementItem(r.DisciplineId, disciplineName, r.RequiredHours));
            }

            ConstraintsEditor.LoadFrom(entity.Constraints);
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            int totalReqHours = Requirements.Sum(r => r.RequiredHours);
            if (totalReqHours > ScheduleUI.Config.TotalCells)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Перевантаження",
                        $"Загальна кількість годин ({totalReqHours}) перевищує фізично можливу кількість годин на тиждень ({ScheduleUI.Config.TotalCells}).",
                        "ОК");
                }
                return false;
            }
            return true;
        }

        protected override void ApplyData(StudentGroupUI entity)
        {
            entity.Name = Name;
            entity.StudentsCount = StudentsCount;

            entity.Requirements.Clear();
            foreach (var r in Requirements)
                entity.Requirements.Add(new GroupRequirementUI { DisciplineId = r.DisciplineId, RequiredHours = r.RequiredHours });

            ConstraintsEditor.ApplyTo(entity.Constraints);
        }
    }

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
