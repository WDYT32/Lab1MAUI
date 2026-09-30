using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(DisciplineId), "id")]
    public class DisciplineEditViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        private Discipline _discipline;
        
        public string DisciplineId
        {
            set
            {
                if (value == "new")
                {
                    _discipline = new Discipline();
                    Name = "";
                    Constraints.Clear();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _discipline = _schedule.Disciplines.FirstOrDefault(d => d.Id == id);
                    if (_discipline != null)
                    {
                        Name = _discipline.Name;
                        Constraints.Clear();
                        foreach (var c in _discipline.Constraints) Constraints.Add(c);
                    }
                }
                
                // Перестворюємо форму обмежень, оскільки тепер у нас є _discipline
                ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Discipline, _discipline, _schedule);
                ConstraintForm.OnConstraintAdded += c => Constraints.Add(c);
                OnPropertyChanged(nameof(ConstraintForm));
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<IConstraint> Constraints { get; } = new();
        public ConstraintFormViewModel ConstraintForm { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private IConstraint _draggedConstraint;

        public DisciplineEditViewModel(Schedule schedule)
        {
            _schedule = schedule;

            SaveCommand = new Command(async () =>
            {
                if (_discipline == null || string.IsNullOrWhiteSpace(Name)) return;

                _discipline.Name = Name;
                
                _discipline.Constraints.Clear();
                _discipline.Constraints.AddRange(Constraints);

                if (!_schedule.Disciplines.Contains(_discipline))
                {
                    _schedule.Disciplines.Add(_discipline);
                }

                await BaseViewModel.NavigationAction("..");
            });

            DeleteConstraintCommand = new Command<IConstraint>(c =>
            {
                if (c != null) Constraints.Remove(c);
            });

            DragStartingConstraintCommand = new Command<IConstraint>(c => _draggedConstraint = c);
            
            DropConstraintCommand = new Command<IConstraint>(c => 
            {
                if (_draggedConstraint != null && _draggedConstraint != c)
                {
                    var draggedIndex = Constraints.IndexOf(_draggedConstraint);
                    var dropIndex = Constraints.IndexOf(c);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Constraints.Move(draggedIndex, dropIndex);
                    }
                }
                _draggedConstraint = null;
            });
        }
    }
}
