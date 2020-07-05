﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteMatches : IState
{
    private GameObject[,] _gemGOGrid;
    private List<List<Vector2Int>> _matches;
    private List<GameObject> _gemsToDelete;
    private IEnumerator _fadeCoroutine;
    private Action<List<List<Vector2Int>>> _doneCallback;

    public DeleteMatches(ref GameObject[,] gemGOGrid, List<List<Vector2Int>> matches, Action<List<List<Vector2Int>>> doneCallback)
    {
        _gemGOGrid = gemGOGrid;
        _matches = matches;
        _doneCallback = doneCallback;

        _gemsToDelete = new List<GameObject>();
        foreach (var group in matches)
        {
            foreach (var gemGridPos in group)
            {
                _gemsToDelete.Add(_gemGOGrid[gemGridPos.x, gemGridPos.y]);
            }
        }
    }

    private IEnumerator FadeGems(List<GameObject> gems)
    {
        List<GameObject> fadedGems = new List<GameObject>();

        while (gems.Count > fadedGems.Count)
        {
            foreach (var gem in gems)
            {
                var sprite = gem.GetComponent<SpriteRenderer>();
                Color newColor = sprite.color;
                if (sprite.color.a <= 0f)
                {
                    continue;
                }
                else if (sprite.color.a <= 0.1f)
                {
                    newColor.a = 0f;
                    fadedGems.Add(gem);
                }
                else
                {
                    newColor.a -= 1f * Time.deltaTime;
                }
                sprite.color = newColor;
            }

            yield return null;
        }

        _fadeCoroutine = null;
    }

    public void Enter()
    {
        _fadeCoroutine = FadeGems(_gemsToDelete);
        // Any MonoBehaviour instance
        GemManager.Instance.StartCoroutine(_fadeCoroutine);
    }

    public void Execute()
    {
        // if animation is done
        if (_fadeCoroutine == null)
        {
            // TODO: Score
            for (int i = 0; i < _gemsToDelete.Count; i++)
            {
                var gridPos = _gemsToDelete[i].GetComponent<Gem>().gridPos;
                _gemGOGrid[gridPos.x, gridPos.y] = null;

                MonoBehaviour.Destroy(_gemsToDelete[i]);
            }


            _doneCallback(_matches);
        }
    }

    public void Exit()
    {
    }
}
