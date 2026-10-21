using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.Schedule
{
    public class Schedule
    {
        public ScheduleConfig Config { get; }
        
        private readonly List<Lesson>[] _grid;
        public IReadOnlyList<IReadOnlyList<Lesson>> Grid => _grid;

        public List<Teacher> Teachers { get; } = new();
        public List<StudentGroup> Groups { get; } = new();
        public List<Discipline> Disciplines { get; } = new();
        public List<Room> Rooms { get; } = new();

        public Schedule(ScheduleConfig config)
        {
            Config = config;
            _grid = new List<Lesson>[config.TotalCells];
            for (int i = 0; i < config.TotalCells; i++)
            {
                _grid[i] = new List<Lesson>();
            }
        }

        private int _maxPlaced = -1;
        private List<Lesson> _bestLessons = new();

        public void Generate()
        {
            _maxPlaced = -1;
            _bestLessons.Clear();

            if (!GenerateRecursive(0))
            {
                
                foreach (var c in _grid) c.Clear();
                foreach (var g in Groups) { g.ClearBookings(); foreach (var req in g.Requirements) req.FulfilledHours = 0; }
                foreach (var t in Teachers) { t.ClearBookings(); }
                foreach (var r in Rooms) { r.ClearBookings(); }
                foreach (var d in Disciplines) { d.ClearBookings(); }

                foreach (var lesson in _bestLessons)
                {
                    var g = Groups.First(x => x.Id == lesson.StudentGroupId);
                    var t = Teachers.First(x => x.Id == lesson.TeacherId);
                    var r = Rooms.First(x => x.Id == lesson.RoomId);
                    var d = Disciplines.First(x => x.Id == lesson.DisciplineId);

                    g.BookSlot(lesson.CellIndex);
                    t.BookSlot(lesson.CellIndex);
                    r.ReserveSeats(g.StudentsCount, lesson.CellIndex);
                    d.BookSlot(lesson.CellIndex, g.Id);

                    g.Requirements.First(req => req.DisciplineId == d.Id).FulfilledHours++;
                    _grid[lesson.CellIndex].Add(lesson);
                }
            }
        }

        private bool GenerateRecursive(int placedCount)
        {
            if (placedCount > _maxPlaced)
            {
                _maxPlaced = placedCount;
                _bestLessons = _grid.SelectMany(c => c).Select(l => new Lesson 
                { 
                    Id = l.Id, DisciplineId = l.DisciplineId, TeacherId = l.TeacherId, 
                    StudentGroupId = l.StudentGroupId, RoomId = l.RoomId, CellIndex = l.CellIndex 
                }).ToList();
            }

            
            StudentGroup currentGroup = null;
            GroupRequirement currentReq = null;
            
            foreach (var group in Groups)
            {
                var unfulfilled = group.GetUnfulfilledRequirements();
                if (unfulfilled.Any())
                {
                    currentGroup = group;
                    currentReq = unfulfilled.First();
                    break;
                }
            }

            
            if (currentGroup == null) return true;

            var discipline = Disciplines.FirstOrDefault(d => d.Id == currentReq.DisciplineId);
            if (discipline == null) throw new InvalidOperationException("Discipline not found");

            var eligibleTeachers = FilterTeachers(discipline, currentGroup).ToList();
            var eligibleRooms = FilterRooms(discipline, currentGroup).ToList();

            foreach (var teacher in eligibleTeachers)
            {
                foreach (var room in eligibleRooms)
                {
                    discipline.SetContext(currentGroup.Id); 
                    int slot = FindCommonFreeSlot(currentGroup, teacher, room, discipline);
                    if (slot != -1)
                    {
                        var lesson = CommitTransaction(currentGroup, teacher, room, discipline, slot);
                        
                        
                        if (GenerateRecursive(placedCount + 1)) return true;
                        
                        
                        RollbackTransaction(lesson);
                    }
                }
            }

            return false; 
        }

        private IEnumerable<Teacher> FilterTeachers(Discipline disc, StudentGroup group)
        {
            return Teachers.Where(t => t.CanTeach(disc) && t.CanTakeMoreHours());
        }

        private IEnumerable<Room> FilterRooms(Discipline disc, StudentGroup group)
        {
            return Rooms.Where(r => 
            {
                bool fits = r.Capacity >= group.StudentsCount;
                if (fits) r.SetContext(group.StudentsCount);
                return fits;
            });
        }

        private int FindCommonFreeSlot(IParticipant g, IParticipant t, IParticipant r, IParticipant d)
        {
            bool[] gMask = g.GetAvailabilityMask();
            bool[] tMask = t.GetAvailabilityMask();
            bool[] rMask = r.GetAvailabilityMask();
            bool[] dMask = d.GetAvailabilityMask();

            for (int i = 0; i < Config.TotalCells; i++)
            {
                if (gMask[i] && tMask[i] && rMask[i] && dMask[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private Lesson CommitTransaction(StudentGroup g, Teacher t, Room r, Discipline d, int slot)
        {
            var lesson = new Lesson
            {
                DisciplineId = d.Id,
                TeacherId = t.Id,
                StudentGroupId = g.Id,
                RoomId = r.Id,
                CellIndex = slot
            };

            
            g.BookSlot(slot);
            t.BookSlot(slot);
            r.ReserveSeats(g.StudentsCount, slot);
            d.BookSlot(slot, g.Id); 
            
            
            var req = g.Requirements.First(req => req.DisciplineId == d.Id);
            req.FulfilledHours++;

            
            _grid[slot].Add(lesson);

            return lesson;
        }

        private void RollbackTransaction(Lesson lesson)
        {
            var g = Groups.First(x => x.Id == lesson.StudentGroupId);
            var t = Teachers.First(x => x.Id == lesson.TeacherId);
            var r = Rooms.First(x => x.Id == lesson.RoomId);
            var d = Disciplines.First(x => x.Id == lesson.DisciplineId);

            
            g.ReleaseSlot(lesson.CellIndex);
            t.ReleaseSlot(lesson.CellIndex);
            r.FreeSeats(g.StudentsCount, lesson.CellIndex);
            d.ReleaseSlot(lesson.CellIndex, g.Id); 

            
            var req = g.Requirements.First(req => req.DisciplineId == lesson.DisciplineId);
            req.FulfilledHours--;

            
            _grid[lesson.CellIndex].Remove(lesson);
        }
    }
}
