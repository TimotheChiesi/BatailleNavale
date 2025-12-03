namespace Models;

using System;
using System.Collections.Generic;

public class FinalizePlacementRequest
{
    public Guid GameId { get; set; }
    public List<Ship> Ships { get; set; } = new List<Ship>();
}
public class FinalizePlacementResponse
{
    public Guid GameId { get; set; }
    public BattleGrid PlayerGrid { get; set; } = new BattleGrid();
    public List<MoveLog> History { get; set; } = new List<MoveLog>();
}
