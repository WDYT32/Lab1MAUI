using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule
{
    public class Discipline : IParticipant
    {
        public Guid Id { get; }
        public string Name { get; }

        public List<IConstraint> Constraints { get; } = new();

        private readonly int _totalCells;

        public Discipline(Guid id, string name, int totalCells)
        {
            Id = id;
            Name = name;
            _totalCells = totalCells;
        }

        public bool[] GetAvailabilityMask()
        {
            bool[] mask = new bool[_totalCells];
            Array.Fill(mask, true);

            foreach (var constraint in Constraints)
            {
                constraint.Apply(mask);
            }

            return mask;
        }

        public bool IsBooked(int cellIndex)
        {
            // Disciplines are conceptual and don't get "booked" like a room or teacher.
            return false;
        }
    }
}
