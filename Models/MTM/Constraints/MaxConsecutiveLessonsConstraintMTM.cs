using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    public class MaxConsecutiveLessonsConstraintMTM : ConstraintMTM
    {
        public int MaxConsecutive { get; set; } = 2;

        public override string DisplayName => $"Підрядні уроки: макс {MaxConsecutive}";

        public override IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new MaxConsecutiveLessonsConstraint(owner, MaxConsecutive, config.CellsPerDay);
    }
}
