using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule;

public class TimeGrid
{
    public int TargetHours { get; set; }
    
    private Guid?[] _cells = Array.Empty<Guid?>();
    public IReadOnlyList<Guid?> Cells => _cells;

    public void Initialize(int totalCells)
    {
        _cells = new Guid?[totalCells];
    }

    public bool IsFree(int cellIndex) => _cells[cellIndex] == null;

    public void Place(Guid itemId, int cellIndex)
    {
        _cells[cellIndex] = itemId;
    }

    public void Remove(int cellIndex)
    {
        _cells[cellIndex] = null;
    }
}
