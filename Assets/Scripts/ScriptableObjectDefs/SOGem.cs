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

    // [SerializeField] private Sprite milkSprite;
    // [SerializeField] private Sprite appleSprite;
    // [SerializeField] private Sprite orangeSprite;
    // [SerializeField] private Sprite breadSprite;
    // [SerializeField] private Sprite broccoliSprite;
    // [SerializeField] private Sprite coconutSprite;
    // [SerializeField] private Sprite berrySprite;

    // public Sprite GetSprite(Gem.GemType type)
    // {
    //     switch(type)
    //     {
    //         case Gem.GemType.MILK:
    //             return milkSprite;
    //             break;
    //         case Gem.GemType.APPLE:
    //             return appleSprite;
    //             break;
    //         case Gem.GemType.ORANGE:
    //             return orangeSprite;
    //             break;
    //         case Gem.GemType.BREAD:
    //             return orangeSprite;
    //             break;
    //         case Gem.GemType.BROCCOLI:
    //             return broccoliSprite;
    //             break;
    //         case Gem.GemType.COCONUT:
    //             return coconutSprite;
    //             break;
    //         case Gem.GemType.BERRY:
    //             return berrySprite;
    //             break;
    //     }
    // }
}
