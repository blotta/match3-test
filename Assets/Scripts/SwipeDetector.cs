using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
    public SwipePhase Phase;
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum SwipePhase
{
    None,
    Swipping,
    Ended
}

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    [SerializeField]
    private float minSwipeDistance = 20f;

    [SerializeField]
    private bool inactiveOnUI = true;

    public static event Action<SwipeData> OnSwipe = delegate { };

    bool dragging = false;

    void Update()
    {
        if (inactiveOnUI && EventSystem.current.currentSelectedGameObject != null)
        {
            // Handling UI
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerUpPosition = touch.position;
                fingerDownPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                fingerDownPosition = touch.position;
                DetectSwipe(SwipePhase.Swipping);
            }

            if (touch.phase == TouchPhase.Ended)
            {
                fingerDownPosition = touch.position;
                DetectSwipe(SwipePhase.Ended);
            }
        }

        // Touch simulation with mouse
        if (Input.GetMouseButtonDown(0))
        {
            fingerUpPosition = Input.mousePosition;
            fingerDownPosition = Input.mousePosition;
            dragging = true;
        }

        if (dragging && Input.GetMouseButton(0))
        {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe(SwipePhase.Swipping);
        }

        if (Input.GetMouseButtonUp(0))
        {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe(SwipePhase.Ended);
            dragging = false;
        }
    }

    private void DetectSwipe(SwipePhase phase)
    {
        if (SwipeDistanceCheckMet())
        {
            if (IsVerticalSwipe())
            {
                SwipeDirection direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                SendSwipe(direction, phase);
            }
            else
            {
                SwipeDirection direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                SendSwipe(direction, phase);
            }
        }
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceCheckMet()
    {
        return VerticalMovementDistance() > minSwipeDistance || HorizontalMovementDistance() > minSwipeDistance;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);
    }

    private void SendSwipe(SwipeDirection direction, SwipePhase phase)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = fingerUpPosition,
            EndPosition = fingerDownPosition,
            Phase = phase
        };
        OnSwipe(swipeData);
    }
}
