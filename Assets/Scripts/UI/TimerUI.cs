using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    private int _seconds;
    private TMP_Text _timerText;

    void Start()
    {
        BoardManager.OnTimerUpdated += BoardManager_OnTimerUpdated;
        _seconds = (int)Math.Floor(BoardManager.Instance.countdownSeconds);
        _timerText = GetComponent<TMP_Text>();
        _timerText.SetText(FormattedTime(BoardManager.Instance.countdownSeconds));
    }

    private void BoardManager_OnTimerUpdated(float timer)
    {
        // int seconds = (int)Math.Floor(timer);
        _timerText.SetText(FormattedTime(timer));
    }

    private string FormattedTime(float time)
    {
        var span = TimeSpan.FromSeconds(Mathf.Max(new[] { 0, time }));
        return span.ToString(@"mm\:ss");
    }
}
