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
    
    public BattleGrid CreateGridFromShips(int gridSize, List<Ship> ships)
    {
        // 1. Create a clean grid
        var grid = new BattleGrid(gridSize);
    
        // 2. Assign the ships directly
        grid.Ships = ships;

        // 3. Populate the 'Cells' array based on the ship coordinates
        foreach (var ship in ships)
        {
            for (int i = 0; i < ship.Size; i++)
            {
                if (ship.IsHorizontal)
                {
                    // Ensure we don't go out of bounds (safety check)
                    if (ship.StartCol + i < gridSize)
                    {
                        grid.Cells[ship.StartRow][ship.StartCol + i] = ship.Symbol;
                    }
                }
                else
                {
                    if (ship.StartRow + i < gridSize)
                    {
                        grid.Cells[ship.StartRow + i][ship.StartCol] = ship.Symbol;
                    }
                }
            }
        }

        return grid;
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
