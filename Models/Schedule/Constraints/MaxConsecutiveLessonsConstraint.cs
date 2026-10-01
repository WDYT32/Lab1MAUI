using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class MaxConsecutiveLessonsConstraint : IConstraint
    {
        private readonly IParticipant _owner;
        private readonly int _maxConsecutive;
        private readonly int _cellsPerDay;

        public MaxConsecutiveLessonsConstraint(IParticipant owner, int maxConsecutive, int cellsPerDay)
        {
            _owner = owner;
            _maxConsecutive = maxConsecutive;
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
                    int cellIndex = start + i;
                    if (!_owner.IsBooked(cellIndex))
                    {
                        int prevConsecutive = 0;
                        for (int p = i - 1; p >= 0; p--)
                        {
                            if (_owner.IsBooked(start + p)) prevConsecutive++;
                            else break;
                        }
                        
                        int nextConsecutive = 0;
                        for (int n = i + 1; n < _cellsPerDay; n++)
                        {
                            if (_owner.IsBooked(start + n)) nextConsecutive++;
                            else break;
                        }

                        if (prevConsecutive + 1 + nextConsecutive > _maxConsecutive)
                        {
                            mask[cellIndex] = false;
                        }
                    }
                }
            }
        }
    }
}
