using Models;

namespace WebAppBackend.Services;

using Microsoft.AspNetCore.SignalR;

public class BattleHub : Hub
{
    private static readonly Dictionary<string, GameRoom> Rooms = new();
    private static readonly object _lock = new object();
    
    private readonly GameService _gameService;
    
    public BattleHub(GameService gameService)
    {
        _gameService = gameService;
    }

    public async Task JoinRoom(string roomId, Player player)
    {
        GameRoom room;
        string connectionId = Context.ConnectionId;
        bool isGameStarting = false;

        lock (_lock)
        {
            if (!Rooms.TryGetValue(roomId, out room))
            {
                room = new GameRoom { RoomId = roomId };
                Rooms[roomId] = room;
            }

            if (room.Player1?.PlayerId == player.PlayerId)
            {
                room.Player1Connection = connectionId;
                Console.WriteLine("Player1 reconnected with new ID: " + connectionId);
            }
            else if (room.Player2?.PlayerId == player.PlayerId)
            {
                room.Player2Connection = connectionId;
                Console.WriteLine("Player2 reconnected with new ID: " + connectionId);
            }
            else if (room.Player1 is null)
            {
                room.Player1 = player;
                room.Player1Connection = connectionId;
                Console.WriteLine("Player1 joined with ID: " + connectionId);
            }
            else if (room.Player2 is null)
            {
                room.Player2 = player;
                room.Player2Connection = connectionId;
                isGameStarting = true;
                Console.WriteLine("Player2 joined with ID: " + connectionId);
            }
            else
            {
                Clients.Caller.SendAsync("JoinError", "Room is full");
                return;
            }
        }

        // Add this connection to the room group
        await Groups.AddToGroupAsync(connectionId, roomId);

        await Clients.Group(roomId).SendAsync("JoinedRoom", roomId, 1, room.Player1!.PlayerId);
        if (room.Player2Connection != null)
        {
            await Clients.Group(roomId).SendAsync("JoinedRoom", roomId, 2, room.Player2!.PlayerId);
        }

        // Start game if both players joined
        if (isGameStarting)
        {
            // Notify everyone that game is ready
            await Clients.Group(roomId).SendAsync("GameReady", roomId, room.Player1!.Pseudo, room.Player2!.Pseudo);

            // Create game in memory
            _gameService.StartNewMultiplayerGame(roomId, room.Player1.PlayerId, room.Player2.PlayerId);

            // Notify both players to navigate/start game
            await Clients.Group(roomId).SendAsync("StartGame", roomId);
        }
    }
    
    public async Task PlayerAttack(string roomId, string playerId, int row, int col)
    {
        // 1. Find room
        if (!Rooms.TryGetValue(roomId, out var room)) return;

        // 2. Get the MultiplayerGameState
        var gameState = _gameService.GetMultiplayerGame(roomId);

        if (gameState == null) return;

        // 3. Call your attack logic
        var result = _gameService.AttackMultiplayer(gameState, playerId, row, col);

        if (result != null)
        {
            // 4. Notify the attacking player (optional, they already know)
            await Clients.Caller.SendAsync("AttackResult", row, col, result.PlayerAttackSucceeded, result.Winner, result.MultiplayerMoveLog, result.AttackStatus);

            // 5. Notify the opponent about the attack
            string opponentConnectionId = (room.Player1.PlayerId == playerId) ? room.Player2Connection : room.Player1Connection;

            if (!string.IsNullOrEmpty(opponentConnectionId))
            {
                await Clients.Client(opponentConnectionId).SendAsync(
                    "ReceiveAttack", row, col, result.PlayerAttackSucceeded, result.MultiplayerMoveLog, result.AttackStatus
                );
            }

            // 6. Optionally, notify both about the winner
            if (result.Winner != null)
            {
                await Clients.Group(roomId).SendAsync("GameEnded", result.Winner);
            }
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Hub] Connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[Hub] Disconnected: {Context.ConnectionId}");

        // Clear stale connection IDs
        foreach (var room in Rooms.Values)
        {
            if (room.Player1Connection == Context.ConnectionId)
                room.Player1Connection = null;
            if (room.Player2Connection == Context.ConnectionId)
                room.Player2Connection = null;
        }

        await base.OnDisconnectedAsync(exception);
    }

}

