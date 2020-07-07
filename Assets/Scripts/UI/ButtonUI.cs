using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    private Sprite _normalSprite;
    [SerializeField] private Sprite _pressedSprite;

    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        _normalSprite = _button.GetComponent<Image>().sprite;
    }

    public void ChangeSpriteToPressed()
    {
        _button.GetComponent<Image>().sprite = _pressedSprite;
    }

    public void ChangeSpriteToNormal()
    {
        _button.GetComponent<Image>().sprite = _normalSprite;
    }

    public void OnButtonClicked()
    {
        _button.GetComponent<Image>().sprite = _pressedSprite;
        Debug.Log($"Clicked Down");
    }

    public void OnButtonReleased()
    {
        _button.GetComponent<Image>().sprite = _normalSprite;
        Debug.Log($"Released");
    }
}
