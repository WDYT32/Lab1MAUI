using Lab1MAUI.Models.UI.Constraints;
using Lab1MAUI.Models.Schedule;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels.Edit
{
    public enum ConstraintOwnerType
    {
        Teacher,
        Group,
        Discipline
    }

    public enum FormConstraintType
    {
        DailyLimit,
        MaxConsecutive,
        TimeOff
    }

    public class ConstraintTypeItem(FormConstraintType type, string displayName)
    {
        public FormConstraintType Type { get; } = type;
        public string DisplayName { get; } = displayName;

        public override string ToString() => DisplayName;
    }

    public class ConstraintFormViewModel : BaseViewModel
    {
        private readonly ConstraintOwnerType _ownerType;

        public event Action<IConstraintUI>? OnConstraintAdded;

        public ObservableCollection<ConstraintTypeItem> ConstraintTypes { get; } = new();

        public ConstraintTypeItem? SelectedConstraintType
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    OnPropertyChanged(nameof(IsDailyLimitSelected));
                    OnPropertyChanged(nameof(IsMaxConsecutiveSelected));
                    OnPropertyChanged(nameof(IsTimeOffSelected));
                }
            }
        }

        public bool IsDailyLimitSelected    => SelectedConstraintType?.Type == FormConstraintType.DailyLimit;
        public bool IsMaxConsecutiveSelected => SelectedConstraintType?.Type == FormConstraintType.MaxConsecutive;
        public bool IsTimeOffSelected        => SelectedConstraintType?.Type == FormConstraintType.TimeOff;

        public int DailyLimitValue
        {
            get;
            set => SetProperty(ref field, value);
        } = 3;

        public int MaxConsecutiveValue
        {
            get;
            set => SetProperty(ref field, value);
        } = 2;

        public int TimeOffCellIndex
        {
            get;
            set => SetProperty(ref field, value);
        } = 0;

        public ICommand AddConstraintCommand { get; }

        public ConstraintFormViewModel(ConstraintOwnerType ownerType)
        {
            _ownerType = ownerType;

            if (ownerType == ConstraintOwnerType.Teacher)
            {
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.DailyLimit, "Ліміт занять на день (DailyLimit)"));
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.MaxConsecutive, "Підрядні уроки (MaxConsecutive)"));
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.TimeOff, "Вихідний час (TimeOff)"));
            }
            else if (ownerType == ConstraintOwnerType.Group)
            {
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.DailyLimit, "Ліміт занять на день (DailyLimit)"));
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.MaxConsecutive, "Підрядні уроки (MaxConsecutive)"));
            }
            else if (ownerType == ConstraintOwnerType.Discipline)
            {
                ConstraintTypes.Add(new ConstraintTypeItem(FormConstraintType.DailyLimit, "Ліміт занять на день (DailyLimit)"));
            }

            AddConstraintCommand = new Command(() =>
            {
                if (SelectedConstraintType == null) return;

                IConstraintUI? newConstraint = SelectedConstraintType.Type switch
                {
                    FormConstraintType.DailyLimit => new DailyLimitConstraintUI { MaxLessonsPerDay = DailyLimitValue },
                    FormConstraintType.MaxConsecutive => new MaxConsecutiveLessonsConstraintUI { MaxConsecutive = MaxConsecutiveValue },
                    FormConstraintType.TimeOff => new TimeOffConstraintUI { UnavailableCells = [TimeOffCellIndex] },
                    _ => null
                };

                if (newConstraint != null)
                {
                    OnConstraintAdded?.Invoke(newConstraint);
                    SelectedConstraintType = null;
                }
            });
        }

        public void SelectType(FormConstraintType type)
        {
            SelectedConstraintType = ConstraintTypes.FirstOrDefault(c => c.Type == type);
        }

        public void Reset()
        {
            SelectedConstraintType = null;
            DailyLimitValue = 3;
            MaxConsecutiveValue = 2;
            TimeOffCellIndex = 0;
        }
    }
}
