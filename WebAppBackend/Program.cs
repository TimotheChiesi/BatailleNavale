using Models;
using WebAppBackend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebAppBackend;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http1);
    
    options.ListenLocalhost(5224, o => o.Protocols = HttpProtocols.Http1);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

builder.Services.AddSingleton<GridService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<IValidator<AttackRequest>, AttackRequestValidator>();
builder.Services.AddGrpc();

var app = builder.Build();


app.UseRouting();

app.UseGrpcWeb();
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
            return Results.BadRequest(validationResult.Errors);

        var game = gameService.GetGame(req.GameId);
        if (game == null)
            return Results.NotFound(new { Message = "Game not found" });

        var result = gameService.Attack(game, req.Row, req.Col);
        return Results.Ok(result);
    });

app.MapGrpcService<BattleshipGrpcService>()
   .EnableGrpcWeb(); 

app.MapGet("/", () => "gRPC Battleship Server Running (HTTP/1.1)");

app.Run();