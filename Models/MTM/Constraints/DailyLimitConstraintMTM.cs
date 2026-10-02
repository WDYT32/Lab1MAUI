using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    public class DailyLimitConstraintMTM : ConstraintMTM
    {
        public int MaxLessonsPerDay { get; set; } = 3;

        public override string DisplayName => $"Ліміт на день: {MaxLessonsPerDay}";

        public override IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new DailyLimitConstraint(owner, MaxLessonsPerDay, config.CellsPerDay);
    }
}
