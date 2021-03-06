﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BoardUtils
{
    public static Gem.GemType[,] GetNewPlayableGrid(int width, int height)
    {
        Gem.GemType[,] grid = GetRandomGrid(width, height);
        var matches = CheckMatches(grid);
        while (matches.Count > 0 || !HasMovesLeft(grid))
        {
            grid = GetRandomGrid(width, height);
            matches = CheckMatches(grid);
        }
        return grid;
    }

    public static Gem.GemType[,] GetRandomGrid(int width, int height)
    {
        Gem.GemType[,] grid = new Gem.GemType[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                grid[i, j] = GemManager.Instance.GetRandomGem().GemType;
            }
        }
        return grid;
    }

    // Returns a playable grid made with the same given gems, and a dictionary with the previous
    // and new grid positions for each gem (as key and value, respectively)
    // Returns the same grid if given grid has no matches and has moves left
    public static (Gem.GemType[,], Dictionary<Vector2Int, Vector2Int>) ShuffleDiff(Gem.GemType[,] grid, bool forceShuffle = false)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
        Dictionary<Vector2Int, Vector2Int> diffs = new Dictionary<Vector2Int, Vector2Int>();
        Gem.GemType[,] newGrid = (Gem.GemType[,])grid.Clone();
        List<Vector2Int> alreadyAssigned = new List<Vector2Int>();
        var matches = CheckMatches(newGrid);
        if (forceShuffle)
        {
            // "Trick" function to get into the loop
            matches = new List<List<Vector2Int>>() { new List<Vector2Int>() { new Vector2Int(0, 0) } };
        }

        while (matches.Count > 0 || !HasMovesLeft(newGrid))
        {
            // generate new possible grid
            alreadyAssigned.Clear();
            diffs.Clear();
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Vector2Int newPos = new Vector2Int(UnityEngine.Random.Range(0, w), UnityEngine.Random.Range(0, h));
                    while (alreadyAssigned.Contains(newPos))
                    {
                        newPos = new Vector2Int(UnityEngine.Random.Range(0, w), UnityEngine.Random.Range(0, h));
                    }
                    alreadyAssigned.Add(newPos);
                    newGrid[newPos.x, newPos.y] = grid[i, j];
                    diffs.Add(new Vector2Int(i, j), newPos);
                }
            }

            // Test new grid
            matches = CheckMatches(newGrid);
        }

        return (newGrid, diffs);
    }

    public static List<List<Vector2Int>> CheckMatches(Gem.GemType[,] grid)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        List<Vector2Int> currMatch = new List<Vector2Int>();
        List<Vector2Int> gemsAlreadyInAMatch = new List<Vector2Int>();

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
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
                    gemsAlreadyInAMatch.AddRange(currMatch);
                    results.Add(currMatch.ToList());
                }

                // Reset currMatch
                currMatch.Clear();
            }
        }

        return results.ToList();
    }

    // Returns a list of the neighbor GameObject references of the gems matching
    // the same type of the given gem (excluding the given gem)
    public static List<Vector2Int> SearchSameGemTypeNeighborsMock(Vector2Int gemPos, Gem.GemType[,] grid, ref List<Vector2Int> exclude)
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

    // shifts rows and columns and checks for matches in each iteration
    // Returns true on the first match it finds, or false if there are no matches available.
    public static bool HasMovesLeft(Gem.GemType[,] grid)
    {
        var w = grid.GetLength(0);
        var h = grid.GetLength(1);

        // check matches by shifting rows
        for (int row = 0; row <= h - 1; row++)
        {
            for (int shiftCount = 1; shiftCount < w - 1; shiftCount++)
            {
                // Get next shifted grid
                // shiftedGrid = MovedGridLine(shiftedGrid, row, Vector2Int.right);
                grid = MovedGridLine(grid, row, Vector2Int.right);

                // Check for matches with a shifted grid
                var matches = CheckMatches(grid);
                // This is returning Count > 0 when it shouldn't
                if (matches.Count > 0)
                {
                    return true;
                }
            }
            grid = MovedGridLine(grid, row, Vector2Int.right);
        }

        // Check matches by shifting columns
        for (int col = 0; col <= w - 1; col++)
        {
            for (int shiftCount = 1; shiftCount < h - 1; shiftCount++)
            {
                // Get next shifted grid
                grid = MovedGridLine(grid, col, Vector2Int.up);

                // Check for matches with a shifted grid
                var matches = CheckMatches(grid);
                if (matches.Count > 0)
                {
                    return true;
                }
            }
            grid = MovedGridLine(grid, col, Vector2Int.up);
        }

        return false;
    }

    public static Gem.GemType[,] MovedGridLine(Gem.GemType[,] grid, int idx, Vector2Int direction)
    {
        // Maybe unnecessary
        Gem.GemType[,] newGrid = grid.Clone() as Gem.GemType[,];

        if (direction == Vector2Int.right)
        {
            int row = idx;

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
            int row = idx;

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
}
