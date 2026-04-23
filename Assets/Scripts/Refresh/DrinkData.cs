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

        [Header("Pouring Minigame")]
        public Color particleStartColor = Color.white;
        public float fillSpeed = 0.5f;
        public float perfectTimeSeconds = 5.539817f;
    }
}