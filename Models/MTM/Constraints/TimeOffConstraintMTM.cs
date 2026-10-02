using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    public class TimeOffConstraintMTM : ConstraintMTM
    {
        /// <summary>
        /// Глобальні індекси слотів, що є недоступними.
        /// </summary>
        public int[] UnavailableCells { get; set; } = [];

        public override string DisplayName =>
            $"Вихідний час: слоти [{string.Join(", ", UnavailableCells)}]";

        public override IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new TimeOffConstraint(UnavailableCells);
    }
}
