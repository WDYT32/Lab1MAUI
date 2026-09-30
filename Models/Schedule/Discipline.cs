using System;

using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule;

public class Discipline
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    
    public List<IConstraint> Constraints { get; set; } = new();

    public List<IConstraint> GetPersonalConstraints(int totalCells)
    {
        return Constraints;
    }
}
