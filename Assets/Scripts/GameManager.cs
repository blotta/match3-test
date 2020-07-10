using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public int currentStage { get; private set; }

    public static SaveData saveData;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }

        // Load save
        LoadSave();
    }

    private void Start()
    {
        currentStage = LastClearedStage() + 1;
    }

    public void AdjustWorldCamera(Rect bounds)
    {
        GameObject camObj = Camera.main.gameObject;
        WorldCamera wc;
        camObj.TryGetComponent<WorldCamera>(out wc);
        if (wc != null)
            wc.ResetCenterAndWidth(bounds);
    }

    public void StageCleared(StageData stageData)
    {
        var savedStage = GetSaveDataStage(stageData.stage);
        if (savedStage.countdownSeconds < stageData.countdownSeconds)
        {
            SetStageSave(stageData);
            SaveGame();
        }
    }

    void SaveGame()
    {
        var data = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("GameData", data);
    }

    void LoadSave()
    {
        var data = PlayerPrefs.GetString("GameData", "{}");
        print($"Loaded game: {data}");
        saveData = JsonUtility.FromJson<SaveData>(data);
    }

    public static bool HasClearedStage(int t)
    {
        return saveData.stages.Any(s => s.stage == t);
    }

    public static StageData GetSaveDataStage(int s)
    {
        StageData ret = new StageData() { stage = s, countdownSeconds = 0 };
        if (saveData.stages == null || !HasClearedStage(s))
            return ret;

        foreach (var r in saveData.stages)
            if (r.stage == s)
                ret = r;

        return ret;
    }

    public static int LastClearedStage()
    {
        int last = 0;
        foreach (var s in saveData.stages)
            if (s.stage > last)
                last = s.stage;
        return last;
    }

    public static void SetStageSave(StageData stageData)
    {
        if (saveData.stages == null)
            saveData.stages = new List<StageData>();

        saveData.stages.RemoveAll(s => s.stage == stageData.stage);
        saveData.stages.Add(stageData);
        SortSaveDataStages();
    }

    public static void SortSaveDataStages()
    {
        saveData.stages.Sort(
            delegate (StageData a, StageData b)
            {
                return a.stage.CompareTo(b.stage);
            }
        );
    }    

    public void LoadStage(int stage = -1)
    {
        currentStage = stage <= 0 ? currentStage : stage;
        LoadGame();
    }

    public void LoadNextStage()
    {
        currentStage += 1;
        LoadGame();
    }

    public void LoadLastAvailableStage()
    {
        currentStage = LastClearedStage() + 1;
        LoadGame();
    }

    private void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
