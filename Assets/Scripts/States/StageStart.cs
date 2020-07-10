using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageStart : IState
{
    private IEnumerator _animTextC;
    private RectTransform _stageStartTextT;
    private Action _doneCallback;

    public StageStart(Action doneCallback)
    {
        _doneCallback = doneCallback;
        _stageStartTextT = GameObject.Find("StageStartText").GetComponent<RectTransform>();
        _stageStartTextT.GetComponent<TMP_Text>().text =
            $"STAGE {BoardManager.Instance.StageData.stage}";
    }

    public void Enter()
    {
        _animTextC = animateStageText();
        BoardManager.Instance.StartCoroutine(_animTextC);
    }

    public void Execute()
    {
        if (_animTextC == null)
            _doneCallback();
    }

    IEnumerator animateStageText()
    {
        yield return new WaitForSeconds(1);

        Vector3 startPos = _stageStartTextT.transform.position;
        Vector3 endPos = new Vector3(Screen.width / 2, Screen.height / 2, startPos.z);
        float closeEnoughDist = Vector3.Distance(startPos, endPos)/1000f;

        float speed = 10f;

        while (Vector3.Distance(_stageStartTextT.transform.position, endPos) > closeEnoughDist)
        {
            _stageStartTextT.transform.position = Vector3.Lerp(_stageStartTextT.transform.position, endPos, Time.deltaTime * speed);
            yield return null;
        }
        _stageStartTextT.transform.position = endPos;

        yield return new WaitForSeconds(1);

        while (Vector3.Distance(_stageStartTextT.transform.position, startPos) > closeEnoughDist)
        {
            _stageStartTextT.transform.position = Vector3.Lerp(_stageStartTextT.transform.position, startPos, Time.deltaTime * speed);

            yield return null;
        }

        _stageStartTextT.transform.position = startPos;

        _animTextC = null;
    }

    public void Exit()
    {
    }
}
