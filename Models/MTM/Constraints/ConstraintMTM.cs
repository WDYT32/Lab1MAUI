using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    /// <summary>
    /// Абстрактний базовий MTM-клас для обмежень.
    /// Зберігає налаштування обмеження в UI-зручному форматі.
    /// При генерації розкладу MapToModel() перетворює його в доменний IConstraint.
    /// </summary>
    public abstract class ConstraintMTM
    {
        /// <summary>
        /// Людиночитне ім'я для відображення у списку обмежень.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Створює доменний IConstraint з прив'язаним власником (IParticipant) та конфігурацією.
        /// Викликається з TeacherMTM.MapToModel(), StudentGroupMTM.MapToModel(), DisciplineMTM.MapToModel()
        /// вже після того, як доменний об'єкт-власник створений.
        /// </summary>
        public abstract IConstraint MapToModel(IParticipant owner, ScheduleConfig config);
    }
}
