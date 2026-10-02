using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(DisciplineId), "id")]
    public class DisciplineEditViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;
        private DisciplineMTM _discipline;

        public string DisciplineId
        {
            set
            {
                if (value == "new")
                {
                    _discipline = new DisciplineMTM();
                    Name = string.Empty;
                    Constraints.Clear();
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _discipline = _scheduleMTM.Disciplines.FirstOrDefault(d => d.Id == id);
                    if (_discipline != null)
                    {
                        Name = _discipline.Name;
                        Constraints.Clear();
                        foreach (var c in _discipline.Constraints) Constraints.Add(c);
                    }
                }

                // Перестворюємо форму обмежень, оскільки тепер у нас є _discipline
                ConstraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Discipline, _scheduleMTM.Config);
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

        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();
        public ConstraintFormViewModel ConstraintForm { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand DeleteConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        private ConstraintMTM _draggedConstraint;

        public DisciplineEditViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            SaveCommand = new Command(async () =>
            {
                if (_discipline == null || string.IsNullOrWhiteSpace(Name)) return;

                _discipline.Name = Name;

                _discipline.Constraints.Clear();
                foreach (var c in Constraints) _discipline.Constraints.Add(c);

                if (!_scheduleMTM.Disciplines.Contains(_discipline))
                    _scheduleMTM.Disciplines.Add(_discipline);

                await BaseViewModel.NavigationAction("..");
            });

            DeleteConstraintCommand = new Command<ConstraintMTM>(c =>
            {
                if (c != null) Constraints.Remove(c);
            });

            DragStartingConstraintCommand = new Command<ConstraintMTM>(c => _draggedConstraint = c);

            DropConstraintCommand = new Command<ConstraintMTM>(c =>
            {
                if (_draggedConstraint != null && _draggedConstraint != c)
                {
                    var from = Constraints.IndexOf(_draggedConstraint);
                    var to   = Constraints.IndexOf(c);
                    if (from >= 0 && to >= 0)
                        Constraints.Move(from, to);
                }
                _draggedConstraint = null;
            });
        }
    }
}
