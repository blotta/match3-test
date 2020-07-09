using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimGems : IState
{

    GameObject[,] _gemGOGrid;
    Action _callback;

    // bool _animationDone = false;

    public AnimGems(GameObject[,] gemGOGrid, Action callback)
    {
        _gemGOGrid = gemGOGrid;
        _callback = callback;
    }

    public void Enter()
    {
        Debug.Log("Entered AnimGems state");
    }

    public void Execute()
    {
        bool animDone = true;
        foreach (var gem in _gemGOGrid)
        {
            if (gem.transform.position != BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos))
            {
                animDone = false;
            }
        }

        if (animDone)
        {
            _callback();
            return;
        }

        foreach (var gem in _gemGOGrid)
        {
            Vector3 targetPos = BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos);
            if (gem.transform.position == targetPos)
            {
                continue;
            }

            if (Vector2.Distance(targetPos, gem.transform.position) < 0.1f)
            {
                gem.transform.position = targetPos;
            }
            else
            {
                // Move half a unit/sec
                Vector3 newPos = gem.transform.position + (targetPos - gem.transform.position).normalized * Time.deltaTime * 25f;
                gem.transform.position = newPos;
            }
        }
    }

    public void Exit()
    {
    }
}
