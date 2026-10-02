using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.MTM
{
    /// <summary>
    /// MTM-клас для дисципліни. Є єдиним джерелом правди для UI.
    /// При генерації MapToModel() створює новий доменний об'єкт Discipline.
    /// </summary>
    public class DisciplineMTM
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<ConstraintMTM> Constraints { get; } = new();

        public Discipline MapToModel(ScheduleConfig config)
        {
            var discipline = new Discipline(Id, Name, config.TotalCells);

            foreach (var c in Constraints)
            {
                discipline.Constraints.Add(c.MapToModel(discipline, config));
            }

            return discipline;
        }
    }
}
