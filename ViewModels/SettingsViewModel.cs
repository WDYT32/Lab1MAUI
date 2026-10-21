using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ScheduleUI _scheduleUI;
        private ScheduleConfig Config => _scheduleUI.Config;

        private int _days;
        public int Days
        {
            get => _days;
            set
            {
                if (SetProperty(ref _days, value))
                {
                    Config.Days = value;
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }

        private int _cellsPerDay;
        public int CellsPerDay
        {
            get => _cellsPerDay;
            set
            {
                if (SetProperty(ref _cellsPerDay, value))
                {
                    Config.CellsPerDay = value;
                    OnPropertyChanged(nameof(TotalCells));
                }
            }
        }

        private int _minLessonsPerDay;
        public int MinLessonsPerDay
        {
            get => _minLessonsPerDay;
            set
            {
                if (SetProperty(ref _minLessonsPerDay, value))
                {
                    Config.MinLessonsPerDay = value;
                }
            }
        }

        private int _lessonDurationMinutes;
        public int LessonDurationMinutes
        {
            get => _lessonDurationMinutes;
            set
            {
                if (SetProperty(ref _lessonDurationMinutes, value))
                {
                    Config.LessonDurationMinutes = value;
                }
            }
        }

        public int TotalCells => Config.TotalCells;

        public SettingsViewModel(ScheduleUI scheduleUI)
        {
            _scheduleUI = scheduleUI;
            _days = Config.Days;
            _cellsPerDay = Config.CellsPerDay;
            _minLessonsPerDay = Config.MinLessonsPerDay;
            _lessonDurationMinutes = Config.LessonDurationMinutes;
        }
    }
}
