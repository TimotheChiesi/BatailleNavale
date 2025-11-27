namespace WebAppFrontend.Services;

public class BattleshipState
{
    // Keep the grid as char[][] (same shape as backend)
    public char[][] PlayerGrid { get; private set; } =
        Enumerable.Range(0, 10).Select(_ => new char[10]).ToArray();

    // Opponent grid must be bool?[,]
    public bool?[,] OpponentGrid { get; private set; } = new bool?[10, 10];

    public Guid GameId { get; private set; }
    
    public String? Winner { get; set; }

    public void Initialize(Guid gameId, char[][] playerGridFromApi)
    {
        GameId = gameId;
        PlayerGrid = playerGridFromApi;

        // Reset opponent grid
        OpponentGrid = new bool?[10, 10];
    }

    public void RegisterPlayerAttack(int row, int col, bool hit)
    {
        OpponentGrid[row, col] = hit;
    }

    public void RegisterAiAttack(int row, int col, bool hit)
    {
        var symbol = hit ? 'X' : 'O';
        PlayerGrid[row][col] = symbol;
    }
}

