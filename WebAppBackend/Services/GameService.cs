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
        var playerGrid = _gridService.GenerateGrid();
        var aiGrid = _gridService.GenerateGrid();
        
        var game = new GameState
        {
            PlayerGrid = playerGrid,
            AiGrid = aiGrid,
            OriginalPlayerGrid = DeepCopyGrid(playerGrid),
            OriginalAiGrid = DeepCopyGrid(aiGrid),
            
            AiMoves = GenerateAiMoves(),
            History = new List<MoveLog>()
        };

        _games[game.GameId] = game;
        return game;
    }
    
    private BattleGrid DeepCopyGrid(BattleGrid source)
    {
        return new BattleGrid
        {
            // This iterates through every row and creates a distinct copy
            Cells = source.Cells.Select(row => (char[])row.Clone()).ToArray()
        };
    }

    public RollbackResponse RollbackGame(Guid id, int index)
    {
        if (!_games.TryGetValue(id, out var game))
            throw new Exception("Game not found.");

        if (index < 0 || index > game.History.Count)
            throw new Exception("Invalid rollback index.");

        game.PlayerGrid = DeepCopyGrid(game.OriginalPlayerGrid);
        game.AiGrid = DeepCopyGrid(game.OriginalAiGrid);
        game.Winner = null;

        game.AiMoves = GenerateAiMoves();

        for (int i = 0; i < index; i++)
        {
            var move = game.History[i];

            // ---- PLAYER ATTACK ----
            bool playerHit = move.PlayerAttackSucceeded;

            game.AiGrid.Cells[move.Row][move.Col] = playerHit ? 'X' : 'O';

            // ---- AI ATTACK (only if this move had one) ----
            if (move.AiAttack != null)
            {
                var ai = move.AiAttack;

                bool aiHit = ai.AiAttackSucceeded;

                game.PlayerGrid.Cells[ai.Row][ai.Col] = aiHit ? 'X' : 'O';

                game.AiMoves.Dequeue();
            }
        }

        // 4. Delete the "Future" from History
        // We want to keep everything up to 'index'. 
        // So we remove starting from 'index + 1' to the end.
        int startIndexToRemove = index + 1;
        int countToRemove = game.History.Count - startIndexToRemove;

        if (countToRemove > 0)
        {
            game.History.RemoveRange(startIndexToRemove, countToRemove);
        }

        return new RollbackResponse
        {
            PlayerGrid = game.PlayerGrid,
            AiGrid = ConvertGridToBool(game.AiGrid.Cells)
        };
    }

    private bool?[][] ConvertGridToBool(char[][] cells)
    {
        var result = new bool?[10][];
        for (int r = 0; r < 10; r++)
        {
            result[r] = new bool?[10];
            for (int c = 0; c < 10; c++)
            {
                char cell = cells[r][c];
                result[r][c] = cell switch
                {
                    'X' => true,
                    'O' => false,
                    _ => null
                };
            }
        }
        return result;
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
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = false,
                AiAttackResult = null,
                Winner = game.Winner
            };
        }
        
        // Player's turn
        char cell = game.AiGrid.Cells[row][col];
        bool playerHit = cell is >= 'A' and <= 'F';

        game.AiGrid.Cells[row][col] = playerHit ? 'X' : 'O';
        
        var moveLog = new MoveLog
        {
            Row = row,
            Col = col,
            PlayerAttackSucceeded = playerHit
        };

        // Check if player has won
        if (!GridHasShips(game.AiGrid.Cells))
        {
            game.History.Insert(game.History.Count, moveLog);
            
            game.Winner = "Player";
            return new AttackResponse
            {
                AttackIndex = game.History.Count,
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

            moveLog.AiAttack = aiAttackResult;
            game.History.Insert(game.History.Count, moveLog);

            // Check if AI has won
            if (!GridHasShips(game.PlayerGrid.Cells))
            {
                game.Winner = "AI";
                return new AttackResponse
                {
                    AttackIndex = game.History.Count,
                    PlayerAttackSucceeded = playerHit,
                    AiAttackResult = aiAttackResult,
                    Winner = "AI"
                };
            }
            
            return new AttackResponse
            {
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = playerHit,
                AiAttackResult = aiAttackResult,
                Winner = null
            };
        }
        else
        {
            game.History.Insert(game.History.Count, moveLog);
            return new AttackResponse
            {
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = playerHit,
                AiAttackResult = null,
                Winner = null
            };
        }
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
