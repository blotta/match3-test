using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageStart : IState
{
    private IEnumerator _waitC;
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
        _waitC = animateStageText();
        BoardManager.Instance.StartCoroutine(_waitC);
    }

    public void Execute()
    {
        if (_waitC == null)
            _doneCallback();
    }

    IEnumerator animateStageText()
    {
        yield return new WaitForSeconds(1);

        Vector3 startPos = _stageStartTextT.transform.position;
        Vector3 endPos = new Vector3(Screen.width / 2, Screen.height / 2, startPos.z);

        float speed = 10f;

        while (_stageStartTextT.transform.position != endPos)
        {
            _stageStartTextT.transform.position = Vector3.Lerp(_stageStartTextT.transform.position, endPos, Time.deltaTime * speed);
            if (Vector3.Distance(_stageStartTextT.transform.position, endPos) < 0.05f)
            {
                _stageStartTextT.transform.position = endPos;
            }
            yield return null;
        }

        yield return new WaitForSeconds(1);

        while (_stageStartTextT.transform.position != startPos)
        {
            _stageStartTextT.transform.position = Vector3.Lerp(_stageStartTextT.transform.position, startPos, Time.deltaTime * speed);
            if (Vector3.Distance(_stageStartTextT.transform.position, startPos) < 0.05f)
            {
                _stageStartTextT.transform.position = startPos;
            }
            yield return null;
        }

        _waitC = null;
    }

    public void Exit()
    {
    }
}
