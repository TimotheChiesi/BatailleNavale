namespace Models;

public class MultiplayerAttackRequest
{
    public string GameId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public string PlayerId { get; set; }
}