using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public class StudentGroup : IParticipant
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public int StudentsCount { get; set; }

        public List<GroupRequirement> Requirements { get; } = new();
        public List<IConstraint> Constraints { get; } = new();
        
        private readonly int _totalCells;
        private readonly bool[] _bookedCells;

        public StudentGroup(Guid id, string name, int totalCells)
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

        public void BookSlot(int cellIndex)
        {
            if (_bookedCells[cellIndex])
                throw new InvalidOperationException($"Slot {cellIndex} is already booked for group {Name}");
            _bookedCells[cellIndex] = true;
        }

        public void ReleaseSlot(int cellIndex)
        {
            _bookedCells[cellIndex] = false;
        }

        public void ClearBookings()
        {
            Array.Fill(_bookedCells, false);
        }

        public List<GroupRequirement> GetUnfulfilledRequirements()
        {
            return Requirements.Where(r => !r.IsFulfilled()).ToList();
        }
    }
}
