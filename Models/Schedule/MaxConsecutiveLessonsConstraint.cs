using System;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class MaxConsecutiveLessonsConstraint : IConstraint
{
    public int Priority { get; set; } = 90; 
    public Guid OwnerId { get; }
    
    private readonly int _maxConsecutive;

    public MaxConsecutiveLessonsConstraint(Guid ownerId, int maxConsecutive)
    {
        OwnerId = ownerId;
        _maxConsecutive = maxConsecutive;
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
            int potentialChain = 0;
            
            for (int i = 0; i < cellsPerDay; i++)
            {
                int cellIndex = startCell + i;
                bool isFree = grid.IsFree(cellIndex);
                bool isLesson = !isFree && schedule.Lessons.Any(l => l.Id == grid.Cells[cellIndex]);
                bool isBlocker = !isFree && !isLesson;

                if (isBlocker)
                {
                    potentialChain = 0;
                }
                else
                {
                    potentialChain++;
                    if (potentialChain > _maxConsecutive)
                    {
                        if (isFree)
                        {
                            // Розриваємо потенційний або існуючий ланцюжок порожнім уроком
                            grid.Place(Guid.NewGuid(), cellIndex);
                            potentialChain = 0; 
                        }
                    }
                }
            }
        }
    }
}
