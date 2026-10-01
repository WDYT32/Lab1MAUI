using Lab1MAUI.Views;
using Microsoft.Maui.Controls;

namespace Lab1MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(TeacherEditPage), typeof(TeacherEditPage));
            Routing.RegisterRoute(nameof(GroupEditPage), typeof(GroupEditPage));
            Routing.RegisterRoute(nameof(DisciplineEditPage), typeof(DisciplineEditPage));
            Routing.RegisterRoute(nameof(ScheduleResultPage), typeof(ScheduleResultPage));
            Routing.RegisterRoute(nameof(RoomEditPage), typeof(RoomEditPage));
        }
    }
}
