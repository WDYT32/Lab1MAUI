using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class SpecificTimeConstraint(int[] allowedCellsInDay, int cellsPerDay) : IConstraint
    {
        public void Apply(bool[] mask)
        {
            int days = mask.Length / cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                int start = day * cellsPerDay;
                for (int i = 0; i < cellsPerDay; i++)
                {
                    if (Array.IndexOf(allowedCellsInDay, i) < 0)
                    {
                        mask[start + i] = false;
                    }
                }
            }
        }
    }
}
