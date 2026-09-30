using Microsoft.Maui.Controls;
using Lab1MAUI.Views;

namespace Lab1MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Реєструємо маршрут до сторінки результатів, оскільки ми переходимо на неї програмно (з коду ViewModel),
        // а не вибираємо з бічного меню.
        Routing.RegisterRoute("ScheduleResultPage", typeof(ScheduleResultPage));
        Routing.RegisterRoute("TeacherEditPage", typeof(TeacherEditPage));
        Routing.RegisterRoute("GroupEditPage", typeof(GroupEditPage));
        Routing.RegisterRoute("DisciplineEditPage", typeof(DisciplineEditPage));
    }
}
