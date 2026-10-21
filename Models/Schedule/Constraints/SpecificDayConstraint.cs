namespace Lab1MAUI.Models.Schedule.Constraints
{
    public class SpecificDayConstraint(int[] allowedDays, int cellsPerDay) : IConstraint
    {
        public void Apply(bool[] mask)
        {
            int days = mask.Length / cellsPerDay;
            for (int day = 0; day < days; day++)
            {
                bool isAllowed = Array.IndexOf(allowedDays, day) >= 0;
                if (!isAllowed)
                {
                    int start = day * cellsPerDay;
                    for (int i = 0; i < cellsPerDay; i++)
                    {
                        mask[start + i] = false;
                    }
                }
            }
        }
    }
}
