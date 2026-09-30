using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    // Повна реалізація вимог Частини 2 (ICommand, ObservableCollection)
    public class TeachersViewModel : BaseViewModel
    {
        public ObservableCollection<Teacher> Teachers { get; } = new();

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                    ((Command)AddTeacherCommand).ChangeCanExecute(); // Оновлюємо стан кнопки
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                    ((Command)AddTeacherCommand).ChangeCanExecute();
            }
        }

        private string _patronymic = string.Empty;
        public string Patronymic
        {
            get => _patronymic;
            set => SetProperty(ref _patronymic, value);
        }

        // OneWay Binding (Частина 1.4)
        public string FullNamePreview => $"Новий викладач: {LastName} {FirstName} {Patronymic}".Trim();

        public ICommand AddTeacherCommand { get; }

        public TeachersViewModel()
        {
            // Частина 2.1: Команда з делегатами Execute та CanExecute
            AddTeacherCommand = new Command(AddTeacher, CanAddTeacher);

            // Підписуємося на власні події, щоб оновлювати рядок Preview
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FirstName) || 
                    e.PropertyName == nameof(LastName) || 
                    e.PropertyName == nameof(Patronymic))
                {
                    OnPropertyChanged(nameof(FullNamePreview));
                }
            };
        }

        private void AddTeacher()
        {
            Teachers.Add(new Teacher
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Patronymic = this.Patronymic
            });

            // Очищення полів після додавання
            FirstName = string.Empty;
            LastName = string.Empty;
            Patronymic = string.Empty;
        }

        // Валідація: кнопка працює лише якщо введено Прізвище та Ім'я
        private bool CanAddTeacher()
        {
            return !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
        }
    }
}
