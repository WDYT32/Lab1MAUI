using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class DailyLimitConstraint : IConstraint
{
    public int Priority { get; set; } = 80; 
    public Guid OwnerId { get; }
    
    private readonly int _maxLessonsPerDay;

    public DailyLimitConstraint(Guid ownerId, int maxLessonsPerDay)
    {
        OwnerId = ownerId;
        _maxLessonsPerDay = maxLessonsPerDay;
    }

    public void Apply(Schedule schedule)
    {
        var grid = schedule.Teachers.FirstOrDefault(t => t.Id == OwnerId)?.Grid 
            ?? schedule.Groups.FirstOrDefault(g => g.Id == OwnerId)?.Grid;
        
        if (grid == null) return;

        int days = schedule.Config.Days;
        int cellsPerDay = schedule.Config.CellsPerDay;

        for (int day = 0; day < days; day++)
        {
            int startCell = day * cellsPerDay;
            int placedLessons = 0;
            List<int> freeCells = new();

            for (int i = 0; i < cellsPerDay; i++)
            {
                int cellIndex = startCell + i;
                if (!grid.IsFree(cellIndex) && schedule.Lessons.Any(l => l.Id == grid.Cells[cellIndex])) 
                    placedLessons++;
                else if (grid.IsFree(cellIndex))
                    freeCells.Add(cellIndex);
            }

            int allowedMore = Math.Max(0, _maxLessonsPerDay - placedLessons);
            int toBlock = freeCells.Count - allowedMore;

            for (int i = 0; i < toBlock; i++)
            {
                // Ставимо порожні уроки (блокери) з кінця дня, де вважаємо непотрібним ставити уроки
                int blockIndex = freeCells[freeCells.Count - 1 - i];
                grid.Place(Guid.NewGuid(), blockIndex);
            }
        }
    }
}
