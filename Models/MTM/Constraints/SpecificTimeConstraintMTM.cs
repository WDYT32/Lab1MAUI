using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    public class SpecificTimeConstraintMTM : ConstraintMTM
    {
        /// <summary>
        /// Індекси дозволених слотів всередині дня (0-based).
        /// </summary>
        public int[] AllowedCellsInDay { get; set; } = [];

        public override string DisplayName =>
            $"Тільки слоти в дні: [{string.Join(", ", AllowedCellsInDay)}]";

        public override IConstraint MapToModel(IParticipant owner, ScheduleConfig config)
            => new SpecificTimeConstraint(AllowedCellsInDay, config.CellsPerDay);
    }
}
