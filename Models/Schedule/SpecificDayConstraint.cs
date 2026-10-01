using System;

namespace Lab1MAUI.Models.Schedule
{
    public class SpecificDayConstraint : IConstraint
    {
        private readonly int[] _allowedDays;
        private readonly int _cellsPerDay;

        public SpecificDayConstraint(int[] allowedDays, int cellsPerDay)
        {
            _allowedDays = allowedDays;
            _cellsPerDay = cellsPerDay;
        }

        public void Apply(bool[] mask)
        {
            int days = mask.Length / _cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                bool isAllowed = Array.IndexOf(_allowedDays, day) >= 0;
                if (!isAllowed)
                {
                    int start = day * _cellsPerDay;
                    for (int i = 0; i < _cellsPerDay; i++)
                    {
                        mask[start + i] = false;
                    }
                }
            }
        }
    }
}
