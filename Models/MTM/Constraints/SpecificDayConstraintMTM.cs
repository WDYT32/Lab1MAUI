using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    public class SpecificDayConstraintMTM : ConstraintMTM
    {
        
        
        
        public int[] AllowedDays { get; set; } = [];

        public override string DisplayName =>
            $"Тільки дні: [{string.Join(", ", AllowedDays)}]";

        public override IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new SpecificDayConstraint(AllowedDays, config.CellsPerDay);
    }
}
