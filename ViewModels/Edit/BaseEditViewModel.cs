using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public abstract class BaseEditViewModel<TEntity> : BaseViewModel 
        where TEntity : class, IParticipantUI, new()
    {
        protected readonly ScheduleUI ScheduleUI;
        protected readonly ObservableCollection<TEntity> EntityCollection;

        public TEntity Entity { get; protected set; }
        public bool IsNew { get; protected set; }

        public ICommand SaveCommand { get; }

        protected BaseEditViewModel(ScheduleUI scheduleUI, ObservableCollection<TEntity> entityCollection)
        {
            ScheduleUI = scheduleUI;
            EntityCollection = entityCollection;
            Entity = new TEntity();
            SaveCommand = new Command(async () => await SaveAsync());
        }

        public void LoadEntityById(string? value)
        {
            if (value == "new")
            {
                IsNew = true;
                Entity = new TEntity();
                LoadData(Entity);
            }
            else if (Guid.TryParse(value, out Guid id))
            {
                IsNew = false;
                Entity = EntityCollection.FirstOrDefault(e => e.Id == id) ?? new TEntity();
                LoadData(Entity);
            }
        }

        protected abstract void LoadData(TEntity entity);
        protected abstract void ApplyData(TEntity entity);

        protected virtual Task<bool> ValidateBeforeSaveAsync() => Task.FromResult(true);

        protected virtual async Task SaveAsync()
        {
            if (Entity == null) return;

            if (!await ValidateBeforeSaveAsync())
                return;

            ApplyData(Entity);

            if (IsNew)
            {
                if (!EntityCollection.Contains(Entity))
                    EntityCollection.Add(Entity);
            }
            else
            {
                var index = EntityCollection.IndexOf(Entity);
                if (index >= 0)
                    EntityCollection[index] = Entity;
                else if (!EntityCollection.Contains(Entity))
                    EntityCollection.Add(Entity);
            }

            if (Shell.Current != null)
                await Shell.Current.GoToAsync("..");
        }
    }
}
