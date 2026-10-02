using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    
    public class TeachersViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;

        
        
        
        public ObservableCollection<TeacherMTM> Teachers => _scheduleMTM.Teachers;

        public ICommand AddTeacherCommand { get; }
        public ICommand EditTeacherCommand { get; }
        public ICommand DeleteTeacherCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private TeacherMTM _draggedTeacher;

        public TeachersViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            AddTeacherCommand = new Command(async () =>
            {
                await BaseViewModel.NavigationAction("TeacherEditPage?id=new");
            });

            EditTeacherCommand = new Command<TeacherMTM>(async (teacher) =>
            {
                if (teacher != null)
                    await BaseViewModel.NavigationAction($"TeacherEditPage?id={teacher.Id}");
            });

            DeleteTeacherCommand = new Command<TeacherMTM>((teacher) =>
            {
                if (teacher != null)
                    _scheduleMTM.Teachers.Remove(teacher);
            });

            DragStartingCommand = new Command<TeacherMTM>(t => _draggedTeacher = t);

            DropCommand = new Command<TeacherMTM>(t =>
            {
                if (_draggedTeacher != null && _draggedTeacher != t)
                {
                    var from = Teachers.IndexOf(_draggedTeacher);
                    var to   = Teachers.IndexOf(t);
                    if (from >= 0 && to >= 0)
                        Teachers.Move(from, to);
                }
                _draggedTeacher = null;
            });
        }
    }
}
