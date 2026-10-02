using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.MTM
{
    
    
    
    
    
    public class StudentGroupMTM
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int StudentsCount { get; set; } = 25;

        public ObservableCollection<GroupRequirementMTM> Requirements { get; } = new();
        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();

        public StudentGroup MapToModel(ScheduleConfig config)
        {
            var group = new StudentGroup(Id, Name, config.TotalCells);
            group.StudentsCount = StudentsCount;

            foreach (var req in Requirements)
            {
                group.Requirements.Add(req.MapToModel());
            }

            foreach (var c in Constraints)
            {
                group.Constraints.Add(c.MapToModel(group, config));
            }

            return group;
        }
    }
}
