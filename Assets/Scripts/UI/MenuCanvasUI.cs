﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCanvasUI : MonoBehaviour
{
    [SerializeField] private RectTransform _mainMenuPanel;
    [SerializeField] private RectTransform _stagesMenuPanel;

    private IEnumerator _panelSlideCoroutine = null;

    void Start()
    {
    }

    void Update()
    {
        print($"m {_mainMenuPanel.transform.position} s {_stagesMenuPanel.transform.position}");
    }

    public void SlideMenuPanels(bool slideLeft)
    {
        if (_panelSlideCoroutine != null)
        {
            StopCoroutine(_panelSlideCoroutine);
        }
        _panelSlideCoroutine = SlideMenuPanelsC(slideLeft);
        StartCoroutine(_panelSlideCoroutine);
    }

    IEnumerator SlideMenuPanelsC(bool slideLeft)
    {
        Vector3 targetPos = _mainMenuPanel.transform.position +
            (slideLeft ? Vector3.left * Screen.width :  Vector3.right * Screen.width);

        while (_mainMenuPanel.transform.position != targetPos)
        {
            _mainMenuPanel.transform.position =
                Vector3.Lerp(_mainMenuPanel.transform.position, targetPos, Time.unscaledDeltaTime * 5f);
            _stagesMenuPanel.transform.position = _mainMenuPanel.transform.position + Vector3.right * Screen.width;

            if (Vector3.Distance(_mainMenuPanel.transform.position, targetPos) < 0.5f)
            {
                _mainMenuPanel.transform.position = targetPos;
                _stagesMenuPanel.transform.position = _mainMenuPanel.transform.position + Vector3.right * Screen.width;
            }

            yield return null;
        }

        _panelSlideCoroutine = null;
    }
}