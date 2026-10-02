using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public class DisciplinesViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;

        
        
        
        public ObservableCollection<DisciplineMTM> Disciplines => _scheduleMTM.Disciplines;

        public ICommand AddDisciplineCommand { get; }
        public ICommand EditDisciplineCommand { get; }
        public ICommand DeleteDisciplineCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private DisciplineMTM _draggedDiscipline;

        public DisciplinesViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            AddDisciplineCommand = new Command(async () =>
            {
                await BaseViewModel.NavigationAction("DisciplineEditPage?id=new");
            });

            EditDisciplineCommand = new Command<DisciplineMTM>(async (d) =>
            {
                if (d != null)
                    await BaseViewModel.NavigationAction($"DisciplineEditPage?id={d.Id}");
            });

            DeleteDisciplineCommand = new Command<DisciplineMTM>((d) =>
            {
                if (d != null)
                    _scheduleMTM.Disciplines.Remove(d);
            });

            DragStartingCommand = new Command<DisciplineMTM>(d => _draggedDiscipline = d);

            DropCommand = new Command<DisciplineMTM>(d =>
            {
                if (_draggedDiscipline != null && _draggedDiscipline != d)
                {
                    var from = Disciplines.IndexOf(_draggedDiscipline);
                    var to   = Disciplines.IndexOf(d);
                    if (from >= 0 && to >= 0)
                        Disciplines.Move(from, to);
                }
                _draggedDiscipline = null;
            });
        }
    }
}
