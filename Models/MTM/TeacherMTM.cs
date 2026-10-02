using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.MTM
{
    
    
    
    
    
    public class TeacherMTM
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        
        
        
        public string Name { get; set; } = string.Empty;

        public int TargetHours { get; set; } = 10;

        public ObservableCollection<TeacherCompetenceMTM> Competences { get; } = new();
        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();

        public Teacher MapToModel(ScheduleConfig config)
        {
            var teacher = new Teacher(Id, Name, config.TotalCells);
            teacher.TargetHours = TargetHours;

            foreach (var comp in Competences)
            {
                teacher.Competences.Add(comp.MapToModel());
            }

            foreach (var c in Constraints)
            {
                teacher.Constraints.Add(c.MapToModel(teacher, config));
            }

            return teacher;
        }
    }
}
