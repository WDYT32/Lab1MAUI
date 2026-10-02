using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public class StudentGroupsViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;

        /// <summary>
        /// Напряму прив'язана до ScheduleMTM.Groups — жодної синхронізації не потрібно.
        /// </summary>
        public ObservableCollection<StudentGroupMTM> Groups => _scheduleMTM.Groups;

        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private StudentGroupMTM _draggedGroup;

        public StudentGroupsViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            AddGroupCommand = new Command(async () =>
            {
                await BaseViewModel.NavigationAction("GroupEditPage?id=new");
            });

            EditGroupCommand = new Command<StudentGroupMTM>(async (group) =>
            {
                if (group != null)
                    await BaseViewModel.NavigationAction($"GroupEditPage?id={group.Id}");
            });

            DeleteGroupCommand = new Command<StudentGroupMTM>((group) =>
            {
                if (group != null)
                    _scheduleMTM.Groups.Remove(group);
            });

            DragStartingCommand = new Command<StudentGroupMTM>(g => _draggedGroup = g);

            DropCommand = new Command<StudentGroupMTM>(g =>
            {
                if (_draggedGroup != null && _draggedGroup != g)
                {
                    var from = Groups.IndexOf(_draggedGroup);
                    var to   = Groups.IndexOf(g);
                    if (from >= 0 && to >= 0)
                        Groups.Move(from, to);
                }
                _draggedGroup = null;
            });
        }
    }
}
