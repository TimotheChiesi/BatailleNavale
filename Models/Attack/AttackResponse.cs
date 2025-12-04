namespace Models;

public class AttackResponse
{
    public int AttackIndex { get; set; }
    public bool PlayerAttackSucceeded { get; set; }
    public List<AiAttackResult> AiAttackResults { get; set; } = new();
    public string? Winner { get; set; }
}
