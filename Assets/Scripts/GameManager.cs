using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public int stagesCleared { get; private set; }
    public int currentStage { get; private set; }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        // Load save
        stagesCleared = 0;
        currentStage = stagesCleared + 1;
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }
}
