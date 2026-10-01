using Microsoft.Extensions.Logging;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Views;

namespace Lab1MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // РЕЄСТРАЦІЯ DEPENDENCY INJECTION
        
        // 1. Єдиний стан моделі на всю програму
        builder.Services.AddSingleton<Schedule>();

        // 2. ViewModels
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<TeachersViewModel>();
        builder.Services.AddSingleton<RoomsViewModel>();
        builder.Services.AddSingleton<StudentGroupsViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<DisciplinesViewModel>();
        builder.Services.AddTransient<DisciplineEditViewModel>();
        builder.Services.AddTransient<ScheduleResultViewModel>();
        builder.Services.AddTransient<TeacherEditViewModel>();
        builder.Services.AddTransient<RoomEditViewModel>();
        builder.Services.AddTransient<GroupEditViewModel>();

        // 3. Сторінки (щоб MAUI міг автоматично ін'єктувати ViewModels у їхні конструктори)
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<DisciplinesPage>();
        builder.Services.AddTransient<DisciplineEditPage>();
        builder.Services.AddSingleton<TeachersPage>();
        builder.Services.AddSingleton<RoomsPage>();
        builder.Services.AddSingleton<StudentGroupsPage>();
        builder.Services.AddTransient<ScheduleResultPage>();
        builder.Services.AddTransient<TeacherEditPage>();
        builder.Services.AddTransient<RoomEditPage>();
        builder.Services.AddTransient<GroupEditPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
