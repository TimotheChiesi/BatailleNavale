namespace Models;

public class SinglePlayerGameState : BaseGameState
{
    public string? Winner { get; set; }
    public int GridSize { get; set; }
    
    // Mutable Grids
    public BattleGrid PlayerGrid { get; set; }
    public BattleGrid AiGrid { get; set; }

    // AI Specifics (Not needed in Multiplayer)
    public BattleGrid OriginalPlayerGrid { get; set; } 
    public BattleGrid OriginalAiGrid { get; set; }
    public Queue<(int Row, int Col)> AiMoves { get; set; } = new();
    public Stack<(int Row, int Col)> AiTargetStack { get; set; } = new();
    public AiDifficulty AiDifficulty { get; set; } = AiDifficulty.Random;
    
    
    public List<MoveLog> History { get; set; } = new();
}