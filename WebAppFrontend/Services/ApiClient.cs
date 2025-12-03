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

    public async Task StartGameAsync()
    {
        var response = await _httpClient.PostAsJsonAsync("/api/start", new { });
        var data = await response.Content.ReadFromJsonAsync<GameInitResponse>();

        if (data != null)
            _battleshipState.Initialize(data.GameId, data.PlayerGrid.Cells, data.PlayerGrid.Ships, data.History);
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
            };
            
            if (data.Winner != null)
            {
                _battleshipState.Winner = data.Winner;
            }
            if (data.AiAttackResult != null)
            {
                var aiAttack = data.AiAttackResult; 
                _battleshipState.RegisterAiAttack(aiAttack.Row, aiAttack.Col, aiAttack.AiAttackSucceeded);
                moveLog.AiAttack = aiAttack;
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
    
    public async Task StartPlacementAsync()
    {
        var response = await _httpClient.PostAsJsonAsync("/api/start", new { });
        var data = await response.Content.ReadFromJsonAsync<GameInitResponse>();

        if (data != null)
        {
            _battleshipState.InitializePlacement(data.GameId, data.PlayerGrid);
        }
    }
    
    public async Task FinalizePlacementAsync(List<Models.Ship> ships, AiDifficulty difficulty)
    {
        var request = new FinalizePlacementRequest
        {
            GameId = _battleshipState.GameId,
            Ships = ships,
            Difficulty = difficulty
        };

        var response = await _httpClient.PostAsJsonAsync("/api/finalize", request);
        var data = await response.Content.ReadFromJsonAsync<FinalizePlacementResponse>();

        if (data != null)
        {
            _battleshipState.Initialize(data.GameId, data.PlayerGrid.Cells, data.PlayerGrid.Ships, data.History);
        }
    }

}
