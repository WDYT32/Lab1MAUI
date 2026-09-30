using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class DisciplineDailyLimitConstraint : IConstraint
{
    public int Priority { get; set; } = 80; 
    public Guid OwnerId { get; }
    
    private readonly int _maxLessonsPerDay;

    public DisciplineDailyLimitConstraint(Guid ownerId, int maxLessonsPerDay)
    {
        OwnerId = ownerId;
        _maxLessonsPerDay = maxLessonsPerDay;
    }

    public override string ToString() => $"Ліміт предмету на день: {_maxLessonsPerDay}";

    public void Apply(Schedule schedule)
    {
        int days = schedule.Config.Days;
        int cellsPerDay = schedule.Config.CellsPerDay;

        foreach (var group in schedule.Groups)
        {
            var grid = group.Grid;
            
            for (int day = 0; day < days; day++)
            {
                int startCell = day * cellsPerDay;
                int placedLessons = 0;
                List<int> freeCells = new();

                for (int i = 0; i < cellsPerDay; i++)
                {
                    int cellIndex = startCell + i;
                    
                    if (!grid.IsFree(cellIndex))
                    {
                        var lesson = schedule.Lessons.FirstOrDefault(l => l.Id == grid.Cells[cellIndex]);
                        if (lesson != null && lesson.DisciplineId == OwnerId)
                        {
                            placedLessons++;
                        }
                    }
                    else
                    {
                        freeCells.Add(cellIndex);
                    }
                }

                int allowedMore = Math.Max(0, _maxLessonsPerDay - placedLessons);
                int toBlock = freeCells.Count - allowedMore;

                // Блокуємо, ставлячи фіктивні ідентифікатори, щоб не можна було поставити цю дисципліну.
                // Але почекайте, фіктивні GUID блокують весь слот. Тобто туди взагалі нічого не можна буде поставити.
                // На жаль, IConstraint в цій архітектурі не дозволяє блокувати конкретну дисципліну для слота.
                // Тому ми не можемо ефективно використати порожній "Lesson" для блокування, оскільки це заблокує і інші предмети.
                // Для простоти, ми можемо переміщати зайві уроки цієї дисципліни з цього дня на інші дні.
                
                if (placedLessons > _maxLessonsPerDay)
                {
                    // Якщо вже поставлено забагато (через випадковий спавн або інші обмеження),
                    // звільняємо зайві, щоб вони перерозподілилися пізніше
                    int toRemove = placedLessons - _maxLessonsPerDay;
                    for (int i = 0; i < cellsPerDay && toRemove > 0; i++)
                    {
                        int cellIndex = startCell + i;
                        if (!grid.IsFree(cellIndex))
                        {
                            var lesson = schedule.Lessons.FirstOrDefault(l => l.Id == grid.Cells[cellIndex]);
                            if (lesson != null && lesson.DisciplineId == OwnerId)
                            {
                                grid.Remove(cellIndex);
                                toRemove--;
                            }
                        }
                    }
                }
            }
        }
    }
}
