using System;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class TeacherTimeOffConstraint : IConstraint
{
    public int Priority { get; set; } = 500; 
    public Guid OwnerId { get; }
    
    private readonly int _blockedCellIndex;

    public TeacherTimeOffConstraint(Teacher teacher, int blockedCellIndex)
    {
        OwnerId = teacher.Id;
        _blockedCellIndex = blockedCellIndex;
    }

    public void Apply(Schedule schedule)
    {
        var teacher = schedule.Teachers.FirstOrDefault(t => t.Id == OwnerId);
        if (teacher == null) return;
        
        // Якщо клітинка вільна, ми її блокуємо (якщо вже зайнята чимсь іншим, 
        // високий пріоритет міг би перезаписати, але поки просто блокуємо вільну)
        if (teacher.Grid.IsFree(_blockedCellIndex))
        {
            teacher.Grid.Place(Guid.Empty, _blockedCellIndex); 
        }
    }
}
