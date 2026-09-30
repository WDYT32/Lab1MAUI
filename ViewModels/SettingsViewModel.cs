using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;

        public int Days
        {
            get => _schedule.Config.Days;
            set
            {
                if (_schedule.Config.Days != value)
                {
                    _schedule.Config.Days = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }

        public int CellsPerDay
        {
            get => _schedule.Config.CellsPerDay;
            set
            {
                if (_schedule.Config.CellsPerDay != value)
                {
                    _schedule.Config.CellsPerDay = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }
        
        public int TotalCells => _schedule.Config.TotalCells;

        public SettingsViewModel(Schedule schedule)
        {
            _schedule = schedule;
        }
    }
}
