namespace Models;

public class StartGameRequest
{
    public AiDifficulty Difficulty { get; set; } = AiDifficulty.Random;
}