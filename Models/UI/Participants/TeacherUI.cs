using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.UI.Participants
{
    public class TeacherUI : IParticipantUI
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;

        public string Name
        {
            get
            {
                var full = $"{LastName} {FirstName} {Patronymic}".Trim();
                return string.IsNullOrWhiteSpace(full) ? field : full;
            }
            set
            {
                field = value ?? string.Empty;
                var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                LastName = parts.Length > 0 ? parts[0] : string.Empty;
                FirstName = parts.Length > 1 ? parts[1] : string.Empty;
                Patronymic = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : string.Empty;
            }
        } = string.Empty;

        public int TargetHours { get; set; } = 10;

        public ObservableCollection<TeacherCompetenceUI> Competences { get; } = new();
        public ObservableCollection<IConstraintUI> Constraints { get; } = new();

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
