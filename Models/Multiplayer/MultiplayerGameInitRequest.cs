namespace Models;

public class MultiplayerGameInitRequest
{
    public string RoomId { get; set; }
    public string Player1Id { get; set; }
    public string Player2Id { get; set; }
}