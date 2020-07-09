using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvasUI : MonoBehaviour
{
    public void PressedBackToMainMenu()
    {
        GameManager.Instance.LoadMenu();
        SoundManager.Instance.PlaySound("select");
    }

    public void PressedNextStage()
    {
        GameManager.Instance.LoadNextStage();
        SoundManager.Instance.PlaySound("select");
    }

    public void PressedRetryStage()
    {
        GameManager.Instance.LoadStage();
        SoundManager.Instance.PlaySound("select");
    }
}
