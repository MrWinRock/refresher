using NaughtyAttributes;
using UnityEngine;

namespace Refresh
{
    [CreateAssetMenu(fileName = "DrinkData", menuName = "REFRESH/Drink Data")]
    public class DrinkData : ScriptableObject
    {
        [Header("Display")]
        [ShowAssetPreview]
        public Sprite drinkIcon;

        [Header("Serving")]
        [ShowAssetPreview]
        public Sprite servedGlassSprite;
    }
}