using Models;

namespace WebAppBackend.Services;

public class GameRoom
{
    public string RoomId { get; set; } = default!;
    
    public Player? Player1 { get; set; }
    public Player? Player2 { get; set; }
    
    public string? Player1Connection { get; set; }
    public string? Player2Connection { get; set; }
}

