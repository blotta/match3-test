using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
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
    public Vector2Int gridPos;
}
