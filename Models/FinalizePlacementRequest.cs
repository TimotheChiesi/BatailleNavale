namespace Models;

using System;
using System.Collections.Generic;

public class FinalizePlacementRequest
{
    public Guid GameId { get; set; }
    public List<Ship> Ships { get; set; } = new List<Ship>();
    public AiDifficulty Difficulty { get; set; }
    public int GridSize { get; set; }
}
public class FinalizePlacementResponse
{
    public Guid GameId { get; set; }
    public BattleGrid PlayerGrid { get; set; } = new BattleGrid();
    public List<MoveLog> History { get; set; } = new List<MoveLog>();
    public int GridSize { get; set; }
}
