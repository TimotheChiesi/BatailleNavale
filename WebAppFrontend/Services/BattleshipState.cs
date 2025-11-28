using Models;

namespace WebAppFrontend.Services;

public class BattleshipState
{
    public char[][] PlayerGrid { get; private set; } =
        Enumerable.Range(0, 10).Select(_ => new char[10]).ToArray();
    
    public bool?[,] OpponentGrid { get; private set; } = new bool?[10, 10];
    
    public List<Ship> PlayerShips { get; private set; } = new();

    public Guid GameId { get; private set; }
    
    public String? Winner { get; set; }
    
    public List<MoveLog> History { get; private set; } = new();

    public void Initialize(Guid gameId, char[][] playerGridFromApi, List<Ship> playerShipsFromApi, List<MoveLog> existingHistory)
    {
        GameId = gameId;
        PlayerGrid = playerGridFromApi;
        PlayerShips = playerShipsFromApi;
        Winner = null;
        History = existingHistory ?? new List<MoveLog>();

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

    public void AddTurn(MoveLog turn)
    {
        History = History.Append(turn).ToList();
    }

    public void UpdateAfterRollback(BattleGrid playerGrid, bool?[][] aiGridJagged)
    {
        PlayerGrid = playerGrid.Cells;
        // Convert jagged array to 2D array
        int rows = aiGridJagged.Length;
        int cols = aiGridJagged[0].Length;
        OpponentGrid = new bool?[rows, cols];

        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            OpponentGrid[r, c] = aiGridJagged[r][c];
    }
}

