using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public enum ConstraintOwnerType
    {
        Teacher,
        Group,
        Discipline
    }

    /// <summary>
    /// ViewModel форми додавання обмеження. Створює ConstraintMTM-об'єкти.
    /// Власник (IParticipant) більше не потрібен тут — він буде призначений
    /// під час MapToModel() при генерації розкладу.
    /// </summary>
    public class ConstraintFormViewModel : BaseViewModel
    {
        private readonly ConstraintOwnerType _ownerType;
        private readonly ScheduleConfig _config;

        public event Action<ConstraintMTM> OnConstraintAdded;

        public ConstraintFormViewModel(ConstraintOwnerType ownerType, ScheduleConfig config)
        {
            _ownerType = ownerType;
            _config = config;

            if (ownerType == ConstraintOwnerType.Teacher)
            {
                ConstraintTypes.Add("Ліміт занять на день (DailyLimit)");
                ConstraintTypes.Add("Підрядні уроки (MaxConsecutive)");
                ConstraintTypes.Add("Вихідний час (TimeOff)");
            }
            else if (ownerType == ConstraintOwnerType.Group)
            {
                ConstraintTypes.Add("Ліміт занять на день (DailyLimit)");
                ConstraintTypes.Add("Підрядні уроки (MaxConsecutive)");
            }
            else if (ownerType == ConstraintOwnerType.Discipline)
            {
                ConstraintTypes.Add("Ліміт занять на день (DailyLimit)");
            }

            AddConstraintCommand = new Command(() =>
            {
                if (string.IsNullOrEmpty(SelectedConstraintType)) return;

                ConstraintMTM newConstraint = null;

                if (IsDailyLimitSelected)
                    newConstraint = new DailyLimitConstraintMTM { MaxLessonsPerDay = DailyLimitValue };
                else if (IsMaxConsecutiveSelected)
                    newConstraint = new MaxConsecutiveLessonsConstraintMTM { MaxConsecutive = MaxConsecutiveValue };
                else if (IsTimeOffSelected)
                    newConstraint = new TimeOffConstraintMTM { UnavailableCells = [TimeOffCellIndex] };

                if (newConstraint != null)
                {
                    OnConstraintAdded?.Invoke(newConstraint);
                    SelectedConstraintType = null;
                }
            });
        }

        public ObservableCollection<string> ConstraintTypes { get; } = new();

        private string _selectedConstraintType;
        public string SelectedConstraintType
        {
            get => _selectedConstraintType;
            set
            {
                if (SetProperty(ref _selectedConstraintType, value))
                {
                    OnPropertyChanged(nameof(IsDailyLimitSelected));
                    OnPropertyChanged(nameof(IsMaxConsecutiveSelected));
                    OnPropertyChanged(nameof(IsTimeOffSelected));
                }
            }
        }

        public bool IsDailyLimitSelected    => _selectedConstraintType?.Contains("DailyLimit")    == true;
        public bool IsMaxConsecutiveSelected => _selectedConstraintType?.Contains("MaxConsecutive") == true;
        public bool IsTimeOffSelected        => _selectedConstraintType?.Contains("TimeOff")        == true;

        // Поля для DailyLimit
        private int _dailyLimitValue = 3;
        public int DailyLimitValue { get => _dailyLimitValue; set => SetProperty(ref _dailyLimitValue, value); }

        // Поля для MaxConsecutive
        private int _maxConsecutiveValue = 2;
        public int MaxConsecutiveValue { get => _maxConsecutiveValue; set => SetProperty(ref _maxConsecutiveValue, value); }

        // Поля для TimeOff
        private int _timeOffCellIndex = 0;
        public int TimeOffCellIndex { get => _timeOffCellIndex; set => SetProperty(ref _timeOffCellIndex, value); }

        public ICommand AddConstraintCommand { get; }
    }
}
