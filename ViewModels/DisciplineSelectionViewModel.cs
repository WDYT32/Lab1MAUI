using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public class DisciplineSelectionViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        public ObservableCollection<Discipline> FilteredDisciplines { get; } = new();

        public event Action<Discipline> OnDisciplineSelected;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterDisciplines();
                }
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

        public DisciplineSelectionViewModel(Schedule schedule)
        {
            _schedule = schedule;

            SelectCommand = new Command<Discipline>(d =>
            {
                if (d != null)
                {
                    OnDisciplineSelected?.Invoke(d);
                    IsVisible = false;
                }
            });

            CancelCommand = new Command(() => 
            {
                IsVisible = false;
            });
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
                ? _schedule.Disciplines 
                : _schedule.Disciplines.Where(d => d.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                
            foreach (var r in results) FilteredDisciplines.Add(r);
        }
    }
}
