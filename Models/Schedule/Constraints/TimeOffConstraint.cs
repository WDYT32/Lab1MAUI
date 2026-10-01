using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class TimeOffConstraint : IConstraint
    {
        private readonly int[] _unavailableCells;

        public TimeOffConstraint(int[] unavailableCells)
        {
            _unavailableCells = unavailableCells;
        }

        public void Apply(bool[] mask)
        {
            foreach (var cell in _unavailableCells)
            {
                if (cell >= 0 && cell < mask.Length)
                    mask[cell] = false;
            }
        }
    }
}
