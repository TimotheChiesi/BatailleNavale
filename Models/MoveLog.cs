namespace Models;

public class MoveLog
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool PlayerAttackSucceeded { get; set; }
    
    public AiAttackResult? AiAttack { get; set; }
}