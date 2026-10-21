using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;

namespace Lab1MAUI.ViewModels.Edit
{
    [QueryProperty(nameof(RoomId), "id")]
    public class RoomEditViewModel : BaseEditViewModel<RoomUI>
    {
        public string RoomId { set => LoadEntityById(value); }

        public string Name
        {
            get;
            set => SetProperty(ref field, value);
        } = string.Empty;

        public int Capacity
        {
            get;
            set => SetProperty(ref field, value);
        } = 30;

        public RoomEditViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI, scheduleUI.Rooms)
        {
        }

        protected override void LoadData(RoomUI entity)
        {
            Name = entity.Name;
            Capacity = IsNew ? 30 : entity.Capacity;
        }

        protected override Task<bool> ValidateBeforeSaveAsync()
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(Name));
        }

        protected override void ApplyData(RoomUI entity)
        {
            entity.Name = Name;
            entity.Capacity = Capacity;
        }
    }
}
