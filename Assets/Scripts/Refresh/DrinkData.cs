using System.Collections.Generic;
using UnityEngine;

namespace Refresh
{
    public enum IngredientType
    {
        Cherry,
        Lemon,
        Orange,
        Pineapple,
        SeaWeed,
        Shell,
        Coconut,
        Shrimp,
        Squid,
        Stawberry,
        Watermelon
    }

    [CreateAssetMenu(fileName = "DrinkData", menuName = "REFRESH/Drink Data")]
    public class DrinkData : ScriptableObject
    {
        [Header("Display")]
        public string drinkName;
        public Sprite drinkIcon;

        [Header("Recipe")]
        public List<IngredientType> requiredIngredients = new List<IngredientType>();
    }
}