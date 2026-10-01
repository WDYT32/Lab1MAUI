using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public enum ConstraintOwnerType
    {
        Teacher,
        Group,
        Discipline
    }

    public class ConstraintFormViewModel : BaseViewModel
    {
        private readonly ConstraintOwnerType _ownerType;
        private readonly Schedule _schedule;
        private readonly object _owner;
        
        public event Action<IConstraint> OnConstraintAdded;

        public ConstraintFormViewModel(ConstraintOwnerType ownerType, object owner, Schedule schedule)
        {
            _ownerType = ownerType;
            _owner = owner;
            _schedule = schedule;

            foreach (var t in _schedule.Teachers) AvailableTeachers.Add(t);

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

                IConstraint newConstraint = null;
                var participantOwner = _owner as IParticipant;

                if (IsDailyLimitSelected && participantOwner != null)
                {
                    newConstraint = new DailyLimitConstraint(participantOwner, DailyLimitValue, _schedule.Config.CellsPerDay);
                }
                else if (IsMaxConsecutiveSelected && participantOwner != null)
                {
                    newConstraint = new MaxConsecutiveLessonsConstraint(participantOwner, MaxConsecutiveValue, _schedule.Config.CellsPerDay);
                }
                else if (IsTimeOffSelected)
                {
                    newConstraint = new TimeOffConstraint(new[] { TimeOffCellIndex });
                }
                
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
                    OnPropertyChanged(nameof(IsAssignmentSelected));
                }
            }
        }

        public bool IsDailyLimitSelected => _selectedConstraintType?.Contains("DailyLimit") == true;
        public bool IsMaxConsecutiveSelected => _selectedConstraintType?.Contains("MaxConsecutive") == true;
        public bool IsTimeOffSelected => _selectedConstraintType?.Contains("TimeOff") == true;
        public bool IsAssignmentSelected => _selectedConstraintType?.Contains("Assignment") == true;

        // Поля для DailyLimit
        private int _dailyLimitValue = 3;
        public int DailyLimitValue { get => _dailyLimitValue; set => SetProperty(ref _dailyLimitValue, value); }

        // Поля для MaxConsecutive
        private int _maxConsecutiveValue = 2;
        public int MaxConsecutiveValue { get => _maxConsecutiveValue; set => SetProperty(ref _maxConsecutiveValue, value); }

        // Поля для TimeOff
        private int _timeOffCellIndex = 0;
        public int TimeOffCellIndex { get => _timeOffCellIndex; set => SetProperty(ref _timeOffCellIndex, value); }

        public ObservableCollection<Teacher> AvailableTeachers { get; } = new();

        private Teacher _selectedAssignmentTeacher;
        public Teacher SelectedAssignmentTeacher
        {
            get => _selectedAssignmentTeacher;
            set => SetProperty(ref _selectedAssignmentTeacher, value);
        }

        public ICommand AddConstraintCommand { get; }
    }
}
