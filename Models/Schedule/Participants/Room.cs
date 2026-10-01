using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public class Room : IParticipant
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        
        private readonly int[] _reservedSeatsPerSlot;
        private readonly int _totalCells;

        public Room(Guid id, string name, int capacity, int totalCells)
        {
            Id = id;
            Name = name;
            Capacity = capacity;
            _totalCells = totalCells;
            _reservedSeatsPerSlot = new int[totalCells];
        }

        private int _currentRequestStudents;

        public void SetContext(int studentsCount)
        {
            _currentRequestStudents = studentsCount;
        }

        public bool[] GetAvailabilityMask()
        {
            bool[] mask = new bool[_totalCells];
            for (int i = 0; i < _totalCells; i++)
            {
                mask[i] = CanAccommodate(_currentRequestStudents, i);
            }
            return mask;
        }

        public bool IsBooked(int cellIndex)
        {
            return _reservedSeatsPerSlot[cellIndex] > 0;
        }

        public bool CanAccommodate(int studentsCount, int cellIndex)
        {
            return (_reservedSeatsPerSlot[cellIndex] + studentsCount) <= Capacity;
        }

        public void ReserveSeats(int studentsCount, int cellIndex)
        {
            if (!CanAccommodate(studentsCount, cellIndex))
                throw new InvalidOperationException($"Room {Name} capacity exceeded at slot {cellIndex}.");
            _reservedSeatsPerSlot[cellIndex] += studentsCount;
        }

        public void FreeSeats(int studentsCount, int cellIndex)
        {
            _reservedSeatsPerSlot[cellIndex] -= studentsCount;
        }
    }
}
