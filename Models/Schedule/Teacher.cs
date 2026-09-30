using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule;

public class Teacher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    
    public string Name 
    { 
        get => $"{LastName} {FirstName} {Patronymic}".Trim(); 
        set 
        {
            var parts = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts?.Length > 0) LastName = parts[0];
            if (parts?.Length > 1) FirstName = parts[1];
            if (parts?.Length > 2) Patronymic = string.Join(" ", parts.Skip(2));
        }
    }
    
    public TimeGrid Grid { get; } = new();

    public List<TeacherCompetence> Competences { get; set; } = new();
    
    public List<IConstraint> Constraints { get; set; } = new();

    public List<IConstraint> GetPersonalConstraints(int totalCells) => Constraints;
}
