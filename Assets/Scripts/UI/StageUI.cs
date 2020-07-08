using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageUI : MonoBehaviour
{
    private TMP_Text _stageText;

    void Start()
    {
        _stageText = GetComponent<TMP_Text>();
        _stageText.SetText($"{BoardManager.Instance.StageData.stage}");
    }
}
