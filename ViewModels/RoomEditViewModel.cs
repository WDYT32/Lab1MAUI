using System;
using System.Linq;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(RoomId), "id")]
    public class RoomEditViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        private Room _room;
        private bool _isNew;

        public string RoomId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _room = new Room(Guid.NewGuid(), "", 30, _schedule.Config.TotalCells);
                    Name = "";
                    Capacity = 30;
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _room = _schedule.Rooms.FirstOrDefault(r => r.Id == id) ?? new Room(Guid.NewGuid(), "", 30, _schedule.Config.TotalCells);
                    Name = _room.Name;
                    Capacity = _room.Capacity;
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _capacity;
        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        public ICommand SaveCommand { get; }

        public RoomEditViewModel(Schedule schedule)
        {
            _schedule = schedule;

            SaveCommand = new Command(async () =>
            {
                if (_room == null || string.IsNullOrWhiteSpace(Name)) return;

                _room.Name = Name;
                _room.Capacity = Capacity;

                if (_isNew && !_schedule.Rooms.Contains(_room))
                {
                    _schedule.Rooms.Add(_room);
                }

                await BaseViewModel.NavigationAction("..");
            });
        }
    }
}
