using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.MTM
{
    /// <summary>
    /// MTM-клас для зв'язку викладач ↔ дисципліна (компетенція).
    /// Зберігає лише ідентифікатор дисципліни; назва резолвиться через ScheduleMTM.Disciplines.
    /// </summary>
    public class TeacherCompetenceMTM
    {
        public Guid DisciplineId { get; set; }

        public TeacherCompetence MapToModel()
            => new TeacherCompetence { DisciplineId = DisciplineId };
    }
}
