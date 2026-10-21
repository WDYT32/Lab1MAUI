using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.UI.Constraints
{
    public interface IConstraintUI
    {
        string DisplayName { get; }

        IConstraint MapToModel(IParticipant owner, ScheduleConfig config);
    }
}
