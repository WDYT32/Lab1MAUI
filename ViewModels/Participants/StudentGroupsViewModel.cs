using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Views.Edit;

namespace Lab1MAUI.ViewModels.Participants
{
    public class StudentGroupsViewModel(ScheduleUI scheduleUi)
        : BaseListViewModel<StudentGroupUI>(scheduleUi.Groups, nameof(GroupEditPage));
}
