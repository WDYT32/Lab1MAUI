using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.UI.Constraints
{
    public class MaxConsecutiveLessonsConstraintUI : IConstraintUI
    {
        public int MaxConsecutive { get; set; } = 2;

        public string DisplayName => $"Підрядні уроки: макс {MaxConsecutive}";

        public IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new MaxConsecutiveLessonsConstraint(owner, MaxConsecutive, config.CellsPerDay);
    }
}
