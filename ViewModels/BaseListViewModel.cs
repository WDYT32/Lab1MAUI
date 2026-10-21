using Lab1MAUI.Models.UI.Participants;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public abstract class BaseListViewModel<TEntity> : BaseViewModel 
        where TEntity : class, IParticipantUI
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
                    await Shell.Current.GoToAsync($"{EditPageRoute}?id=new");
            });

            EditCommand = new Command<TEntity>(async (item) =>
            {
                if (item != null && Shell.Current != null)
                    await Shell.Current.GoToAsync($"{EditPageRoute}?id={item.Id}");
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
    }
}
