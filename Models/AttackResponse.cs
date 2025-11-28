namespace Models;

public class AttackResponse
{
    public int AttackIndex { get; set; }
    public bool PlayerAttackSucceeded { get; set; }
    public AiAttackResult? AiAttackResult { get; set; }
    public string? Winner { get; set; }
}
