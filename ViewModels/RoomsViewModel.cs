using Lab1MAUI.Models.MTM;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels
{
    public class RoomsViewModel : BaseViewModel
    {
        private readonly ScheduleMTM _scheduleMTM;

        /// <summary>
        /// Напряму прив'язана до ScheduleMTM.Rooms — жодної синхронізації не потрібно.
        /// </summary>
        public ObservableCollection<RoomMTM> Rooms => _scheduleMTM.Rooms;

        public ICommand AddRoomCommand { get; }
        public ICommand EditRoomCommand { get; }
        public ICommand DeleteRoomCommand { get; }
        public ICommand DragStartingCommand { get; }
        public ICommand DropCommand { get; }

        private RoomMTM _draggedRoom;

        public RoomsViewModel(ScheduleMTM scheduleMTM)
        {
            _scheduleMTM = scheduleMTM;

            AddRoomCommand = new Command(async () =>
            {
                await BaseViewModel.NavigationAction("RoomEditPage?id=new");
            });

            EditRoomCommand = new Command<RoomMTM>(async (room) =>
            {
                if (room != null)
                    await BaseViewModel.NavigationAction($"RoomEditPage?id={room.Id}");
            });

            DeleteRoomCommand = new Command<RoomMTM>((room) =>
            {
                if (room != null)
                    _scheduleMTM.Rooms.Remove(room);
            });

            DragStartingCommand = new Command<RoomMTM>(r => _draggedRoom = r);

            DropCommand = new Command<RoomMTM>(r =>
            {
                if (_draggedRoom != null && _draggedRoom != r)
                {
                    var from = Rooms.IndexOf(_draggedRoom);
                    var to   = Rooms.IndexOf(r);
                    if (from >= 0 && to >= 0)
                        Rooms.Move(from, to);
                }
                _draggedRoom = null;
            });
        }
    }
}
