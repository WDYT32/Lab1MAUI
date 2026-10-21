using Lab1MAUI.Models.Schedule.Constraints;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public class Teacher(Guid id, string name, int totalCells) : IParticipant
    {
        public Guid Id { get; } = id;
        public string Name { get; set; } = name;
        public int TargetLessons { get; set; }
        
        private int _assignedLessons;
        public int AssignedLessons => _assignedLessons;

        public List<TeacherCompetence> Competences { get; } = new();
        public List<IConstraint> Constraints { get; } = new();

        private readonly bool[] _bookedCells = new bool[totalCells];

        public bool[] GetAvailabilityMask()
        {
            bool[] mask = new bool[totalCells];
            Array.Fill(mask, true);

            foreach (var constraint in Constraints)
            {
                constraint.Apply(mask);
            }

            for (int i = 0; i < totalCells; i++)
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

        public bool CanTakeMoreLessons()
        {
            return _assignedLessons < TargetLessons;
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
            _assignedLessons++;
        }

        public void ReleaseSlot(int cellIndex)
        {
            _bookedCells[cellIndex] = false;
            _assignedLessons--;
        }

        public void ClearBookings()
        {
            Array.Fill(_bookedCells, false);
            _assignedLessons = 0;
        }
    }
}
