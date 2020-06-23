using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public Vector3 originalPosition;
    public Vector2Int gridPos;

    public enum GemType
    {
        MILK,
        APPLE,
        ORANGE,
        BREAD,
        BROCCOLI,
        COCONUT,
        BERRY
    }

    public GemType gemType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
