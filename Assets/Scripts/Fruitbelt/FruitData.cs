// FruitData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FruitData_New", menuName = "Fruit Belt/Fruit Data")]
public class FruitData : ScriptableObject
{
    [Header("Identity")]
    public string fruitId;        // unique key — ใช้เปรียบเทียบใน GameManager
    public string fruitName;      // ชื่อแสดงใน UI

    [Header("Visuals")]
    public Sprite sprite;         // รูปผลไม้
    public Color tintColor = Color.white;  // สีเสริม ถ้าต้องการ tint

    [Header("Belt Settings")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f;  // โอกาสออกบนสายพาน — ยิ่งมากยิ่งออกบ่อย
}