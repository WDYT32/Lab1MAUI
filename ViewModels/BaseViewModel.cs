using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab1MAUI.ViewModels
{
    // Частина 1.3: Реалізація INotifyPropertyChanged з перевіркою рівності
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Універсальний сетер, що перевіряє зміну значення (вимога лаби)
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
