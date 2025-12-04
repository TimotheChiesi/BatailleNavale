namespace Models;

public class BattleGrid
{
    public char[][] Cells { get; set; }

    public List<Ship> Ships { get; set; } = new();

    public BattleGrid() : this(10)
    {
    }

    public BattleGrid(int size)
    {
        Cells = Enumerable.Range(0, size)
            .Select(_ => new char[size])
            .ToArray();
    }
}