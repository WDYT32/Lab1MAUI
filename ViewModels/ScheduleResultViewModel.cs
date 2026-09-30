using System.Collections.ObjectModel;
using System.Linq;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.ViewModels;

public class ScheduleResultViewModel : BaseViewModel
{
    private readonly Schedule _schedule;

    // Колекція для відображення проблемних уроків, яким не знайшлося місця чи викладача
    public ObservableCollection<Lesson> UnassignedLessons { get; } = new();

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
    }
}
