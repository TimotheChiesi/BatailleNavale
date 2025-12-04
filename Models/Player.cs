namespace Models;

public class Player
{
    public string Pseudo { get; set; } = string.Empty;
    public string PlayerId { get; set; } = Guid.NewGuid().ToString();
}