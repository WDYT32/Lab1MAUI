using System;

namespace Lab1MAUI.Models.Schedule;

public interface IConstraint
{
    int Priority { get; set; }
    Guid OwnerId { get; }
    
    // Метод намагається застосувати обмеження (ідемпотентна операція).
    // Якщо його застосувати неможливо, або воно вже задоволене - нічого не відбувається.
    void Apply(Schedule schedule);
}
