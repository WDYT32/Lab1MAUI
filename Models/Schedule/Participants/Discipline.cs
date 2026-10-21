using Lab1MAUI.Models.Schedule.Constraints;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public class Discipline(Guid id, string name, int totalCells) : IParticipant
    {
        public Guid Id { get; } = id;
        public string Name { get; set; } = name;

        public List<IConstraint> Constraints { get; } = new();

        private readonly Dictionary<Guid, bool[]> _bookedCellsPerGroup = new();
        private Guid _currentContextGroupId;

        public void SetContext(Guid groupId)
        {
            _currentContextGroupId = groupId;
            if (!_bookedCellsPerGroup.ContainsKey(groupId))
            {
                _bookedCellsPerGroup[groupId] = new bool[totalCells];
            }
        }

        public void BookSlot(int cellIndex, Guid groupId)
        {
            if (!_bookedCellsPerGroup.ContainsKey(groupId))
                _bookedCellsPerGroup[groupId] = new bool[totalCells];
            _bookedCellsPerGroup[groupId][cellIndex] = true;
        }

        public void ReleaseSlot(int cellIndex, Guid groupId)
        {
            if (_bookedCellsPerGroup.ContainsKey(groupId))
                _bookedCellsPerGroup[groupId][cellIndex] = false;
        }

        public void ClearBookings()
        {
            _bookedCellsPerGroup.Clear();
        }

        public bool[] GetAvailabilityMask()
        {
            bool[] mask = new bool[totalCells];
            Array.Fill(mask, true);

            foreach (var constraint in Constraints)
            {
                constraint.Apply(mask);
            }

            
            if (_currentContextGroupId != Guid.Empty && _bookedCellsPerGroup.TryGetValue(_currentContextGroupId, out var booked))
            {
                for (int i = 0; i < totalCells; i++)
                {
                    if (booked[i]) mask[i] = false;
                }
            }

            return mask;
        }

        public bool IsBooked(int cellIndex)
        {
            if (_currentContextGroupId != Guid.Empty && _bookedCellsPerGroup.TryGetValue(_currentContextGroupId, out var booked))
            {
                return booked[cellIndex];
            }
            return false;
        }
    }
}
