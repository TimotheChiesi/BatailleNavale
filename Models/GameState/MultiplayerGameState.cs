namespace Models;

public class MultiplayerGameState : BaseGameState
{
    public string Player1Id { get; set; }
    public string Player2Id { get; set; }
    
    public BattleGrid Player1Grid { get; set; }
    public BattleGrid Player2Grid { get; set; }

    public PlayerRole CurrentPlayerTurn { get; set; } = PlayerRole.Player1;
    public void NextTurn() => CurrentPlayerTurn = CurrentPlayerTurn == PlayerRole.Player1 ? PlayerRole.Player2 : PlayerRole.Player1;
    
    public string CurrentTurnPlayerId => CurrentPlayerTurn == PlayerRole.Player1 ? Player1Id : Player2Id;
    public BattleGrid OpponentGrid => CurrentPlayerTurn == PlayerRole.Player1 ? Player2Grid : Player1Grid;
    
    public PlayerRole? Winner { get; set; }
    
    public List<MultiplayerMoveLog> History { get; set; } = new();
    
    public int GridSize { get; set; } = 10;
}

public enum PlayerRole
{
    Player1,
    Player2
}