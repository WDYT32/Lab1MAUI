using Lab1MAUI.Models.UI.Participants;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public abstract class BaseEditViewModel<TEntity> : BaseViewModel, IQueryAttributable
        where TEntity : class, IParticipantUI, new()
    {
        public TEntity Entity { get; protected set; }

        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }

        protected BaseEditViewModel()
        {
            Entity = new TEntity();
            SaveCommand = new Command(async () => await SaveAsync());
            GoBackCommand = new Command(async () =>
            {
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("..");
            });
        }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Entity", out var entityObj) && entityObj is TEntity entity)
            {
                query.Remove("Entity");
                Entity = entity;
                LoadData(Entity);
            }
        }

        protected abstract void LoadData(TEntity entity);
        protected abstract void ApplyData(TEntity entity);

        protected virtual Task<bool> ValidateBeforeSaveAsync() => Task.FromResult(true);

        protected async Task ShowAlertAsync(string title, string message)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlertAsync(title, message, "ОК");
        }

        protected virtual async Task SaveAsync()
        {
            if (Entity == null) return;

            if (!await ValidateBeforeSaveAsync())
                return;

            ApplyData(Entity);

            if (Shell.Current != null)
            {
                var navParams = new Dictionary<string, object>
                {
                    { "SavedItem", Entity }
                };
                await Shell.Current.GoToAsync("..", navParams);
            }
        }
    }
}
