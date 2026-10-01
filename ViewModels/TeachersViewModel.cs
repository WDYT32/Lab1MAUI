using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    // Повна реалізація вимог Частини 2 (ICommand, ObservableCollection)
    public class TeachersViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        public ObservableCollection<Teacher> Teachers { get; } = new();

        public ICommand AddTeacherCommand { get; }
        public ICommand EditTeacherCommand { get; }
        public ICommand DeleteTeacherCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private Teacher _draggedTeacher;

        public TeachersViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            LoadTeachers();
            
            AddTeacherCommand = new Command(async () => 
            {
                await BaseViewModel.NavigationAction("TeacherEditPage?id=new");
            });
            
            EditTeacherCommand = new Command<Teacher>(async (teacher) => 
            {
                if (teacher != null)
                    await BaseViewModel.NavigationAction($"TeacherEditPage?id={teacher.Id}");
            });

            DeleteTeacherCommand = new Command<Teacher>((teacher) => 
            {
                if (teacher != null)
                {
                    Teachers.Remove(teacher);
                    _schedule.Teachers.Remove(teacher);
                }
            });

            DragStartingCommand = new Command<Teacher>(teacher => _draggedTeacher = teacher);
            
            DropCommand = new Command<Teacher>(teacher => 
            {
                if (_draggedTeacher != null && _draggedTeacher != teacher)
                {
                    var draggedIndex = Teachers.IndexOf(_draggedTeacher);
                    var dropIndex = Teachers.IndexOf(teacher);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Teachers.Move(draggedIndex, dropIndex);
                        SyncScheduleList();
                    }
                }
                _draggedTeacher = null;
            });
        }

        public void LoadTeachers()
        {
            Teachers.Clear();
            foreach (var t in _schedule.Teachers)
            {
                Teachers.Add(t);
            }
        }

        private void SyncScheduleList()
        {
            _schedule.Teachers.Clear();
            foreach (var t in Teachers)
            {
                _schedule.Teachers.Add(t);
            }
        }
    }
}
