using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    private TMP_Text _timerText;

    void Start()
    {
        BoardManager.OnTimerUpdated += BoardManager_OnTimerUpdated;
        _timerText = GetComponent<TMP_Text>();
        _timerText.SetText(FormattedTime(BoardManager.Instance.StageData.totalCountdownSeconds));
    }

    private void OnDestroy()
    {
        BoardManager.OnTimerUpdated -= BoardManager_OnTimerUpdated;
    }

    private void BoardManager_OnTimerUpdated()
    {
        _timerText.SetText(FormattedTime(BoardManager.Instance.StageData.countdownSeconds));
    }

    private string FormattedTime(float time)
    {
        var span = TimeSpan.FromSeconds(Mathf.Max(new[] { 0, time }));
        return span.ToString(@"m\:ss");
    }
}
