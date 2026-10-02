using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    [QueryProperty(nameof(RoomId), "id")]
    public class RoomEditViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;
        private RoomMTM _room;
        private bool _isNew;

        public string RoomId
        {
            set
            {
                if (value == "new")
                {
                    _isNew = true;
                    _room = new RoomMTM();
                    Name = string.Empty;
                    Capacity = 30;
                }
                else if (Guid.TryParse(value, out Guid id))
                {
                    _isNew = false;
                    _room = _scheduleMTM.Rooms.FirstOrDefault(r => r.Id == id) ?? new RoomMTM();
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

        public RoomEditViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            SaveCommand = new Command(async () =>
            {
                if (_room == null || string.IsNullOrWhiteSpace(Name)) return;

                _room.Name = Name;
                _room.Capacity = Capacity;

                if (_isNew && !_scheduleMTM.Rooms.Contains(_room))
                {
                    _scheduleMTM.Rooms.Add(_room);
                }
                else
                {
                    var index = _scheduleMTM.Rooms.IndexOf(_room);
                    if (index >= 0)
                        _scheduleMTM.Rooms[index] = _room;
                }

                await BaseViewModel.NavigationAction("..");
            });
        }
    }
}
