using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    [QueryProperty(nameof(TeacherId), "id")]
    public class TeacherEditViewModel : BaseEditViewModel<TeacherUI>
    {
        public string TeacherId { set => LoadEntityById(value); }

        public string FirstName
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public string LastName
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public string Patronymic
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public int TargetHours
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ConstraintsEditorViewModel ConstraintsEditor { get; }
        public DisciplineSelectionViewModel DisciplineSelection { get; private set; }
        public ObservableCollection<TeacherCompetenceItem> Competences { get; } = new();

        public ICommand AddCompetenceCommand { get; }
        public ICommand DeleteCompetenceCommand { get; }

        public TeacherEditViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI, scheduleUI.Teachers)
        {
            ConstraintsEditor = new ConstraintsEditorViewModel(ConstraintOwnerType.Teacher, scheduleUI.Config);
            DisciplineSelection = new DisciplineSelectionViewModel(ScheduleUI);
            DisciplineSelection.OnDisciplineSelected += d =>
            {
                if (!Competences.Any(c => c.DisciplineId == d.Id))
                    Competences.Add(new TeacherCompetenceItem(d.Id, d.Name));
            };

            AddCompetenceCommand = new Command(() => DisciplineSelection.Open());
            DeleteCompetenceCommand = new Command<TeacherCompetenceItem>(item =>
            {
                if (item != null) Competences.Remove(item);
            });
        }

        protected override void LoadData(TeacherUI entity)
        {
            LastName   = entity.LastName;
            FirstName  = entity.FirstName;
            Patronymic = entity.Patronymic;
            TargetHours = entity.TargetHours;

            Competences.Clear();
            foreach (var comp in entity.Competences)
            {
                var disciplineName = ScheduleUI.Disciplines
                    .FirstOrDefault(d => d.Id == comp.DisciplineId)?.Name ?? "Невідома дисципліна";
                Competences.Add(new TeacherCompetenceItem(comp.DisciplineId, disciplineName));
            }

            ConstraintsEditor.LoadFrom(entity.Constraints);
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            if (TargetHours > ScheduleUI.Config.TotalCells)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Перевантаження",
                        $"Цільові години ({TargetHours}) перевищують фізично можливу кількість годин на тиждень ({ScheduleUI.Config.TotalCells}).",
                        "ОК");
                }
                return false;
            }
            return true;
        }

        protected override void ApplyData(TeacherUI entity)
        {
            entity.LastName = LastName;
            entity.FirstName = FirstName;
            entity.Patronymic = Patronymic;
            entity.TargetHours = TargetHours;

            entity.Competences.Clear();
            foreach (var c in Competences)
                entity.Competences.Add(new TeacherCompetenceUI { DisciplineId = c.DisciplineId });

            ConstraintsEditor.ApplyTo(entity.Constraints);
        }
    }

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
