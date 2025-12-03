namespace Models;

public class MultiplayerAttackResponse
{
    public int AttackIndex { get; set; }
    public bool PlayerAttackSucceeded { get; set; }
    public string? Winner { get; set; }
    
    public MultiplayerMoveLog MultiplayerMoveLog { get; set; }
}