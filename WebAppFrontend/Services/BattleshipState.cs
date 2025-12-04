using Models;

namespace WebAppFrontend.Services;

public class BattleshipState
{
    public char[][] PlayerGrid { get; private set; }
    
    public bool?[,] OpponentGrid { get; private set; }
    public int GridSize { get; private set; }
    
    public List<Ship> PlayerShips { get; private set; } = new();

    public Guid GameId { get; private set; }
    
    public String? Winner { get; set; }
    
    public List<MoveLog> History { get; private set; } = new();
    
    public string CurrentPlayerTurn { get; set; }
    
    public BattleGrid PlacementGrid { get; private set; } = new();

    public List<string>? AttackStatus { get; set; } = new();

    public BattleshipState()
    {
        SetGridSize(10);
    }

    public void Initialize(Guid gameId, char[][] playerGridFromApi, List<Ship> playerShipsFromApi, List<MoveLog> existingHistory, int gridSize)
    {
        GameId = gameId;
        SetGridSize(gridSize);
        PlayerGrid = playerGridFromApi;
        PlayerShips = playerShipsFromApi;
        Winner = null;
        History = existingHistory ?? new List<MoveLog>();
        OpponentGrid = new bool?[gridSize, gridSize];
    }
    
    public void InitializePlacement(Guid gameId, BattleGrid initialPlayerGrid, int gridSize)
    {
        GameId = gameId;
        SetGridSize(gridSize);
        PlacementGrid = initialPlayerGrid;
        
        // Clear game state properties that will be set on FinalizePlacement
        PlayerGrid = Enumerable.Range(0, gridSize).Select(_ => new char[gridSize]).ToArray();
        OpponentGrid = new bool?[gridSize, gridSize];
        PlayerShips = new();
        Winner = null;
        History = new List<MoveLog>();
    }

    public void RegisterPlayerAttack(int row, int col, bool hit)
    {
        OpponentGrid[row, col] = hit;
    }
    
    public void RegisterOpponentAttack(int row, int col, bool hit)
    {
        var symbol = hit ? 'X' : 'O';
        PlayerGrid[row][col] = symbol;
    }

    public void AddTurn(MoveLog turn)
    {
        History = History.Append(turn).ToList();
    }
    
    public void AddTurnMultiplayer(MultiplayerMoveLog turn)
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
    
    private void SetGridSize(int size)
    {
        GridSize = size;
        PlayerGrid = Enumerable.Range(0, size).Select(_ => new char[size]).ToArray();
        OpponentGrid = new bool?[size, size];
    }
}
