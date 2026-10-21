using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Views.Edit;

namespace Lab1MAUI.ViewModels.Participants
{
    public class DisciplinesViewModel(ScheduleUI scheduleUi)
        : BaseListViewModel<DisciplineUI>(scheduleUi.Disciplines, nameof(DisciplineEditPage));
}
