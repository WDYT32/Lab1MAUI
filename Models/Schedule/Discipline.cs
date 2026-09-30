using System;

namespace Lab1MAUI.Models.Schedule;

public class Discipline
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
