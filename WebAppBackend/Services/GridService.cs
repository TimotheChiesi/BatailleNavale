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

    public BattleGrid GenerateGrid(int gridSize = 10)
    {
        var grid = new BattleGrid(gridSize);

        foreach (var ship in Ships)
        {
            var shipClone = new Ship 
            { 
                Symbol = ship.Symbol, 
                Size = ship.Size 
            };
            
            PlaceShip(grid.Cells, shipClone, gridSize);
            grid.Ships.Add(shipClone);
        }
            

        return grid;
    }

    private void PlaceShip(char[][] grid, Ship ship, int gridSize)
    {
        bool placed = false;

        while (!placed)
        {
            bool horizontal = Random.Shared.Next(2) == 0;
            int row = Random.Shared.Next(gridSize);
            int col = Random.Shared.Next(gridSize);

            if (horizontal)
            {
                if (col + ship.Size > gridSize) continue;

                if (CheckFreeHorizontal(grid, row, col, ship.Size, gridSize))
                {
                    for (int i = 0; i < ship.Size; i++)
                        grid[row][col + i] = ship.Symbol;
                    
                    ship.StartRow = row;
                    ship.StartCol = col;
                    ship.IsHorizontal = true;

                    placed = true;
                }
            }
            else
            {
                if (row + ship.Size > gridSize) continue;

                if (CheckFreeVertical(grid, row, col, ship.Size, gridSize))
                {
                    for (int i = 0; i < ship.Size; i++)
                        grid[row + i][col] = ship.Symbol;
                    
                    ship.StartRow = row;
                    ship.StartCol = col;
                    ship.IsHorizontal = false;

                    placed = true;
                }
            }
        }
    }

    private bool CheckFreeHorizontal(char[][] grid, int row, int col, int size, int gridSize)
    {
        if (col + size > gridSize) return false;
        
        for (int i = 0; i < size; i++)
            if (grid[row][col + i] != '\0') return false;

        return true;
    }

    private bool CheckFreeVertical(char[][] grid, int row, int col, int size, int gridSize)
    {
        if (row + size > gridSize) return false;
        
        for (int i = 0; i < size; i++)
            if (grid[row + i][col] != '\0') return false;

        return true;
    }
}
