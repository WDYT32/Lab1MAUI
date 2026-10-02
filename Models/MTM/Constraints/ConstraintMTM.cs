using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;

namespace Lab1MAUI.Models.MTM.Constraints
{
    
    
    
    
    
    public abstract class ConstraintMTM
    {
        
        
        
        public abstract string DisplayName { get; }

        
        
        
        
        
        public abstract IConstraint MapToModel(IParticipant owner, ScheduleConfig config);
    }
}
