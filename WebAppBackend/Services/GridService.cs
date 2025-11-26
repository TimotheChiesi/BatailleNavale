using Models;

namespace WebAppBackend.Services;

public class GridService
{
    private static readonly Ship[] Ships = new[]
    {
        new Ship { Symbol = 'A', Size = 1 },
        new Ship { Symbol = 'B', Size = 2 },
        new Ship { Symbol = 'C', Size = 2 },
        new Ship { Symbol = 'D', Size = 3 },
        new Ship { Symbol = 'E', Size = 3 },
        new Ship { Symbol = 'F', Size = 4 }
    };

    public BattleGrid GenerateGrid()
    {
        var grid = new BattleGrid();

        foreach (var ship in Ships)
            PlaceShip(grid.Cells, ship);

        return grid;
    }

    private void PlaceShip(char[][] grid, Ship ship)
    {
        bool placed = false;

        while (!placed)
        {
            bool horizontal = Random.Shared.Next(2) == 0;
            int row = Random.Shared.Next(10);
            int col = Random.Shared.Next(10);

            if (horizontal)
            {
                if (col + ship.Size > 10) continue;

                if (CheckFreeHorizontal(grid, row, col, ship.Size))
                {
                    for (int i = 0; i < ship.Size; i++)
                        grid[row][col + i] = ship.Symbol;

                    placed = true;
                }
            }
            else
            {
                if (row + ship.Size > 10) continue;

                if (CheckFreeVertical(grid, row, col, ship.Size))
                {
                    for (int i = 0; i < ship.Size; i++)
                        grid[row + i][col] = ship.Symbol;

                    placed = true;
                }
            }
        }
    }

    private bool CheckFreeHorizontal(char[][] grid, int row, int col, int size)
    {
        for (int i = 0; i < size; i++)
            if (grid[row][col + i] != '\0') return false;

        return true;
    }

    private bool CheckFreeVertical(char[][] grid, int row, int col, int size)
    {
        for (int i = 0; i < size; i++)
            if (grid[row + i][col] != '\0') return false;

        return true;
    }
}
