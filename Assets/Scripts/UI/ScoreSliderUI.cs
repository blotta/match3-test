﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSliderUI : MonoBehaviour
{
    Slider _slider;
    float _scorePercent;

    void Start()
    {
        BoardManager.OnScoreUpdated += BoardManager_OnScoreUpdated;
        _scorePercent = 0;
        _slider = GetComponent<Slider>();
        _slider.value = 0f;
    }

    private void OnDestroy()
    {
        BoardManager.OnScoreUpdated -= BoardManager_OnScoreUpdated;
    }

    private void BoardManager_OnScoreUpdated()
    {
        _scorePercent = BoardManager.Instance.StageData.score / BoardManager.Instance.StageData.targetScore;
    }

    void Update()
    {
        if (_scorePercent != _slider.value)
        {
            _slider.value = Mathf.Lerp(_slider.value, _scorePercent, Time.deltaTime * 2f);
            if (Mathf.Abs(_slider.value - _scorePercent) <= 0.002f)
            {
                _slider.value = Mathf.Clamp(_scorePercent, 0, 1);
            }
        }
    }
}
