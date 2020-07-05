using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerTurn : IState
{
    GameObject[,] _gemGOGrid;
    Rect _worldBoardBound;

    SwipeData _swipeData;

    private GameObject _draggingGem = null;
    private GameObject[] _draggingRowOrCol = null;
    private SwipeDirection _lastSwipeDirection;

    Action _callback;

    public PlayerTurn(GameObject[,] gemGOGrid, Rect worldBoardBound, Action callback)
    {
        _gemGOGrid = gemGOGrid;
        _worldBoardBound = worldBoardBound;
        _callback = callback;
    }

    public void Enter()
    {
        _swipeData.Phase = SwipePhase.None;
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
    }

    public void Execute()
    {
        // Debug.Log($"Swipe Phase: {_swipeData.Phase}");

        if (_swipeData.Phase == SwipePhase.None)
        {
            return;
        }

        if (_swipeData.Phase == SwipePhase.Swipping)
        {
            // print($"Swipping");
            if (_draggingGem == null)
            {
                // Define gem being dragged
                for (var i = 0; i < _gemGOGrid.GetLength(0); i++)
                {
                    for (var j = 0; j < _gemGOGrid.GetLength(1); j++)
                    {
                        var gem = _gemGOGrid[i, j];
                        var gemCollider = gem.GetComponent<BoxCollider2D>();

                        Vector3 worldStartTouchPos = Camera.main.ScreenToWorldPoint(_swipeData.StartPosition);
                        worldStartTouchPos.z = gem.transform.position.z;

                        if (gemCollider.bounds.Contains(worldStartTouchPos))
                        {
                            _draggingGem = gem;
                        }
                    }

                }

                // Now we know the gem and the drag direction.
                // draggingRowOrCol = new GameObject[gemGOGrid.GetLength(0)];
                if (_swipeData.Direction == SwipeDirection.Down || _swipeData.Direction == SwipeDirection.Up)
                {
                    // Col
                    _draggingRowOrCol = BoardManager.GetGemColumnS(_draggingGem, _gemGOGrid);
                }
                else
                {
                    // Row
                    _draggingRowOrCol = BoardManager.GetGemRowS(_draggingGem, _gemGOGrid);
                }
            }
            else
            {
                var worldDeltaDrag = Camera.main.ScreenToWorldPoint(_swipeData.EndPosition) - Camera.main.ScreenToWorldPoint(_swipeData.StartPosition);
                worldDeltaDrag.z = _draggingGem.transform.position.z;

                // Move col or row
                if (_swipeData.Direction == SwipeDirection.Down || _swipeData.Direction == SwipeDirection.Up)
                {
                    // Change draggingRowOrCol if swipe changed directions
                    if (_lastSwipeDirection != SwipeDirection.Down || _lastSwipeDirection != SwipeDirection.Up)
                    {
                        foreach (var gem in _draggingRowOrCol)
                        {
                            // gem.transform.position = gem.GetComponent<Gem>().originalPosition;
                            gem.transform.position = BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos);
                        }
                        _draggingRowOrCol = BoardManager.GetGemColumnS(_draggingGem, _gemGOGrid);
                    }

                    foreach (var gem in _draggingRowOrCol)
                    {
                        var originalPos = BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos);
                        var relPos = ((originalPos.y - _worldBoardBound.y + worldDeltaDrag.y) % _worldBoardBound.height);
                        while (relPos < 0)
                        {
                            relPos += _worldBoardBound.height;
                        }
                        float newYPos = _worldBoardBound.y + relPos;

                        // Research modulus range to make it wrap for positive and negative numbers
                        // without the use of condition
                        gem.transform.position = new Vector3(originalPos.x, newYPos, originalPos.z);
                    }
                }
                else
                {
                    if (_lastSwipeDirection != SwipeDirection.Right || _lastSwipeDirection != SwipeDirection.Left)
                    {
                        foreach (var gem in _draggingRowOrCol)
                        {
                            gem.transform.position = BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos);
                        }
                        _draggingRowOrCol = BoardManager.GetGemRowS(_draggingGem, _gemGOGrid);
                    }

                    foreach (var gem in _draggingRowOrCol)
                    {
                        var originalPos = BoardManager.GridToWorldPos(gem.GetComponent<Gem>().gridPos);
                        var relPos = ((originalPos.x - _worldBoardBound.x + worldDeltaDrag.x) % _worldBoardBound.width);
                        while (relPos < 0)
                        {
                            relPos += _worldBoardBound.width;
                        }
                        float newXPos = _worldBoardBound.x + relPos;
                        gem.transform.position = new Vector3(newXPos, originalPos.y, originalPos.z);
                    }
                }
            }

        }
        else // (_swipeData.Phase == SwipePhase.Ended)
        {
            _swipeData.Phase = SwipePhase.None;

            // Callback here
            _callback();
        }

        _lastSwipeDirection = _swipeData.Direction;
    }

    void SwipeDetector_OnSwipe(SwipeData data)
    {
        _swipeData = data;
    }

    public void Exit()
    {
        // snap to grid
        foreach (var gem in _gemGOGrid)
        {
            gem.transform.position = BoardManager.SnappedGridPos(gem.transform.position);
        }
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
    }
}
