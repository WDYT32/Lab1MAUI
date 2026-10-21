using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System;

namespace Lab1MAUI.Models.UI.Participants
{
    public class RoomUI : IParticipantUI
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; } = 30;

        public Room MapToModel(ScheduleConfig config)
            => new Room(Id, Name, Capacity, config.TotalCells);
    }
}
