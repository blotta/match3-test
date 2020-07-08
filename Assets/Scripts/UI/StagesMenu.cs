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
        PopulateContent();
    }

    public void PopulateContent()
    {
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= GameManager.Instance.stagesCleared + 1; i++)
        {
            var button = Instantiate(_stageButtonPrefab, _content.transform);
            button.name = $"Stage {i}";
            button.GetComponentInChildren<TMP_Text>().text = $"{i}";
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameManager.Instance.LoadStage(i);
            });
        }
    }
}
