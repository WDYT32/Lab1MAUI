using Lab1MAUI.Models.Schedule;
using System;

namespace Lab1MAUI.Models.UI
{
    public class GroupRequirementUI
    {
        public Guid DisciplineId { get; set; }
        public int RequiredHours { get; set; } = 2;

        public GroupRequirement MapToModel()
            => new GroupRequirement
            {
                DisciplineId = DisciplineId,
                RequiredHours = RequiredHours,
                FulfilledHours = 0
            };
    }
}
