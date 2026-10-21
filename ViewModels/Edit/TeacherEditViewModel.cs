using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public class TeacherEditViewModel : BaseEditViewModel<TeacherUI>
    {
        private readonly ScheduleUI _scheduleUI;

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

        public int TargetLessons
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ConstraintsEditorViewModel ConstraintsEditor { get; }
        public DisciplineSelectionViewModel DisciplineSelection { get; }
        public ObservableCollection<TeacherCompetenceItem> Competences { get; } = new();

        public ICommand AddCompetenceCommand { get; }
        public ICommand DeleteCompetenceCommand { get; }

        public TeacherEditViewModel(ScheduleUI scheduleUI)
        {
            _scheduleUI = scheduleUI;
            ConstraintsEditor = new ConstraintsEditorViewModel(ConstraintOwnerType.Teacher);
            DisciplineSelection = new DisciplineSelectionViewModel(scheduleUI);

            DisciplineSelection.OnDisciplineSelected += d =>
            {
                if (Competences.All(c => c.DisciplineId != d.Id))
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
            TargetLessons = entity.TargetLessons;

            Competences.Clear();
            foreach (var comp in entity.Competences)
            {
                var disciplineName = _scheduleUI.Disciplines
                    .FirstOrDefault(d => d.Id == comp.DisciplineId)?.Name ?? "Невідома дисципліна";
                Competences.Add(new TeacherCompetenceItem(comp.DisciplineId, disciplineName));
            }

            ConstraintsEditor.LoadFrom(entity.Constraints);
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName))
            {
                await ShowAlertAsync("Помилка валідації", "Будь ласка, заповніть прізвище та ім'я викладача.");
                return false;
            }

            if (TargetLessons > _scheduleUI.Config.TotalCells)
            {
                await ShowAlertAsync("Перевантаження",
                    $"Цільова кількість занять ({TargetLessons}) перевищує фізично можливу кількість уроків на тиждень ({_scheduleUI.Config.TotalCells}).");
                return false;
            }
            return true;
        }

        protected override void ApplyData(TeacherUI entity)
        {
            entity.LastName = LastName;
            entity.FirstName = FirstName;
            entity.Patronymic = Patronymic;
            entity.TargetLessons = TargetLessons;

            entity.Competences.Clear();
            foreach (var c in Competences)
                entity.Competences.Add(new TeacherCompetenceUI { DisciplineId = c.DisciplineId });

            ConstraintsEditor.ApplyTo(entity.Constraints);
        }
    }

    public class TeacherCompetenceItem(Guid disciplineId, string disciplineName)
    {
        public Guid DisciplineId { get; } = disciplineId;
        public string DisciplineName { get; } = disciplineName;
    }
}
