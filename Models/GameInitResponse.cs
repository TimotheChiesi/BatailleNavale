namespace Models;

public class GameInitResponse
{
    public BattleGrid PlayerGrid { get; set; }
    public Guid GameId { get; set; }
    public List<MoveLog> History { get; set; } = new();
}