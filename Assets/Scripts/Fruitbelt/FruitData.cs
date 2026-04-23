using UnityEngine;

public enum FruitType
{
    Normal,
    Bonus,
    Trap,        
    Rare,
}

[CreateAssetMenu(fileName = "FruitData_New", menuName = "Fruit Belt/Fruit Data")]
public class FruitData : ScriptableObject
{
    [Header("Identity")]
    public string    fruitId;
    public string    fruitName;
    public FruitType fruitType = FruitType.Normal;  // ← เพิ่ม

    [Header("Visuals")]
    public Sprite sprite;
    public Color  tintColor = Color.white;

    [Header("Scaling & Rotation")]
    public Vector3 recipeScale = Vector3.one;
    public Vector3 beltScale = Vector3.one;
    public Vector3 beltRotation = Vector3.zero;

    [Header("Belt Settings")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f;
}