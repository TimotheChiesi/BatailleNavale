namespace Models;

public class GameState
{
    public Guid GameId { get; set; } = Guid.NewGuid();

    public BattleGrid PlayerGrid { get; set; }
    public BattleGrid AiGrid { get; set; }

    public Queue<(int Row, int Col)> AiMoves { get; set; } = new();
    public string? Winner { get; set; }
}