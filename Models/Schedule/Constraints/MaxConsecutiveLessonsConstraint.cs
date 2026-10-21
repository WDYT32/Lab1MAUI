using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class MaxConsecutiveLessonsConstraint(IParticipant owner, int maxConsecutive, int cellsPerDay) : IConstraint
    {
        public void Apply(bool[] mask)
        {
            int days = mask.Length / cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                int start = day * cellsPerDay;
                
                for (int i = 0; i < cellsPerDay; i++)
                {
                    int cellIndex = start + i;
                    if (!owner.IsBooked(cellIndex))
                    {
                        int prevConsecutive = 0;
                        for (int p = i - 1; p >= 0; p--)
                        {
                            if (owner.IsBooked(start + p)) prevConsecutive++;
                            else break;
                        }
                        
                        int nextConsecutive = 0;
                        for (int n = i + 1; n < cellsPerDay; n++)
                        {
                            if (owner.IsBooked(start + n)) nextConsecutive++;
                            else break;
                        }

                        if (prevConsecutive + 1 + nextConsecutive > maxConsecutive)
                        {
                            mask[cellIndex] = false;
                        }
                    }
                }
            }
        }
    }
}
