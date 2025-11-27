using Models;
using WebAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<GridService>();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

app.UseCors();

app.MapPost("/api/start", (GameService gameService) =>
{
    var game = gameService.StartNewGame();

    return new GameInitResponse
    {
        GameId = game.GameId,
        PlayerGrid = game.PlayerGrid
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