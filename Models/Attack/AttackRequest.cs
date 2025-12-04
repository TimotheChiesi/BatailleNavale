namespace Models;

public class AttackRequest
{
    public Guid GameId { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}