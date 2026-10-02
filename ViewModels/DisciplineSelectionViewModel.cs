using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public class DisciplineSelectionViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;
        public ObservableCollection<DisciplineMTM> FilteredDisciplines { get; } = new();

        public event Action<DisciplineMTM> OnDisciplineSelected;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    FilterDisciplines();
            }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ICommand SelectCommand { get; }
        public ICommand CancelCommand { get; }

        public DisciplineSelectionViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            SelectCommand = new Command<DisciplineMTM>(d =>
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
                ? _scheduleMTM.Disciplines
                : _scheduleMTM.Disciplines.Where(d =>
                    d.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var d in results) FilteredDisciplines.Add(d);
        }
    }
}
