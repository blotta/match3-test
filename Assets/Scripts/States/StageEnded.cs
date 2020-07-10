using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEnded : IState
{
    private IEnumerator _animPanelC;
    private GameObject _stageEndedPanel;
    private bool _success;

    public StageEnded(bool success)
    {
        _success = success;
    }

    public void Enter()
    {
        if (_success)
            _stageEndedPanel = GameObject.Find("StageClearedPanel");
        else
            _stageEndedPanel = GameObject.Find("StageFailedPanel");

        _stageEndedPanel.SetActive(true);

        _animPanelC = CenterPanel(_stageEndedPanel);
        BoardManager.Instance.StartCoroutine(_animPanelC);

        if (_success)
        {
            SoundManager.Instance.PlaySound("clear");
            GameManager.Instance.StageCleared(BoardManager.Instance.StageData);
        }
        else
        {
            SoundManager.Instance.PlaySound("swap");
        }
    }

    public void Execute()
    {
    }

    IEnumerator CenterPanel(GameObject panel)
    {
        Vector3 targetPos = new Vector3(Screen.width / 2, Screen.height / 2, panel.transform.position.z);
        float closeEnoughDist = Vector3.Distance(panel.transform.position, targetPos)  / 1000f;
        float speed = 10f;

        while (Vector3.Distance(panel.transform.position, targetPos) > closeEnoughDist)
        {
            panel.transform.position =
                Vector3.Lerp(panel.transform.position, targetPos, Time.unscaledDeltaTime * speed);

            yield return null;
        }

        panel.transform.position = targetPos;

        _animPanelC = null;
    }

    public void Exit()
    {
    }
}
