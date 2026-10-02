using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.MTM
{
    
    
    
    
    public class TeacherCompetenceMTM
    {
        public Guid DisciplineId { get; set; }

        public TeacherCompetence MapToModel()
            => new TeacherCompetence { DisciplineId = DisciplineId };
    }
}
