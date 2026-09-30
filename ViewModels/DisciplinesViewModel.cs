using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public class DisciplinesViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        
        public ObservableCollection<Discipline> Disciplines { get; } = new();

        private string _newDisciplineName = string.Empty;
        public string NewDisciplineName
        {
            get => _newDisciplineName;
            set
            {
                if (SetProperty(ref _newDisciplineName, value))
                    ((Command)AddDisciplineCommand).ChangeCanExecute();
            }
        }

        public ICommand AddDisciplineCommand { get; }
        public ICommand EditDisciplineCommand { get; }
        public ICommand DeleteDisciplineCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private Discipline _draggedDiscipline;

        public DisciplinesViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            LoadDisciplines();
            
            AddDisciplineCommand = new Command(async () => 
            {
                await BaseViewModel.NavigationAction("DisciplineEditPage?id=new");
            });
            
            EditDisciplineCommand = new Command<Discipline>(async (d) => 
            {
                if (d != null)
                {
                    await BaseViewModel.NavigationAction($"DisciplineEditPage?id={d.Id}");
                }
            });
            
            DeleteDisciplineCommand = new Command<Discipline>((d) => 
            {
                if (d != null)
                {
                    Disciplines.Remove(d);
                    _schedule.Disciplines.Remove(d);
                }
            });

            DragStartingCommand = new Command<Discipline>(d => _draggedDiscipline = d);
            
            DropCommand = new Command<Discipline>(d => 
            {
                if (_draggedDiscipline != null && _draggedDiscipline != d)
                {
                    var draggedIndex = Disciplines.IndexOf(_draggedDiscipline);
                    var dropIndex = Disciplines.IndexOf(d);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Disciplines.Move(draggedIndex, dropIndex);
                        SyncScheduleList();
                    }
                }
                _draggedDiscipline = null;
            });
        }

        private void SyncScheduleList()
        {
            _schedule.Disciplines.Clear();
            foreach (var d in Disciplines)
            {
                _schedule.Disciplines.Add(d);
            }
        }

        public void LoadDisciplines()
        {
            Disciplines.Clear();
            foreach (var d in _schedule.Disciplines)
            {
                Disciplines.Add(d);
            }
        }
    }
}
