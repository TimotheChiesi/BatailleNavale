namespace Models;

public class BattleGrid
{
    public char[][] Cells { get; set; } =
        Enumerable.Range(0, 10)
            .Select(_ => new char[10])
            .ToArray();

}