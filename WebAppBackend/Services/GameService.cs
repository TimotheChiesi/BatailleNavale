using Models;

namespace WebAppBackend.Services;

public class GameService
{
    private readonly GridService _gridService;
    private readonly Dictionary<Guid, GameState> _games = new();

    public GameService(GridService gridService)
    {
        _gridService = gridService;
    }

    public GameState StartNewGame()
    {
        var game = new GameState
        {
            PlayerGrid = _gridService.GenerateGrid(),
            AiGrid = _gridService.GenerateGrid(),
            AiMoves = GenerateAiMoves()
        };

        _games[game.GameId] = game;
        return game;
    }

    public GameState? GetGame(Guid id)
    {
        _games.TryGetValue(id, out var game);
        return game;
    }

    private Queue<(int Row, int Col)> GenerateAiMoves()
    {
        var moves = new List<(int, int)>();

        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                moves.Add((r, c));

        return new Queue<(int, int)>(moves.OrderBy(_ => Random.Shared.Next()));
    }
    
    public AttackResponse Attack(GameState game, int row, int col)
    {
        // If game is already over, simply return winner info
        if (game.Winner != null)
        {
            return new AttackResponse
            {
                PlayerAttackSucceeded = false,
                AiAttackResult = null,
                Winner = game.Winner
            };
        }
        
        // Player's turn
        char cell = game.AiGrid.Cells[row][col];
        bool playerHit = cell is >= 'A' and <= 'F';

        game.AiGrid.Cells[row][col] = playerHit ? 'X' : 'O';

        // Check if player has won
        if (!GridHasShips(game.AiGrid.Cells))
        {
            game.Winner = "Player";
            return new AttackResponse
            {
                PlayerAttackSucceeded = true,
                AiAttackResult = null,
                Winner = "Player"
            };
        }
        
        // AI's turn
        if (!playerHit)
        {
            var (aiRow, aiCol) = game.AiMoves.Dequeue();

            char aiCell = game.PlayerGrid.Cells[aiRow][aiCol];
            bool aiHit = aiCell is >= 'A' and <= 'F';

            game.PlayerGrid.Cells[aiRow][aiCol] = aiHit ? 'X' : 'O';

            AiAttackResult aiAttackResult = new AiAttackResult
            {
                AiAttackSucceeded = aiHit,
                Row = aiRow,
                Col = aiCol
            };

            // Check if AI has won
            if (!GridHasShips(game.PlayerGrid.Cells))
            {
                game.Winner = "AI";
                return new AttackResponse
                {
                    PlayerAttackSucceeded = playerHit,
                    AiAttackResult = aiAttackResult,
                    Winner = "AI"
                };
            }

            return new AttackResponse
            {
                PlayerAttackSucceeded = playerHit,
                AiAttackResult = aiAttackResult,
                Winner = null
            };
        }
        else
            return new AttackResponse
            {
                PlayerAttackSucceeded = playerHit,
                AiAttackResult = null,
                Winner = null
            };

    }

    private static bool GridHasShips(char[][] grid)
    {
        foreach (var row in grid)
            foreach (var cell in row)
                if (cell is >= 'A' and <= 'F')
                    return true;

        return false;
    }
}
