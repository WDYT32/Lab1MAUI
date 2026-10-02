using ScheduleModel = Lab1MAUI.Models.Schedule.Schedule;
using Lab1MAUI.Models.Schedule;
using System.Collections.ObjectModel;

namespace Lab1MAUI.Models.MTM
{
    
    
    
    
    
    
    
    
    
    
    public class ScheduleMTM
    {
        public ScheduleConfig Config { get; } = new ScheduleConfig();

        public ObservableCollection<TeacherMTM> Teachers { get; } = new();
        public ObservableCollection<StudentGroupMTM> Groups { get; } = new();
        public ObservableCollection<RoomMTM> Rooms { get; } = new();
        public ObservableCollection<DisciplineMTM> Disciplines { get; } = new();

        
        
        
        public ScheduleModel? LastResult { get; private set; }

        
        
        
        
        
        public ScheduleModel MapToModel()
        {
            var schedule = new ScheduleModel(Config);

            
            
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
