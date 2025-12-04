namespace Models;

public abstract class BaseGameState
{
    public Guid GameId { get; set; } = Guid.NewGuid();
}