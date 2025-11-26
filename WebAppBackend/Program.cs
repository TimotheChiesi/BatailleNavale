using Models;
using WebAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GridService>();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

app.MapPost("/api/start", (GameService gameService) =>
{
    var game = gameService.StartNewGame();

    return new
    {
        gameId = game.GameId,
        playerGrid = game.PlayerGrid
    };
});

app.MapPost("/api/attack", (AttackRequest req, GameService gameService) =>
{
    var game = gameService.GetGame(req.GameId);
    if (game == null) return Results.NotFound("Game not found");

    var result = gameService.Attack(game, req.Row, req.Col);
    return Results.Ok(result);
});

app.Run();