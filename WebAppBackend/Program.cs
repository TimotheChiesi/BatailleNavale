using Models;
using WebAppBackend.Services;
using FluentValidation;
using WebAppBackend;

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

builder.Services.AddSingleton<IValidator<AttackRequest>, AttackRequestValidator>();

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

app.MapPost("/api/attack", 
    (AttackRequest req, GameService gameService, IValidator<AttackRequest> validator) =>
    {
        var validationResult = validator.Validate(req);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var game = gameService.GetGame(req.GameId);
        if (game == null)
            return Results.NotFound(new { Message = "Game not found" });

        var result = gameService.Attack(game, req.Row, req.Col);
        return Results.Ok(result);
    });


app.Run();