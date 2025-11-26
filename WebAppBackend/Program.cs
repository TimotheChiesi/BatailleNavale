using Models;
using WebAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GridService>();

var app = builder.Build();

app.MapGet("/api/init", (GridService gridService) =>
{
    return new GameInitResponse
    {
        PlayerGrid = gridService.GenerateGrid(),
        AIGrid = gridService.GenerateGrid()
    };
});

app.Run();