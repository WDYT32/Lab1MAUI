using System.Collections.ObjectModel;
using System.Linq;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.ViewModels;

public class ScheduleResultViewModel : BaseViewModel
{
    private readonly Schedule _schedule;

    public ObservableCollection<Lesson> UnassignedLessons { get; } = new();

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
        var unassigned = _schedule.Lessons.Where(l => l.TeacherId == System.Guid.Empty);
        
        foreach (var lesson in unassigned)
        {
            UnassignedLessons.Add(lesson);
        }
        OnPropertyChanged(nameof(HasUnassignedLessons));
        
        LoadGroupSchedules();
    }

    public ObservableCollection<GroupScheduleDisplay> GroupSchedules { get; } = new();

    private void LoadGroupSchedules()
    {
        GroupSchedules.Clear();
        
        var validLessons = _schedule.Lessons.Where(l => l.TeacherId != System.Guid.Empty).ToList();
        
        foreach (var group in _schedule.Groups)
        {
            var display = new GroupScheduleDisplay { GroupName = group.Name };
            var groupLessons = validLessons.Where(l => l.StudentGroupId == group.Id).ToList();
            
            var lessonsByDay = groupLessons
                .Select(l => {
                    var cellIndex = group.Grid.Cells.ToList().IndexOf(l.Id);
                    int day = cellIndex != -1 ? cellIndex / _schedule.Config.CellsPerDay : -1;
                    int timeSlot = cellIndex != -1 ? cellIndex % _schedule.Config.CellsPerDay : -1;
                    return new { Lesson = l, Day = day, TimeSlot = timeSlot };
                })
                .Where(x => x.Day != -1)
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
                    dayDisplay.Lessons.Add(new LessonDisplay
                    {
                        TimeSlotName = (item.TimeSlot + 1).ToString(),
                        SubjectName = item.Lesson.SubjectName,
                        TeacherName = teacher != null ? $"{teacher.LastName} {teacher.FirstName}" : "Невідомий",
                        RoomName = "ауд. ON-LINE",
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
