using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.UI.Constraints;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public class ConstraintsEditorViewModel : BaseViewModel
    {
        private IConstraintUI? _draggedConstraint;

        public ObservableCollection<IConstraintUI> Constraints { get; } = new();

        public ConstraintFormViewModel ConstraintForm { get; }

        public ICommand DeleteConstraintCommand { get; }
        public ICommand DragStartingConstraintCommand { get; }
        public ICommand DropConstraintCommand { get; }

        public ConstraintsEditorViewModel(ConstraintOwnerType ownerType, ScheduleConfig config)
        {
            ConstraintForm = new ConstraintFormViewModel(ownerType, config);
            ConstraintForm.OnConstraintAdded += c => Constraints.Add(c);

            DeleteConstraintCommand = new Command<IConstraintUI>(c =>
            {
                if (c != null)
                    Constraints.Remove(c);
            });

            DragStartingConstraintCommand = new Command<IConstraintUI>(c => _draggedConstraint = c);

            DropConstraintCommand = new Command<IConstraintUI>(c =>
            {
                if (_draggedConstraint != null && _draggedConstraint != c)
                {
                    var from = Constraints.IndexOf(_draggedConstraint);
                    var to = Constraints.IndexOf(c);
                    if (from >= 0 && to >= 0)
                        Constraints.Move(from, to);
                }
                _draggedConstraint = null;
            });
        }

        public void LoadFrom(IEnumerable<IConstraintUI> constraints)
        {
            Constraints.Clear();
            foreach (var c in constraints)
                Constraints.Add(c);

            ConstraintForm.Reset();
        }

        public void ApplyTo(ICollection<IConstraintUI> target)
        {
            target.Clear();
            foreach (var c in Constraints)
                target.Add(c);
        }
    }
}
