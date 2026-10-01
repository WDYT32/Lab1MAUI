using System;

namespace Lab1MAUI.Models.Schedule
{
    public class Lesson
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DisciplineId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid StudentGroupId { get; set; }
        public Guid RoomId { get; set; }
        public int CellIndex { get; set; }
    }
}
