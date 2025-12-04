using System.Net.Http.Json;
using Models;

namespace WebAppFrontend.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BattleshipState _battleshipState;

    public ApiClient(HttpClient httpClient, BattleshipState battleshipState)
    {
        _httpClient = httpClient;
        _battleshipState = battleshipState;
    }

    public async Task AttackAsync(int row, int col)
    {
        var request = new AttackRequest
        {
            GameId = _battleshipState.GameId,
            Row = row,
            Col = col
        };

        var response = await _httpClient.PostAsJsonAsync("/api/attack", request);
        var data = await response.Content.ReadFromJsonAsync<AttackResponse>();

        if (data != null)
        {
            _battleshipState.RegisterPlayerAttack(row, col, data.PlayerAttackSucceeded);
            var moveLog = new MoveLog
            {
                Row = row,
                Col = col,
                PlayerAttackSucceeded = data.PlayerAttackSucceeded,
                AiAttacks = data.AiAttackResults
            };
            
            if (data.Winner != null)
            {
                _battleshipState.Winner = data.Winner;
            }
            if (data.AiAttackResults.Any())
            {
                foreach (var aiAttack in data.AiAttackResults)
                {
                    _battleshipState.RegisterOpponentAttack(aiAttack.Row, aiAttack.Col, aiAttack.AiAttackSucceeded);
                }
            }
            
            _battleshipState.AddTurn(moveLog);
        }
    }

    public async Task RollbackAsync(int index)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/rollback", new { GameId = _battleshipState.GameId, Index = index });
        var data = await response.Content.ReadFromJsonAsync<RollbackResponse>();

        if (data != null)
        {
            _battleshipState.UpdateAfterRollback(data.PlayerGrid, data.AiGrid);
            int startIndexToRemove = index;
            int countToRemove = _battleshipState.History.Count - startIndexToRemove;

            if (countToRemove > 0)
            {
                _battleshipState.History.RemoveRange(startIndexToRemove, countToRemove);
            }
            
            _battleshipState.Winner = null;
        }
    }
    
    public async Task StartPlacementAsync(int gridSize)
    {
        var request = new StartGameRequest { GridSize = gridSize };
        var response = await _httpClient.PostAsJsonAsync("/api/start", request);
        var data = await response.Content.ReadFromJsonAsync<GameInitResponse>();

        if (data != null)
        {
            _battleshipState.InitializePlacement(data.GameId, data.PlayerGrid, data.GridSize);
        }
    }
    
    public async Task FinalizePlacementAsync(List<Models.Ship> ships, AiDifficulty difficulty, int gridSize)
    {
        var request = new FinalizePlacementRequest
        {
            GameId = _battleshipState.GameId,
            Ships = ships,
            Difficulty = difficulty,
            GridSize = gridSize
        };

        var response = await _httpClient.PostAsJsonAsync("/api/finalize", request);
        var data = await response.Content.ReadFromJsonAsync<FinalizePlacementResponse>();

        if (data != null)
        {
            _battleshipState.Initialize(data.GameId, data.PlayerGrid.Cells, data.PlayerGrid.Ships, data.History, data.GridSize);
        }
    }

    public async Task GetMultiplayerGameAsync(string roomId, string playerId)
    {
        var request = new MultiplayerStartRequest
        {
            RoomId = roomId,
            PlayerId = playerId
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/multiplayer/start", request);
        if (!response.IsSuccessStatusCode)
            return;

        var init = await response.Content.ReadFromJsonAsync<GameInitResponse>();
        if (init == null)
            return;

        _battleshipState.Initialize(
            init.GameId,
            init.PlayerGrid.Cells,
            init.PlayerGrid.Ships,
            new List<MoveLog>(),
           10
        );
        _battleshipState.CurrentPlayerTurn = init.StartingPlayer;
    }
}
