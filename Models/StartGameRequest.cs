namespace Models;

public class StartGameRequest
{
    public AiDifficulty Difficulty { get; set; } = AiDifficulty.Random;
    public int GridSize { get; set; } = 10;
}