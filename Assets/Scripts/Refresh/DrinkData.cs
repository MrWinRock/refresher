using UnityEngine;

namespace Refresh
{
    [CreateAssetMenu(fileName = "DrinkData", menuName = "REFRESH/Drink Data")]
    public class DrinkData : ScriptableObject
    {
        [Header("Display")]
        public Sprite drinkIcon;
    }
}