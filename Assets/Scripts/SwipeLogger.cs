using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeLogger : MonoBehaviour
{
    void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        
    }

    private void OnDestroy()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
    }

    private void SwipeDetector_OnSwipe(SwipeData data)
    {
        Debug.Log($"Swipe in Direction: {data.Direction} ({data.StartPosition}, {data.EndPosition})");
    }
}
