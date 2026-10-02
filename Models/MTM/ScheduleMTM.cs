using ScheduleModel = Lab1MAUI.Models.Schedule.Schedule;
using Lab1MAUI.Models.Schedule;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.MTM
{
    /// <summary>
    /// Singleton-сховище налаштувань розкладу для UI (MapToModel Layer).
    ///
    /// Це єдине місце, де UI зберігає стан між навігацією.
    /// Доменні об'єкти (Schedule, Teacher, StudentGroup тощо) НЕ зберігаються постійно —
    /// вони створюються заново при кожному виклику MapToModel().
    ///
    /// Після успішної генерації результат зберігається у LastResult для читання
    /// ScheduleResultViewModel.
    /// </summary>
    public class ScheduleMTM
    {
        public ScheduleConfig Config { get; } = new ScheduleConfig();

        public ObservableCollection<TeacherMTM> Teachers { get; } = new();
        public ObservableCollection<StudentGroupMTM> Groups { get; } = new();
        public ObservableCollection<RoomMTM> Rooms { get; } = new();
        public ObservableCollection<DisciplineMTM> Disciplines { get; } = new();

        /// <summary>
        /// Результат останньої генерації. Null якщо генерація ще не відбувалась або провалилась.
        /// </summary>
        public ScheduleModel? LastResult { get; private set; }

        /// <summary>
        /// Перетворює поточний стан MTM на новий екземпляр доменної моделі Schedule.
        /// Викликати перед кожною генерацією — кожен виклик повертає абсолютно новий об'єкт
        /// з чистими масками та нульовими лічильниками, тому повторна генерація безпечна.
        /// </summary>
        public ScheduleModel MapToModel()
        {
            var schedule = new ScheduleModel(Config);

            // Спочатку маємо всі доменні Discipline, щоб під час маппінгу
            // Constraints дисципліна вже існувала як IParticipant-власник.
            foreach (var d in Disciplines)
            {
                schedule.Disciplines.Add(d.MapToModel(Config));
            }

            foreach (var t in Teachers)
            {
                schedule.Teachers.Add(t.MapToModel(Config));
            }

            foreach (var g in Groups)
            {
                schedule.Groups.Add(g.MapToModel(Config));
            }

            foreach (var r in Rooms)
            {
                schedule.Rooms.Add(r.MapToModel(Config));
            }

            return schedule;
        }

        /// <summary>
        /// Запускає генерацію і зберігає результат.
        /// Повертає true якщо генерація успішна, false — якщо обмеження несумісні.
        /// </summary>
        public bool TryGenerate(out string errorMessage)
        {
            var schedule = MapToModel();
            try
            {
                schedule.Generate();
                LastResult = schedule;
                
                int totalRequired = Groups.Sum(g => g.Requirements.Sum(r => r.RequiredHours));
                int totalPlaced = schedule.Grid.SelectMany(c => c).Count();
                
                if (totalPlaced < totalRequired)
                {
                    errorMessage = $"Частковий розклад: розміщено {totalPlaced} з {totalRequired} уроків.";
                    return false;
                }

                errorMessage = string.Empty;
                return true;
            }
            catch (System.Exception ex)
            {
                LastResult = schedule;
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
