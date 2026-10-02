using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.MTM
{
    /// <summary>
    /// MTM-клас для вимоги групи до кількості годин по дисципліні.
    /// </summary>
    public class GroupRequirementMTM
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
