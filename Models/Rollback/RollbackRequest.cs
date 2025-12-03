namespace Models;

public class RollbackRequest
{
    public Guid GameId { get; set; }
    public int Index { get; set; }
}