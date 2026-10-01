using System;

namespace Lab1MAUI.Models.Schedule
{
    public class GroupRequirement
    {
        public Guid DisciplineId { get; set; }
        public int RequiredHours { get; set; }
        public int FulfilledHours { get; set; }

        public bool IsFulfilled()
        {
            return FulfilledHours >= RequiredHours;
        }
    }
}
