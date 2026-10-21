using Lab1MAUI.Models.UI.Participants;

namespace Lab1MAUI.ViewModels.Edit
{
    public class RoomEditViewModel : BaseEditViewModel<RoomUI>
    {
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

        public RoomEditViewModel()
        {
        }

        protected override void LoadData(RoomUI entity)
        {
            Name = entity.Name;
            Capacity = entity.Capacity;
        }

        protected override async Task<bool> ValidateBeforeSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowAlertAsync("Помилка валідації", "Будь ласка, вкажіть назву аудиторії.");
                return false;
            }

            if (Capacity <= 0)
            {
                await ShowAlertAsync("Помилка валідації", "Місткість аудиторії повинна бути більшою за 0.");
                return false;
            }

            return true;
        }

        protected override void ApplyData(RoomUI entity)
        {
            entity.Name = Name;
            entity.Capacity = Capacity;
        }
    }
}
