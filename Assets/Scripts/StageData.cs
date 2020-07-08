using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StageData
{
    public float score;
    public float countdownSeconds;

    public int stage;
    public float baseTargetScore;
    public float baseTargetScoreMultiplier;

    public float totalCountdownSeconds;
    

    public float targetScore
    {
        get
        {
            return stage * baseTargetScore *baseTargetScoreMultiplier;
        }
    }
}
