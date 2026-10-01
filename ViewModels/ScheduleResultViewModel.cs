using System.Collections.ObjectModel;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;

namespace Lab1MAUI.ViewModels;

public class ScheduleResultViewModel : BaseViewModel
{
    private readonly Schedule _schedule;

    public ObservableCollection<UnassignedDisplay> UnassignedLessons { get; } = new();

    public bool HasUnassignedLessons => UnassignedLessons.Any();

    public ScheduleResultViewModel(Schedule schedule)
    {
        _schedule = schedule;
        LoadUnassignedLessons();
    }

    private void LoadUnassignedLessons()
    {
        UnassignedLessons.Clear();
        
        // Знаходимо уроки без викладача (наш Edge Case)
        var allLessons = _schedule.Grid.SelectMany(cell => cell).ToList();
        var unassigned = allLessons.Where(l => l.TeacherId == System.Guid.Empty).ToList();
        
        var grouped = unassigned
            .GroupBy(l => new { l.DisciplineId, l.StudentGroupId })
            .Select(g => new UnassignedDisplay
            {
                SubjectName = _schedule.Disciplines.FirstOrDefault(d => d.Id == g.Key.DisciplineId)?.Name ?? "Невідомо",
                GroupName = _schedule.Groups.FirstOrDefault(gr => gr.Id == g.Key.StudentGroupId)?.Name ?? "Невідома група",
                Hours = g.Count(),
                TotalMinutes = g.Count() * _schedule.Config.LessonDurationMinutes
            });
            
        foreach (var group in grouped)
        {
            UnassignedLessons.Add(group);
        }
        OnPropertyChanged(nameof(HasUnassignedLessons));
        
        LoadGroupSchedules(allLessons);
    }

    public ObservableCollection<GroupScheduleDisplay> GroupSchedules { get; } = new();

    private void LoadGroupSchedules(List<Lesson> allLessons)
    {
        GroupSchedules.Clear();
        
        var validLessons = allLessons.Where(l => l.TeacherId != System.Guid.Empty).ToList();
        
        foreach (var group in _schedule.Groups)
        {
            var display = new GroupScheduleDisplay { GroupName = group.Name };
            var groupLessons = validLessons.Where(l => l.StudentGroupId == group.Id).ToList();
            
            var lessonsByDay = groupLessons
                .Select(l => {
                    int day = l.CellIndex / _schedule.Config.CellsPerDay;
                    int timeSlot = l.CellIndex % _schedule.Config.CellsPerDay;
                    return new { Lesson = l, Day = day, TimeSlot = timeSlot };
                })
                .GroupBy(x => x.Day)
                .OrderBy(g => g.Key);
            
            foreach (var dayGroup in lessonsByDay)
            {
                bool isEvenDay = dayGroup.Key % 2 == 0;
                string mainColor = isEvenDay ? "#E09040" : "#2B6048";
                string lightColor = isEvenDay ? "#FFF8F0" : "#F0FFF8";
                
                var dayDisplay = new DayScheduleDisplay 
                { 
                    DayName = $"День {dayGroup.Key + 1}",
                    MainColor = mainColor
                };
                
                foreach (var item in dayGroup.OrderBy(x => x.TimeSlot))
                {
                    var teacher = _schedule.Teachers.FirstOrDefault(t => t.Id == item.Lesson.TeacherId);
                    var discipline = _schedule.Disciplines.FirstOrDefault(d => d.Id == item.Lesson.DisciplineId);
                    var room = _schedule.Rooms.FirstOrDefault(r => r.Id == item.Lesson.RoomId);
                    
                    dayDisplay.Lessons.Add(new LessonDisplay
                    {
                        TimeSlotName = (item.TimeSlot + 1).ToString(),
                        SubjectName = discipline?.Name ?? "Невідомо",
                        TeacherName = teacher?.Name ?? "Невідомий",
                        RoomName = room?.Name ?? "ON-LINE",
                        MainColor = mainColor,
                        LightColor = lightColor
                    });
                }
                display.Days.Add(dayDisplay);
            }
            
            if (display.Days.Any())
            {
                GroupSchedules.Add(display);
            }
        }
    }
}

public class UnassignedDisplay
{
    public string SubjectName { get; set; }
    public string GroupName { get; set; }
    public int Hours { get; set; }
    public int TotalMinutes { get; set; }
}

public class GroupScheduleDisplay
{
    public string GroupName { get; set; }
    public ObservableCollection<DayScheduleDisplay> Days { get; } = new();
}

public class DayScheduleDisplay
{
    public string DayName { get; set; }
    public string MainColor { get; set; }
    public ObservableCollection<LessonDisplay> Lessons { get; } = new();
}

public class LessonDisplay
{
    public string TimeSlotName { get; set; }
    public string SubjectName { get; set; }
    public string TeacherName { get; set; }
    public string RoomName { get; set; }
    public string MainColor { get; set; }
    public string LightColor { get; set; }
}
