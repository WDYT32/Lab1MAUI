using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class DailyLimitConstraint : IConstraint
    {
        private readonly IParticipant _owner;
        private readonly int _maxLessonsPerDay;
        private readonly int _cellsPerDay;

        public DailyLimitConstraint(IParticipant owner, int maxLessonsPerDay, int cellsPerDay)
        {
            _owner = owner;
            _maxLessonsPerDay = maxLessonsPerDay;
            _cellsPerDay = cellsPerDay;
        }

        public void Apply(bool[] mask)
        {
            int days = mask.Length / _cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                int start = day * _cellsPerDay;
                
                int bookedToday = 0;
                for (int i = 0; i < _cellsPerDay; i++)
                {
                    if (_owner.IsBooked(start + i))
                        bookedToday++;
                }

                if (bookedToday >= _maxLessonsPerDay)
                {
                    for (int i = 0; i < _cellsPerDay; i++)
                    {
                        if (!_owner.IsBooked(start + i))
                            mask[start + i] = false;
                    }
                }
            }
        }
    }
}
