using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.Schedule;
using ScheduleModel = Lab1MAUI.Models.Schedule.Schedule;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lab1MAUI.ViewModels;

public class ScheduleResultViewModel : BaseViewModel
{
    private readonly ScheduleUI _scheduleUI;

    public ObservableCollection<UnassignedDisplay> UnassignedLessons { get; } = new();
    public ObservableCollection<GroupScheduleDisplay> GroupSchedules { get; } = new();

    public bool HasUnassignedLessons => UnassignedLessons.Any();

    public ScheduleResultViewModel(ScheduleUI scheduleUI)
    {
        _scheduleUI = scheduleUI;
        LoadResults();
    }

    private void LoadResults()
    {
        UnassignedLessons.Clear();
        GroupSchedules.Clear();

        var schedule = _scheduleUI.LastResult;
        if (schedule == null) return;

        var allLessons = schedule.Grid.SelectMany(cell => cell).ToList();

        LoadUnassignedLessons(schedule, allLessons);
        LoadGroupSchedules(schedule, allLessons);

        OnPropertyChanged(nameof(HasUnassignedLessons));
    }

    private void LoadUnassignedLessons(ScheduleModel schedule, List<Lesson> allLessons)
    {
        foreach (var groupUI in _scheduleUI.Groups)
        {
            foreach (var reqUI in groupUI.Requirements)
            {
                int placed = allLessons.Count(l =>
                    l.StudentGroupId == groupUI.Id &&
                    l.DisciplineId == reqUI.DisciplineId);

                int unassigned = reqUI.RequiredLessons - placed;
                if (unassigned <= 0) continue;

                var disciplineName = _scheduleUI.Disciplines
                    .FirstOrDefault(d => d.Id == reqUI.DisciplineId)?.Name ?? "Невідомо";

                UnassignedLessons.Add(new UnassignedDisplay
                {
                    SubjectName  = disciplineName,
                    GroupName    = groupUI.Name,
                    LessonsCount = unassigned,
                    TotalMinutes = unassigned * schedule.Config.LessonDurationMinutes
                });
            }
        }
    }

    private void LoadGroupSchedules(ScheduleModel schedule, List<Lesson> allLessons)
    {
        foreach (var groupUI in _scheduleUI.Groups)
        {
            var display = new GroupScheduleDisplay { GroupName = groupUI.Name };
            var groupLessons = allLessons.Where(l => l.StudentGroupId == groupUI.Id).ToList();

            var lessonsByDay = groupLessons
                .Select(l => new
                {
                    Lesson   = l,
                    Day      = l.CellIndex / schedule.Config.CellsPerDay,
                    TimeSlot = l.CellIndex % schedule.Config.CellsPerDay
                })
                .GroupBy(x => x.Day)
                .OrderBy(g => g.Key);

            foreach (var dayGroup in lessonsByDay)
            {
                bool isEvenDay   = dayGroup.Key % 2 == 0;
                string mainColor  = isEvenDay ? "#E09040" : "#2B6048";
                string lightColor = isEvenDay ? "#FFF8F0" : "#F0FFF8";

                var dayDisplay = new DayScheduleDisplay
                {
                    DayName   = $"День {dayGroup.Key + 1}",
                    MainColor = mainColor
                };

                foreach (var item in dayGroup.OrderBy(x => x.TimeSlot))
                {
                    var teacher    = schedule.Teachers.FirstOrDefault(t => t.Id == item.Lesson.TeacherId);
                    var discipline = schedule.Disciplines.FirstOrDefault(d => d.Id == item.Lesson.DisciplineId);
                    var room       = schedule.Rooms.FirstOrDefault(r => r.Id == item.Lesson.RoomId);

                    dayDisplay.Lessons.Add(new LessonDisplay
                    {
                        TimeSlotName = (item.TimeSlot + 1).ToString(),
                        SubjectName  = discipline?.Name ?? "Невідомо",
                        TeacherName  = teacher?.Name   ?? "Невідомий",
                        RoomName     = room?.Name      ?? "ON-LINE",
                        MainColor    = mainColor,
                        LightColor   = lightColor
                    });
                }

                display.Days.Add(dayDisplay);
            }

            if (display.Days.Any())
                GroupSchedules.Add(display);
        }
    }
}

public class UnassignedDisplay
{
    public string SubjectName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
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
