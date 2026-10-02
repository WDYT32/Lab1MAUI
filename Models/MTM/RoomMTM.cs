using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.MTM
{
    /// <summary>
    /// MTM-клас для аудиторії. Є єдиним джерелом правди для UI.
    /// Аудиторія не має обмежень (обмежує лише місткість — це вже в доменній логіці Room).
    /// </summary>
    public class RoomMTM
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; } = 30;

        public Room MapToModel(ScheduleConfig config)
            => new Room(Id, Name, Capacity, config.TotalCells);
    }
}
