using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    private Gem.GemType[,] gGrid;

    void Awake()
    {
        gGrid = new Gem.GemType[width, height];

        PopulateGemGrid();

        var matches = CheckMatches(gGrid);
        while (matches.Count > 0 || !HasMovesLeft())
        {
            // print($"Repopulating grid");

            PopulateGemGrid();
            matches = CheckMatches(gGrid);
        }
    }

    void PopulateGemGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                SOGem newGem = GemManager.Instance.GetRandomGem();
                gGrid[i, j] = newGem.GemType;
            }
        }

    }

    public List<List<Vector2Int>> CheckMatches(Gem.GemType[,] grid)
    {
        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        List<Vector2Int> currMatch = new List<Vector2Int>();
        List<Vector2Int> gemsAlreadyInAMatch = new List<Vector2Int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (gemsAlreadyInAMatch.Contains(new Vector2Int(i, j)))
                {
                    continue;
                }

                // Add current gem node being evaluated to the current match
                currMatch.Add(new Vector2Int(i, j));

                // Check neighbors recursively
                List<Vector2Int> operationExclude = new List<Vector2Int>();
                operationExclude.Add(new Vector2Int(i, j));
                List<Vector2Int> connectedSameTypeNeighbors =
                    SearchSameGemTypeNeighborsMock(new Vector2Int(i, j), grid, ref operationExclude);
                currMatch.AddRange(connectedSameTypeNeighbors);

                if (currMatch.Count >= 3)
                {
                    // print($"Found Match: {currMatch.Count} . Type: {GetGemTypeAt(i, j)}");
                    gemsAlreadyInAMatch.AddRange(currMatch);
                    results.Add(currMatch.ToList());
                }

                // Reset currMatch
                currMatch.Clear();
            }
        }

        // if (results.Count > 0)
        // {
        //     print($"Matches: {results.Count}");
        //     foreach (var result in results)
        //     {
        //         print($"    Type: {grid[result[0].x, result[0].y]} Count: {result.Count}");
        //     }
        // }

        return results.ToList();
    }

    // Returns a list of the neighbor GameObject references of the gems matching
    // the same type of the given gem (excluding the given gem)
    public List<Vector2Int> SearchSameGemTypeNeighborsMock(Vector2Int gemPos, Gem.GemType[,] grid, ref List<Vector2Int> exclude)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        // Check same gem type neighbors within grid range
        foreach (Vector2Int neighbor in new Vector2Int[] {
            gemPos + Vector2Int.up, gemPos + Vector2Int.down, gemPos + Vector2Int.left, gemPos + Vector2Int.right })
        {
            if (neighbor.x >= 0 && neighbor.x < w && neighbor.y >= 0 && neighbor.y < h &&
                grid[gemPos.x, gemPos.y] == grid[neighbor.x, neighbor.y])
            {
                ret.Add(neighbor);
            }

        }

        // Remove exclude matches
        foreach (Vector2Int excludeGemPos in exclude)
        {
            if (ret.Contains(excludeGemPos))
            {
                ret.Remove(excludeGemPos);
            }
        }

        // Update exclude
        exclude.AddRange(ret);

        // Check if neighbors have neighbors of same gem type
        if (ret.Count > 0)
        {

            List<Vector2Int> retFromNeighbor = new List<Vector2Int>();
            foreach (Vector2Int neighbor in ret)
            {
                retFromNeighbor.AddRange(SearchSameGemTypeNeighborsMock(neighbor, grid, ref exclude));
            }
            ret.AddRange(retFromNeighbor);
        }

        return ret;
    }

    // Button On Click GUI callable
    public void PrintMovesLeft()
    {
        if (HasMovesLeft())
            print("There are moves left");
        else
            print("There are no moves left");
    }

    // shifts rows and columns and checks for matches in each iteration
    // Returns true on the first match it finds, or false if there are no matches available.
    public bool HasMovesLeft()
    {
        var mockGrid = GetGGridCopy();
        var w = mockGrid.GetLength(0);
        var h = mockGrid.GetLength(1);

        // check matches by shifting rows
        for (int row = 0; row <= h - 1; row++)
        {
            Gem.GemType[,] shiftedGrid = GetGGridCopy();
            for (int shiftCount = 1; shiftCount < w - 1; shiftCount++)
            {
                // Get next shifted grid
                shiftedGrid = MovedGridLine(shiftedGrid, row, Vector2Int.right);

                // Check for matches with a shifted grid
                var matches = CheckMatches(shiftedGrid);
                // This is returning Count > 0 when it shouldn't
                if (matches.Count > 0)
                {
                    // print($"Still game left (row {row}, shift {shiftCount})");
                    // foreach (var match in matches)
                    // {
                    //     print($"    Type: {shiftedGrid[match[0].x, match[0].y]} Count: {match.Count}");
                    // }
                    return true;
                }
            }
        }

        // Check matches by shifting columns
        for (int col = 0; col <= w - 1; col++)
        {
            Gem.GemType[,] shiftedGrid = GetGGridCopy();
            for (int shiftCount = 1; shiftCount < h - 1; shiftCount++)
            {
                // Get next shifted grid
                shiftedGrid = MovedGridLine(shiftedGrid, col, Vector2Int.up);

                // Check for matches with a shifted grid
                var matches = CheckMatches(shiftedGrid);
                if (matches.Count > 0)
                {
                    // print($"Still game left (col {col} shift {shiftCount})");
                    // foreach (var match in matches)
                    // {
                    //     print($"    Type: {shiftedGrid[match[0].x, match[0].y]} Count: {match.Count}");
                    // }
                    return true;
                }
            }
        }

        return false;
    }

    public Gem.GemType[,] MovedGridLine(Gem.GemType[,] grid, int idx, Vector2Int direction)
    {
        // Maybe unnecessary
        Gem.GemType[,] newGrid = grid.Clone() as Gem.GemType[,];

        if (direction == Vector2Int.right)
        {
            int row = idx; // for clarity

            // Move row right
            Gem.GemType last = newGrid[grid.GetLength(0) - 1, row];
            for (int i = grid.GetLength(0) - 1; i > 0; i--)
            {
                newGrid[i, row] = newGrid[i - 1, row];
            }
            newGrid[0, row] = last;
        }
        else if (direction == Vector2Int.left)
        {
            int row = idx; // for clarity

            // Move row left
            Gem.GemType first = newGrid[0, row];
            for (int i = 0; i <= grid.GetLength(0) - 2; i++)
            {
                newGrid[i, row] = newGrid[i + 1, row];
            }
            newGrid[grid.GetLength(0) - 1, row] = first;
        }
        else if (direction == Vector2Int.up)
        {
            int col = idx;

            // Move col up
            Gem.GemType last = newGrid[col, grid.GetLength(1) - 1];
            for (int j = grid.GetLength(1) - 1; j > 0; j--)
            {
                newGrid[col, j] = newGrid[col, j - 1];
            }
            newGrid[col, 0] = last;
        }
        else if (direction == Vector2Int.down)
        {
            int col = idx;

            // Move col down
            Gem.GemType first = newGrid[col, 0];
            for (int j = 0; j <= grid.GetLength(1) - 2; j++)
            {
                newGrid[col, j] = newGrid[col, j + 1];
            }
            newGrid[col, grid.GetLength(1) - 1] = first;
        }

        return newGrid;
    }

    public Gem.GemType[,] GetGGridCopy()
    {
        Gem.GemType[,] mockGrid = new Gem.GemType[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                mockGrid[i, j] = gGrid[i, j];
            }
        }
        return mockGrid;
    }

    public void UpdateGemTypeGrid(Gem.GemType[,] newGrid)
    {
        gGrid = newGrid;
    }
}
