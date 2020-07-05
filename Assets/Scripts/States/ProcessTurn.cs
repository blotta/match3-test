using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessTurn : IState
{
    GameObject[,] _gemGOGrid;
    Action<List<List<Vector2Int>>> _callback;

    public ProcessTurn(GameObject[,] gemGOGrid, Action<List<List<Vector2Int>>> callback)
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

        // if (matches.Count > 0)
        // {
        //     // update gridPos in each gem because it is a valid move
        //     _gemGOGrid = BoardManager.UpdateGemGOGridFromWorldPos(_gemGOGrid);
        // }

        _callback(matches);
    }

    public void Exit()
    {
    }
}
