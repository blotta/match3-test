using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gem", menuName = "Gem", order = 51)]
public class SOGem : ScriptableObject
{
    [SerializeField] private Gem.GemType gemType;
    public Gem.GemType GemType { get { return gemType; } }

    [SerializeField] private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

}
