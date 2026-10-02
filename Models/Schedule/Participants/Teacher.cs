using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public class Teacher : IParticipant
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public int TargetHours { get; set; }
        
        private int _assignedHours;
        public int AssignedHours => _assignedHours;

        public List<TeacherCompetence> Competences { get; } = new();
        public List<IConstraint> Constraints { get; } = new();

        private readonly int _totalCells;
        private readonly bool[] _bookedCells;

        public Teacher(Guid id, string name, int totalCells)
        {
            Id = id;
            Name = name;
            _totalCells = totalCells;
            _bookedCells = new bool[totalCells];
        }

        public bool[] GetAvailabilityMask()
        {
            bool[] mask = new bool[_totalCells];
            Array.Fill(mask, true);

            foreach (var constraint in Constraints)
            {
                constraint.Apply(mask);
            }

            for (int i = 0; i < _totalCells; i++)
            {
                if (_bookedCells[i])
                {
                    mask[i] = false;
                }
            }

            return mask;
        }

        public bool IsBooked(int cellIndex)
        {
            return _bookedCells[cellIndex];
        }

        public bool CanTakeMoreHours()
        {
            return _assignedHours < TargetHours;
        }

        public bool CanTeach(Discipline disc)
        {
            return Competences.Any(c => c.DisciplineId == disc.Id);
        }

        public void BookSlot(int cellIndex)
        {
            if (_bookedCells[cellIndex])
                throw new InvalidOperationException($"Slot {cellIndex} is already booked for teacher {Name}");
            _bookedCells[cellIndex] = true;
            _assignedHours++;
        }

        public void ReleaseSlot(int cellIndex)
        {
            _bookedCells[cellIndex] = false;
            _assignedHours--;
        }

        public void ClearBookings()
        {
            Array.Fill(_bookedCells, false);
            _assignedHours = 0;
        }
    }
}
