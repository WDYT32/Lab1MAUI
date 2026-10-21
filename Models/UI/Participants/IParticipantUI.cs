using System;

namespace Lab1MAUI.Models.UI.Participants
{
    public interface IParticipantUI
    {
        Guid Id { get; }
        string Name { get; set; }
    }
}
