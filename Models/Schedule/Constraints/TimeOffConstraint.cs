using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class TimeOffConstraint(int[] unavailableCells) : IConstraint
    {
        public void Apply(bool[] mask)
        {
            foreach (var cell in unavailableCells)
            {
                if (cell >= 0 && cell < mask.Length)
                    mask[cell] = false;
            }
        }
    }
}
