using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab1MAUI.ViewModels
{
    
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        
        public static System.Func<string, System.Threading.Tasks.Task> NavigationAction = async (route) => 
        {
            if (Microsoft.Maui.Controls.Shell.Current != null)
                await Microsoft.Maui.Controls.Shell.Current.GoToAsync(route);
        };
    }
}
