using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;

namespace Lab1MAUI.ViewModels.Edit
{
    [QueryProperty(nameof(DisciplineId), "id")]
    public class DisciplineEditViewModel : BaseEditViewModel<DisciplineUI>
    {
        public string DisciplineId { set => LoadEntityById(value); }

        public string Name
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public ConstraintsEditorViewModel ConstraintsEditor { get; }

        public DisciplineEditViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI, scheduleUI.Disciplines)
        {
            ConstraintsEditor = new ConstraintsEditorViewModel(ConstraintOwnerType.Discipline, scheduleUI.Config);
        }

        protected override void LoadData(DisciplineUI entity)
        {
            Name = entity.Name;
            ConstraintsEditor.LoadFrom(entity.Constraints);
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowAlertAsync("Помилка валідації", "Будь ласка, вкажіть назву дисципліни.");
                return false;
            }
            return true;
        }

        protected override void ApplyData(DisciplineUI entity)
        {
            entity.Name = Name;
            ConstraintsEditor.ApplyTo(entity.Constraints);
        }
    }
}
