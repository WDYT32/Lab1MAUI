using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Views.Edit;

namespace Lab1MAUI.ViewModels.Participants
{
    public class DisciplinesViewModel : BaseListViewModel<DisciplineUI>
    {
        public DisciplinesViewModel(ScheduleUI scheduleUI)
            : base(scheduleUI.Disciplines, nameof(DisciplineEditPage))
        {
        }
    }
}
