using Lab1MAUI.Models.Schedule.Participants;
namespace Lab1MAUI.Models.Schedule.Constraints
{
    public interface IConstraint
    {
        void Apply(bool[] mask);
    }
}
