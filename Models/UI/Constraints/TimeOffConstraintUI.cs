using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.UI.Constraints
{
    public class TimeOffConstraintUI : IConstraintUI
    {
        public int[] UnavailableCells { get; set; } = [];

        public string DisplayName =>
            $"Вихідний час: слоти [{string.Join(", ", UnavailableCells)}]";

        public IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new TimeOffConstraint(UnavailableCells);
    }
}
