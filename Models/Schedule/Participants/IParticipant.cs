using System;

namespace Lab1MAUI.Models.Schedule.Participants
{
    public interface IParticipant
    {
        Guid Id { get; }
        string Name { get; }
        bool[] GetAvailabilityMask();
        bool IsBooked(int cellIndex);
    }
}
