using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class DailyLimitConstraint(IParticipant owner, int maxLessonsPerDay, int cellsPerDay) : IConstraint
    {
        public void Apply(bool[] mask)
        {
            int days = mask.Length / cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                int start = day * cellsPerDay;
                
                int bookedToday = 0;
                for (int i = 0; i < cellsPerDay; i++)
                {
                    if (owner.IsBooked(start + i))
                        bookedToday++;
                }

                if (bookedToday >= maxLessonsPerDay)
                {
                    for (int i = 0; i < cellsPerDay; i++)
                    {
                        if (!owner.IsBooked(start + i))
                            mask[start + i] = false;
                    }
                }
            }
        }
    }
}
