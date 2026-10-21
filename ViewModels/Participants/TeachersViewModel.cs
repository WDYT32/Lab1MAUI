using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Views.Edit;

namespace Lab1MAUI.ViewModels.Participants
{
    public class TeachersViewModel : BaseListViewModel<TeacherUI>
    {
        public TeachersViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI.Teachers, nameof(TeacherEditPage))
        {
        }
    }
}
