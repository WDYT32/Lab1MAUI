using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public class DisciplineSelectionViewModel : BaseViewModel
    {
        private readonly ScheduleUI _scheduleUI;
        public ObservableCollection<DisciplineUI> FilteredDisciplines { get; } = new();

        public event Action<DisciplineUI>? OnDisciplineSelected;

        public string SearchQuery
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                    FilterDisciplines();
            }
        } = string.Empty;

        public bool IsVisible
        {
            get;
            set => SetProperty(ref field, value);
        }

        public ICommand SelectCommand { get; }
        public ICommand CancelCommand { get; }

        public DisciplineSelectionViewModel(ScheduleUI scheduleUI)
        {
            _scheduleUI = scheduleUI;

            SelectCommand = new Command<DisciplineUI>(d =>
            {
                if (d != null)
                {
                    OnDisciplineSelected?.Invoke(d);
                    IsVisible = false;
                }
            });

            CancelCommand = new Command(() => IsVisible = false);
        }

        public void Open()
        {
            SearchQuery = string.Empty;
            FilterDisciplines();
            IsVisible = true;
        }

        private void FilterDisciplines()
        {
            FilteredDisciplines.Clear();
            var results = string.IsNullOrWhiteSpace(SearchQuery)
                ? _scheduleUI.Disciplines
                : _scheduleUI.Disciplines.Where(d =>
                    d.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var d in results) FilteredDisciplines.Add(d);
        }
    }
}
