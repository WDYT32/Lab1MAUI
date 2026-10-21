using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public abstract class BaseListViewModel<TEntity> : BaseViewModel, IQueryAttributable
        where TEntity : class, IParticipantUI, new()
    {
        public ObservableCollection<TEntity> Items { get; }
        public string EditPageRoute { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private TEntity? _draggedItem;

        protected BaseListViewModel(ObservableCollection<TEntity> items, string editPageRoute)
        {
            Items = items;
            EditPageRoute = editPageRoute;

            AddCommand = new Command(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(EditPageRoute, new Dictionary<string, object>
                    {
                        { "Entity", new TEntity() }
                    });
                }
            });

            EditCommand = new Command<TEntity>(async (item) =>
            {
                if (item != null && Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(EditPageRoute, new Dictionary<string, object>
                    {
                        { "Entity", item }
                    });
                }
            });

            DeleteCommand = new Command<TEntity>((item) =>
            {
                if (item != null)
                    Items.Remove(item);
            });

            DragStartingCommand = new Command<TEntity>(item => _draggedItem = item);

            DropCommand = new Command<TEntity>(item =>
            {
                if (_draggedItem != null && _draggedItem != item)
                {
                    var from = Items.IndexOf(_draggedItem);
                    var to = Items.IndexOf(item);
                    if (from >= 0 && to >= 0)
                        Items.Move(from, to);
                }
                _draggedItem = null;
            });
        }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("SavedItem", out var itemObj) && itemObj is TEntity item)
            {
                query.Remove("SavedItem");
                var index = Items.IndexOf(item);
                if (index >= 0)
                    Items[index] = item;
                else
                    Items.Add(item);
            }
        }
    }
}
