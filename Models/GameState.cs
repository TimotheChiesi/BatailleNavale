namespace Models;

public class GameState
{
    public Guid GameId { get; set; } = Guid.NewGuid();

    // Mutable Grids (Change during game)
    public BattleGrid PlayerGrid { get; set; }
    public BattleGrid AiGrid { get; set; }

    // NEW: Immutable Originals (For rollback)
    public BattleGrid OriginalPlayerGrid { get; set; } 
    public BattleGrid OriginalAiGrid { get; set; }
    
    public Queue<(int Row, int Col)> AiMoves { get; set; } = new();
    public string? Winner { get; set; }
    
    public List<MoveLog> History { get; set; } = new();
}