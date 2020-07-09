using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StagesMenu : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _stageButtonPrefab;

    void Start()
    {
        PopulateContent(GameManager.saveData.stages);
    }

    public void PopulateContent(List<StageData> stagesData)
    {
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= stagesData.Count; i++)
        {
            print($"Setting up button {stagesData[i - 1].stage}");
            int stageNum = stagesData[i - 1].stage;
            var button = Instantiate(_stageButtonPrefab, _content.transform);
            button.name = $"Stage {stageNum}";
            button.GetComponentInChildren<TMP_Text>().text = $"{stageNum}";
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                print($"Trying to load stage {stageNum}");
                GameManager.Instance.LoadStage(stageNum);
            });
        }

        // One more for uncleared/next level 
        var nbutton = Instantiate(_stageButtonPrefab, _content.transform);
        nbutton.name = $"Stage {stagesData.Count + 1}";
        nbutton.GetComponentInChildren<TMP_Text>().text = $"{stagesData.Count + 1}";
        nbutton.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameManager.Instance.LoadStage(stagesData.Count + 1);
            print($"Loading stage {stagesData.Count + 1}");
        });
    }
}
