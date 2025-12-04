using Models;

namespace WebAppBackend.Services;

public class GameService
{
    private readonly GridService _gridService;
    private readonly Dictionary<Guid, BaseGameState> _games = new();

    public GameService(GridService gridService)
    {
        _gridService = gridService;
    }

    public SinglePlayerGameState StartNewSinglePlayerGame()
    {
        var playerGrid = _gridService.GenerateGrid();
        var aiGrid = _gridService.GenerateGrid();
        
        var game = new SinglePlayerGameState
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
    
    public void StartNewMultiplayerGame(string roomIdString, string player1Id, string player2Id)
    {
        // 1. Convert the Room String to a GUID
        if (!Guid.TryParse(roomIdString, out Guid gameId))
        {
            throw new ArgumentException("Invalid Room ID format");
        }

        var game = new MultiplayerGameState
        {
            GameId = gameId, // The Room GUID becomes the Game GUID
            Player1Id = player1Id,
            Player1Grid = _gridService.GenerateGrid(),
            Player2Id = player2Id,
            Player2Grid = _gridService.GenerateGrid(),
            History = new List<MultiplayerMoveLog>()
        };

        _games[gameId] = game;
    }

    public MultiplayerGameState? GetMultiplayerGame(string idString)
    {
        if (Guid.TryParse(idString, out Guid id))
        {
            _games.TryGetValue(id, out var game);
            return game as MultiplayerGameState;
        }
        return null;
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
        if (!_games.TryGetValue(id, out var baseGame))
            throw new Exception("Game not found.");
        
        if (baseGame is not SinglePlayerGameState game)
        {
            throw new Exception("Rollback is not allowed in Multiplayer games.");
        }

        if (index < 0 || index > game.History.Count)
            throw new Exception("Invalid rollback index.");

        game.PlayerGrid = DeepCopyGrid(game.OriginalPlayerGrid);
        game.AiGrid = DeepCopyGrid(game.OriginalAiGrid);
        game.Winner = null;
        game.AiTargetStack.Clear();

        game.AiMoves = GenerateAiMoves();

        for (int i = 0; i < index; i++)
        {
            var move = game.History[i];

            // ---- PLAYER ATTACK ----
            bool playerHit = move.PlayerAttackSucceeded;

            game.AiGrid.Cells[move.Row][move.Col] = playerHit ? 'X' : 'O';

            // ---- AI ATTACK (only if this move had one) ----
            if (move.AiAttacks.Any())
            {
                foreach (var aiAttack in move.AiAttacks)
                {
                    bool aiHit = aiAttack.AiAttackSucceeded;
                    game.PlayerGrid.Cells[aiAttack.Row][aiAttack.Col] = aiHit ? 'X' : 'O';
                }

                game.AiMoves.Dequeue();
            }
        }

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

    public SinglePlayerGameState? GetGame(Guid id)
    {
        if (_games.TryGetValue(id, out var baseGame))
        {
            return baseGame as SinglePlayerGameState;
        }
        return null;
    }

    private Queue<(int Row, int Col)> GenerateAiMoves()
    {
        var moves = new List<(int, int)>();

        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                moves.Add((r, c));

        return new Queue<(int, int)>(moves.OrderBy(_ => Random.Shared.Next()));
    }
    
    public AttackResponse Attack(SinglePlayerGameState game, int row, int col)
    {
        // If game is already over, simply return winner info
        if (game.Winner != null)
        {
            return new AttackResponse
            {
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = false,
                AiAttackResults = new(),
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
                AiAttackResults = new(),
                Winner = "Player"
            };
        }
        
        // AI's turn
        if (!playerHit)
        {
            var aiAttackResults = new List<AiAttackResult>();
            bool aiHit;

            do
            {
                var (aiRow, aiCol) = GetNextAiMove(game);

                char aiCell = game.PlayerGrid.Cells[aiRow][aiCol];
                aiHit = IsShip(aiCell);

                game.PlayerGrid.Cells[aiRow][aiCol] = aiHit ? 'X' : 'O';

                AiAttackResult aiAttackResult = new AiAttackResult
                {
                    AiAttackSucceeded = aiHit,
                    Row = aiRow,
                    Col = aiCol
                };
                aiAttackResults.Add(aiAttackResult);

                if (game.AiDifficulty == AiDifficulty.TargetingRandom && aiHit)
                {
                    AddNeighborsToTargetStack(game, aiRow, aiCol);
                }

                // Check if AI has won after each shot
                if (!GridHasShips(game.PlayerGrid.Cells))
                {
                    game.Winner = "AI";
                    // The last AI attack is the one that matters for the history log
                    moveLog.AiAttacks = aiAttackResults;
                    game.History.Insert(game.History.Count, moveLog);
                    return new AttackResponse
                    {
                        AttackIndex = game.History.Count,
                        PlayerAttackSucceeded = playerHit,
                        AiAttackResults = aiAttackResults,
                        Winner = "AI"
                    };
                }
            } while (aiHit); // AI plays again if it hit

            // The last AI attack is the one that matters for the history log
            moveLog.AiAttacks = aiAttackResults;
            game.History.Insert(game.History.Count, moveLog);

            return new AttackResponse
            {
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = playerHit,
                AiAttackResults = aiAttackResults,
                Winner = null
            };
        }
        else
        {
            // Player hit, so it's their turn again. AI does not play.
            game.History.Insert(game.History.Count, moveLog);
            return new AttackResponse
            {
                AttackIndex = game.History.Count,
                PlayerAttackSucceeded = playerHit,
                AiAttackResults = new(),
                Winner = null
            };
        }
    }
    
    public MultiplayerAttackResponse? AttackMultiplayer(MultiplayerGameState multiplayerGameState, string playerId, int row, int col)
    {
        // If game is already over, simply return winner info
        if (multiplayerGameState.Winner != null)
        {
            return new MultiplayerAttackResponse
            {
                AttackIndex = multiplayerGameState.History.Count,
                PlayerAttackSucceeded = false,
                Winner = multiplayerGameState.Winner == PlayerRole.Player1 ? multiplayerGameState.Player1Id : multiplayerGameState.Player2Id
            };
        }
        
        if (multiplayerGameState.CurrentTurnPlayerId == playerId)
        {
            char cell = multiplayerGameState.OpponentGrid.Cells[row][col];
            bool playerHit = cell is >= 'A' and <= 'F';
            
            multiplayerGameState.OpponentGrid.Cells[row][col] = playerHit ? 'X' : 'O';
            
            var moveLog = new MultiplayerMoveLog()
            {
                AttackerId = playerId,
                Row = row,
                Col = col,
                PlayerAttackSucceeded = playerHit
            };
            
            // Check if player has won
            if (!GridHasShips(multiplayerGameState.OpponentGrid.Cells))
            {
                multiplayerGameState.History.Insert(multiplayerGameState.History.Count, moveLog);
            
                multiplayerGameState.Winner = multiplayerGameState.CurrentPlayerTurn;
                
                return new MultiplayerAttackResponse
                {
                    AttackIndex = multiplayerGameState.History.Count,
                    PlayerAttackSucceeded = true,
                    Winner = multiplayerGameState.Winner == PlayerRole.Player1 ? multiplayerGameState.Player1Id : multiplayerGameState.Player2Id,
                    MultiplayerMoveLog = moveLog
                };
            }

            if (!playerHit)
            {
                multiplayerGameState.NextTurn();
            }
            
            multiplayerGameState.History.Insert(multiplayerGameState.History.Count, moveLog);
            return new MultiplayerAttackResponse
            {
                AttackIndex = multiplayerGameState.History.Count,
                PlayerAttackSucceeded = playerHit,
                Winner = null,
                MultiplayerMoveLog = moveLog
            };
        }
        else
        {
            return null;
        }
    }

    
    private (int Row, int Col) GetNextAiMove(SinglePlayerGameState game)
    {
        if (game.AiDifficulty == AiDifficulty.TargetingRandom)
        {
            // Targeting Mode: Use the stack of potential targets
            while (game.AiTargetStack.TryPop(out var target))
            {
                // Check if this cell has already been attacked
                if (game.PlayerGrid.Cells[target.Row][target.Col] is 'X' or 'O')
                {
                    continue; // Already tried, pop next target
                }
                return target; // Found a valid, untried target
            }
        }
        
        // Hunting Mode (or if TargetStack is empty): Get a random move
        (int Row, int Col) move;
        do
        {
            move = game.AiMoves.Dequeue();
        } 
        // Ensure we don't attack a cell we've already hit (can happen during rollback/replay)
        while (game.PlayerGrid.Cells[move.Row][move.Col] is 'X' or 'O');

        return move;
    }

    private void AddNeighborsToTargetStack(SinglePlayerGameState game, int row, int col)
    {
        // Potential neighbors (Up, Down, Left, Right)
        var neighbors = new[]
        {
            (row - 1, col),
            (row + 1, col),
            (row, col - 1),
            (row, col + 1)
        };

        foreach (var (r, c) in neighbors)
        {
            // Check bounds and if it's already been attacked
            if (r >= 0 && r < 10 && c >= 0 && c < 10 && game.PlayerGrid.Cells[r][c] is not 'X' and not 'O')
            {
                game.AiTargetStack.Push((r, c));
            }
        }
    }

    private static bool IsShip(char cell) => cell is >= 'A' and <= 'F';

    private static bool GridHasShips(char[][] grid)
    {
        return grid.Any(row => row.Any(IsShip));
    }
    
    public BaseGameState? FinalizeGameSetup(Guid id, List<Ship> playerShips, AiDifficulty difficulty)
    {
        if (!_games.TryGetValue(id, out var baseGame))
            return null;

        if (baseGame is not SinglePlayerGameState game)
        {
            throw new Exception("FinalizeGameSetup is not allowed in Multiplayer games.");
        }
        // Clear the original player grid cells
        game.PlayerGrid.Cells = Enumerable.Range(0, 10).Select(_ => new char[10]).ToArray();
        game.PlayerGrid.Ships = playerShips;
        game.AiDifficulty = difficulty;
        
        // Re-populate the cells based on the user-placed ships
        foreach (var ship in playerShips)
        {
            if (ship.IsHorizontal)
            {
                for (int i = 0; i < ship.Size; i++)
                    game.PlayerGrid.Cells[ship.StartRow][ship.StartCol + i] = ship.Symbol;
            }
            else
            {
                for (int i = 0; i < ship.Size; i++)
                    game.PlayerGrid.Cells[ship.StartRow + i][ship.StartCol] = ship.Symbol;
            }
        }
        
        // Update the original grid copy for rollback purposes
        game.OriginalPlayerGrid = DeepCopyGrid(game.PlayerGrid);
        
        return game;
    }
}
