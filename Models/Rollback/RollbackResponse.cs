namespace Models;

public class RollbackResponse
{
    public BattleGrid PlayerGrid { get; set; }
    public bool?[][] AiGrid { get; set; }
}