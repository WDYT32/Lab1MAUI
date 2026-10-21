namespace Lab1MAUI.Models.Schedule
{
    public class GroupRequirement
    {
        public Guid DisciplineId { get; set; }
        public int RequiredLessons { get; set; }
        public int FulfilledLessons { get; set; }

        public bool IsFulfilled()
        {
            return FulfilledLessons >= RequiredLessons;
        }
    }
}
