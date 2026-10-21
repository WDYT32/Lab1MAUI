using Lab1MAUI.Models.Schedule;
using System;

namespace Lab1MAUI.Models.UI
{
    public class TeacherCompetenceUI
    {
        public Guid DisciplineId { get; set; }

        public TeacherCompetence MapToModel()
            => new() { DisciplineId = DisciplineId };
    }
}
