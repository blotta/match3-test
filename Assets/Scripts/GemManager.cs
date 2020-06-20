using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour
{
    private static GemManager _instance;
    public static GemManager Instance { get { return _instance; } }

    [SerializeField]
    private List<SOGem> scriptableObjects;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }
    }

    // private void Start()
    // {
    // }

    public SOGem GetGemOfType(Gem.GemType type)
    {
        foreach (SOGem g in scriptableObjects)
        {
            if (g.GemType == type)
                return g;
        }
        return null;
    }

    public SOGem GetRandomGem()
    {
        return scriptableObjects[UnityEngine.Random.Range(0, scriptableObjects.Count)];
    }
}
