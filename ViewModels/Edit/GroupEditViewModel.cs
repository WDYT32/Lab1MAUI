using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public class GroupEditViewModel : BaseEditViewModel<StudentGroupUI>
    {
        private readonly ScheduleUI _scheduleUI;

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
        public DisciplineSelectionViewModel DisciplineSelection { get; }
        public ObservableCollection<GroupRequirementItem> Requirements { get; } = new();

        public ICommand AddRequirementCommand { get; }
        public ICommand DeleteRequirementCommand { get; }

        public GroupEditViewModel(ScheduleUI scheduleUI)
        {
            _scheduleUI = scheduleUI;
            ConstraintsEditor = new ConstraintsEditorViewModel(ConstraintOwnerType.Group);
            DisciplineSelection = new DisciplineSelectionViewModel(scheduleUI);

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
                var disciplineName = _scheduleUI.Disciplines
                    .FirstOrDefault(d => d.Id == r.DisciplineId)?.Name ?? "Невідома дисципліна";
                Requirements.Add(new GroupRequirementItem(r.DisciplineId, disciplineName, r.RequiredLessons));
            }

            ConstraintsEditor.LoadFrom(entity.Constraints);
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowAlertAsync("Помилка валідації", "Будь ласка, вкажіть назву групи.");
                return false;
            }

            int totalReqLessons = Requirements.Sum(r => r.RequiredLessons);
            if (totalReqLessons > _scheduleUI.Config.TotalCells)
            {
                await ShowAlertAsync("Перевантаження",
                    $"Загальна кількість занять ({totalReqLessons}) перевищує фізично можливу кількість уроків на тиждень ({_scheduleUI.Config.TotalCells}).");
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
                entity.Requirements.Add(new GroupRequirementUI { DisciplineId = r.DisciplineId, RequiredLessons = r.RequiredLessons });

            ConstraintsEditor.ApplyTo(entity.Constraints);
        }
    }

    public class GroupRequirementItem(Guid disciplineId, string disciplineName, int requiredLessons)
        : BaseViewModel
    {
        public Guid DisciplineId { get; } = disciplineId;
        public string DisciplineName { get; } = disciplineName;

        private int _requiredLessons = requiredLessons;
        public int RequiredLessons
        {
            get => _requiredLessons;
            set => SetProperty(ref _requiredLessons, value);
        }
    }
}
