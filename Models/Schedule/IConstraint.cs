namespace Lab1MAUI.Models.Schedule
{
    public interface IConstraint
    {
        void Apply(bool[] mask);
    }
}
