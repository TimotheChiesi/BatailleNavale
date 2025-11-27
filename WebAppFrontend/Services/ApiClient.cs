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
            _battleshipState.Initialize(data.GameId, data.PlayerGrid.Cells);
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
            if (data.Winner != null)
            {
                _battleshipState.Winner = data.Winner;
            }
            if (data.AiAttackResult != null)
            {
                var aiAttack = data.AiAttackResult; 
                _battleshipState.RegisterAiAttack(aiAttack.Row, aiAttack.Col, aiAttack.AiAttackSucceeded);
            }
        }
    }

}
