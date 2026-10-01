using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels
{
    public class RoomsViewModel : BaseViewModel
    {
        private readonly Schedule _schedule;
        
        public ObservableCollection<Room> Rooms { get; } = new();

        public ICommand AddRoomCommand { get; }
        public ICommand EditRoomCommand { get; }
        public ICommand DeleteRoomCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private Room _draggedRoom;

        public RoomsViewModel(Schedule schedule)
        {
            _schedule = schedule;
            
            LoadRooms();
            
            AddRoomCommand = new Command(async () => 
            {
                await BaseViewModel.NavigationAction("RoomEditPage?id=new");
            });
            
            EditRoomCommand = new Command<Room>(async (room) => 
            {
                if (room != null)
                {
                    await BaseViewModel.NavigationAction($"RoomEditPage?id={room.Id}");
                }
            });
            
            DeleteRoomCommand = new Command<Room>((room) => 
            {
                if (room != null)
                {
                    Rooms.Remove(room);
                    _schedule.Rooms.Remove(room);
                }
            });

            DragStartingCommand = new Command<Room>(r => _draggedRoom = r);
            
            DropCommand = new Command<Room>(r => 
            {
                if (_draggedRoom != null && _draggedRoom != r)
                {
                    var draggedIndex = Rooms.IndexOf(_draggedRoom);
                    var dropIndex = Rooms.IndexOf(r);
                    
                    if (draggedIndex >= 0 && dropIndex >= 0)
                    {
                        Rooms.Move(draggedIndex, dropIndex);
                        SyncScheduleList();
                    }
                }
                _draggedRoom = null;
            });
        }

        private void SyncScheduleList()
        {
            _schedule.Rooms.Clear();
            foreach (var r in Rooms)
            {
                _schedule.Rooms.Add(r);
            }
        }

        public void LoadRooms()
        {
            Rooms.Clear();
            foreach (var r in _schedule.Rooms)
            {
                Rooms.Add(r);
            }
        }
    }
}
