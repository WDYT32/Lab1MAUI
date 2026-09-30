using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule;

public class StudentGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    
    public TimeGrid Grid { get; } = new();

    public List<GroupRequirement> Requirements { get; set; } = new();
    
    public List<IConstraint> Constraints { get; set; } = new();

    public List<IConstraint> GetPersonalConstraints(int totalCells)
    {
        var list = new List<IConstraint>(Constraints)
        {
            new TeacherAssignmentConstraint(this)
        };
        return list;
    }
}
