using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProcessTurn : IState
{
    GameObject[,] _gemGOGrid;
    Action<List<List<Vector2Int>>> _callback;

    public ProcessTurn(ref GameObject[,] gemGOGrid, Action<List<List<Vector2Int>>> callback)
    {
        _gemGOGrid = gemGOGrid;
        _callback = callback;
    }

    public void Enter()
    {
        Debug.Log("Entered ProcessTurn state");
    }

    public void Execute()
    {
        Gem.GemType[,] attemptGrid = BoardManager.GemArrayFromWorldPos(_gemGOGrid);

        var matches = BoardUtils.CheckMatches(attemptGrid);

        if (matches.Count == 0)
        {
            var currentGrid = BoardManager.GemArrayFromGridPos(_gemGOGrid);
            if (!BoardUtils.HasMovesLeft(currentGrid))
            {
                var (newGrid, diffs) = BoardUtils.ShuffleDiff(currentGrid);
                GameObject[,] newGOGrid = new GameObject[newGrid.GetLength(0), newGrid.GetLength(1)];
                foreach (var diff in diffs)
                {
                    var oldPos = diff.Key;
                    var newPos = diff.Value;
                    newGOGrid[newPos.x, newPos.y] = _gemGOGrid[oldPos.x, oldPos.y];
                    newGOGrid[newPos.x, newPos.y].GetComponent<Gem>().gridPos = newPos;
                    // world position will be updated within the AnimGems state
                }
                _gemGOGrid = newGOGrid;
            }
        }

        _callback(matches);
    }

    public void Exit()
    {
    }
}
