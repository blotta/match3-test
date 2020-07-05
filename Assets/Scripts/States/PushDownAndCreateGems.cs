using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushDownAndCreateGems : IState
{
    private GameObject[,] _gemGOGrid;
    private List<List<Vector2Int>> _matches;
    private Action _callback;

    public PushDownAndCreateGems(ref GameObject[,] gemGOGrid, List<List<Vector2Int>> matches, Action callback)
    {
        _gemGOGrid = gemGOGrid;
        _matches = matches;
        _callback = callback;
    }

    public void Enter()
    {
    }

    public void Execute()
    {

        for (int i = 0; i < _gemGOGrid.GetLength(0); i++)
        {
            List<GameObject> colGems = new List<GameObject>();
            List<Vector2Int> emptySpots = new List<Vector2Int>();
            for (int j = 0; j < _gemGOGrid.GetLength(1); j++)
            {
                if (_gemGOGrid[i, j] != null)
                {
                    colGems.Add(_gemGOGrid[i, j]);
                }
                else
                {
                    emptySpots.Add(new Vector2Int(i, j));
                }
            }

            // Reposition remaining gems in column (push down)
            // This happens only in the grid (logically) and on each Gem's gridPos.
            // AnimGem state will set GameObject positions later, based on grid position
            for (int j = 0; j < colGems.Count; j++)
            {
                colGems[j].GetComponent<Gem>().gridPos = new Vector2Int(i, j);
                _gemGOGrid[i, j] = colGems[j];
            }

            // Generate new Gems for the available spots (on top)
            for (int j = colGems.Count; j < _gemGOGrid.GetLength(1); j++)
            {
                Vector2Int emptySpot = emptySpots[0];
                emptySpots.Remove(emptySpot);

                GameObject gem = BoardManager.Instance.GemFactory(
                    GemManager.Instance.GetRandomGem().GemType,
                    new Vector2Int(i, j), // emptySpot,
                    BoardManager.GridToWorldPos(new Vector2Int(i, 0)) + Vector3.up * 7);

                _gemGOGrid[i, j] = gem;
            }
        }

        _callback();
    }

    public void Exit()
    {
    }
}
