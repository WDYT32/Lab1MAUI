using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.UI.Constraints
{
    public class DailyLimitConstraintUI : IConstraintUI
    {
        public int MaxLessonsPerDay { get; set; } = 3;

        public string DisplayName => $"Ліміт на день: {MaxLessonsPerDay}";

        public IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new DailyLimitConstraint(owner, MaxLessonsPerDay, config.CellsPerDay);
    }
}
