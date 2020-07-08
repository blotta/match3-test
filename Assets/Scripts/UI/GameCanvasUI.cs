using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvasUI : MonoBehaviour
{
    public void PressedBackToMainMenu()
    {
        GameManager.Instance.LoadMenu();
    }
}
