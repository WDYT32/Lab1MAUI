using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;

        public int Days
        {
            get => _scheduleMTM.Config.Days;
            set
            {
                if (_scheduleMTM.Config.Days != value)
                {
                    _scheduleMTM.Config.Days = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }

        public int CellsPerDay
        {
            get => _scheduleMTM.Config.CellsPerDay;
            set
            {
                if (_scheduleMTM.Config.CellsPerDay != value)
                {
                    _scheduleMTM.Config.CellsPerDay = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }

        public int MinLessonsPerDay
        {
            get => _scheduleMTM.Config.MinLessonsPerDay;
            set
            {
                if (_scheduleMTM.Config.MinLessonsPerDay != value)
                {
                    _scheduleMTM.Config.MinLessonsPerDay = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LessonDurationMinutes
        {
            get => _scheduleMTM.Config.LessonDurationMinutes;
            set
            {
                if (_scheduleMTM.Config.LessonDurationMinutes != value)
                {
                    _scheduleMTM.Config.LessonDurationMinutes = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalCells => _scheduleMTM.Config.TotalCells;

        public SettingsViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;
        }
    }
}
