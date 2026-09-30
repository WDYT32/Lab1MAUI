using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public class StudentGroupsViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        public ObservableCollection<StudentGroup> Groups { get; } = new();

        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private StudentGroup _draggedGroup;

        public StudentGroupsViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            LoadGroups();
            
            AddGroupCommand = new Command(async () => 
            {
                await BaseViewModel.NavigationAction("GroupEditPage?id=new");
            });
            
            EditGroupCommand = new Command<StudentGroup>(async (group) => 
            {
                if (group != null)
                    await BaseViewModel.NavigationAction($"GroupEditPage?id={group.Id}");
            });

            DeleteGroupCommand = new Command<StudentGroup>((group) => 
            {
                if (group != null)
                {
                    Groups.Remove(group);
                    _schedule.Groups.Remove(group);
                }
            });

            DragStartingCommand = new Command<StudentGroup>(group => _draggedGroup = group);
            
            DropCommand = new Command<StudentGroup>(group => 
            {
                if (_draggedGroup != null && _draggedGroup != group)
                {
                    var draggedIndex = Groups.IndexOf(_draggedGroup);
                    var dropIndex = Groups.IndexOf(group);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Groups.Move(draggedIndex, dropIndex);
                        SyncScheduleList();
                    }
                }
                _draggedGroup = null;
            });
        }

        public void LoadGroups()
        {
            Groups.Clear();
            foreach (var g in _schedule.Groups)
            {
                Groups.Add(g);
            }
        }

        private void SyncScheduleList()
        {
            _schedule.Groups.Clear();
            foreach (var g in Groups)
            {
                _schedule.Groups.Add(g);
            }
        }
    }
}
