using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class SpecificTimeConstraint : IConstraint
    {
        private readonly int[] _allowedCellsInDay;
        private readonly int _cellsPerDay;

        public SpecificTimeConstraint(int[] allowedCellsInDay, int cellsPerDay)
        {
            _allowedCellsInDay = allowedCellsInDay;
            _cellsPerDay = cellsPerDay;
        }

        public void Apply(bool[] mask)
        {
            int days = mask.Length / _cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                int start = day * _cellsPerDay;
                for (int i = 0; i < _cellsPerDay; i++)
                {
                    if (Array.IndexOf(_allowedCellsInDay, i) < 0)
                    {
                        mask[start + i] = false;
                    }
                }
            }
        }
    }
}
