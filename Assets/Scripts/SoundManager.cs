using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }

    [SerializeField] private AudioClip _clearSound;
    [SerializeField] private AudioClip _selectSound;
    [SerializeField] private AudioClip _swapSound;

    private AudioSource _backgrondMusic;
    private AudioSource _soundEffects;


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        _backgrondMusic = transform.Find("BackgroundMusic").GetComponent<AudioSource>();
        _soundEffects = transform.Find("SoundEffects").GetComponent<AudioSource>();
    }

    private void Start()
    {
        _backgrondMusic.Play();
    }

    public void PlaySound(string sfx)
    {
        switch (sfx)
        {
            case "clear":
                _soundEffects.PlayOneShot(_clearSound);
                break;
            case "select":
                _soundEffects.PlayOneShot(_selectSound);
                break;
            case "swap":
                _soundEffects.PlayOneShot(_swapSound);
                break;
        }
    }

}
