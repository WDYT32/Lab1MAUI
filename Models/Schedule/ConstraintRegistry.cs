using System.Collections.Generic;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class ConstraintRegistry
{
    // High priority first
    private readonly SortedDictionary<int, IConstraint> _constraints = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));

    public void Register(IConstraint constraint)
    {
        // Simple priority resolution: if priority exists, we decrement until we find empty spot
        int p = constraint.Priority;
        while (_constraints.ContainsKey(p)) p--;
        constraint.Priority = p;
        _constraints.Add(p, constraint);
    }

    public void Clear()
    {
        _constraints.Clear();
    }

    public List<IConstraint> GetOrdered() => _constraints.Values.ToList();
}
