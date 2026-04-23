using UnityEngine;
using System.Collections.Generic;

namespace Refresh
{
    [System.Serializable]
    public class CharacterDefinition
    {
        public string characterName;
        public Sprite normalSprite;
        public Sprite happySprite;
        
        [Header("Fever Settings")]
        public bool hasFeverVersion;
        public Sprite feverNormalSprite;
        public Sprite feverHappySprite;
        public Sprite feverReactionActionSprite; // The "Fresh" action sprite
    }

    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Refresh/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        public List<CharacterDefinition> characters = new List<CharacterDefinition>();
    }
}
