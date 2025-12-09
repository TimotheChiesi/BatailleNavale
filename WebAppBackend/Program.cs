using Models;
using WebAppBackend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebAppBackend;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5021")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<GridService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<IValidator<AttackRequest>, AttackRequestValidator>();
builder.Services.AddSingleton<IValidator<MultiplayerAttackRequest>, MultiplayerAttackRequestValidator>();
builder.Services.AddSingleton<IValidator<FinalizePlacementRequest>, FinalizePlacementRequestValidator>();
builder.Services.AddSingleton<IValidator<RollbackRequest>, RollbackRequestValidator>();
builder.Services.AddSingleton<IValidator<MultiplayerStartRequest>, MultiplayerStartRequestValidator>();
builder.Services.AddGrpc();
builder.Services.AddSignalR();

var app = builder.Build();


app.UseRouting();

app.UseGrpcWeb();
app.UseCors();

app.MapHub<BattleHub>("/battleHub");

app.MapPost("/api/start", (GameService gameService, [FromBody] StartGameRequest request) =>
{
    var game = gameService.StartNewSinglePlayerGame(request.GridSize);
    return new GameInitResponse
    {
        GameId = game.GameId,
        PlayerGrid = game.PlayerGrid,
        GridSize = game.GridSize
    };
});

app.MapPost("/api/attack", 
    (AttackRequest req, GameService gameService, IValidator<AttackRequest> validator) =>
    {
        var validationResult = validator.Validate(req);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var game = gameService.GetGame(req.GameId);
        if (game == null)
            return Results.NotFound(new { Message = "Game not found" });

        var result = gameService.Attack(game, req.Row, req.Col);
        return Results.Ok(result);
    });

app.MapPost("/api/rollback", (RollbackRequest req, GameService gameService, IValidator<RollbackRequest> validator) =>
{
    var validationResult = validator.Validate(req);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updatedGameState = gameService.RollbackGame(req.GameId, req.Index);

    if (updatedGameState == null)
    {
        return Results.NotFound(new { Message = "Game not found" });
    }

    return Results.Ok(updatedGameState);
});

app.MapPost("/api/finalize", (FinalizePlacementRequest req, GameService gameService, IValidator<FinalizePlacementRequest> validator) =>
{
    var validationResult = validator.Validate(req);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updatedGameState = gameService.FinalizeGameSetup(req.GameId, req.Ships, req.Difficulty, req.GridSize);

    if (updatedGameState == null)
    {
        return Results.NotFound(new { Message = "Game not found" });
    }
    
    if (updatedGameState is not SinglePlayerGameState game)
    {
        throw new Exception("FinalizeGameSetup is not allowed in Multiplayer games.");
    }
    
    return Results.Ok(new FinalizePlacementResponse
    {
        GameId = game.GameId,
        PlayerGrid = game.PlayerGrid,
        History = game.History,
        GridSize = game.GridSize
    });
});

app.MapPost("/api/multiplayer/start", (GameService gameService, MultiplayerStartRequest request, IValidator<MultiplayerStartRequest> validator) =>
{
    var validationResult = validator.Validate(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    // A. FETCH ONLY (Do not StartNewGame here)
    // The game was already created by the SignalR Hub when the 2nd player joined.
    var game = (gameService.GetMultiplayerGame(request.RoomId) as MultiplayerGameState)!;
    
    // B. Security Check & Response
    if (request.PlayerId == game.Player1Id)
    {
        return Results.Ok(new GameInitResponse 
        { 
            GameId = game.GameId,
            PlayerGrid = game.Player1Grid, // Return P1's secrets
            StartingPlayer = "You",
            GridSize = game.GridSize
        });
    }
    else if (request.PlayerId == game.Player2Id)
    {
        return Results.Ok(new GameInitResponse 
        { 
            GameId = game.GameId,
            PlayerGrid = game.Player2Grid, // Return P2's secrets
            StartingPlayer = "Opponent",
            GridSize = game.GridSize
        });
    }

    return Results.BadRequest("Player ID not found in this match.");
});

app.MapGrpcService<BattleshipGrpcService>()
   .EnableGrpcWeb(); 

app.MapGet("/", () => "gRPC Battleship Server Running (HTTP/1.1)");

app.Run();