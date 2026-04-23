using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ActiveZone : MonoBehaviour
{
    private List<FruitObject> fruitsInZone = new();

    public FruitData GetActiveFruit()
    {
        if (fruitsInZone.Count == 0) return null;

        FruitObject closest = null;
        float minDist = float.MaxValue;

        foreach (var fruit in fruitsInZone)
        {
            if (fruit == null) continue;
            float dist = Mathf.Abs(fruit.transform.position.x - transform.position.x);
            if (dist < minDist) { minDist = dist; closest = fruit; }
        }

        return closest?.Data;
    }

private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"[ActiveZone] OnTriggerEnter2D: {other.gameObject.name}");

    FruitObject fruit = other.GetComponent<FruitObject>();
    if (fruit == null)
    {
        Debug.LogWarning($"[ActiveZone] ไม่เจอ FruitObject บน {other.gameObject.name}");
        return;
    }

    if (!fruitsInZone.Contains(fruit))
        fruitsInZone.Add(fruit);

    Debug.Log($"[ActiveZone] fruitsInZone.Count = {fruitsInZone.Count}");
}

private void OnTriggerExit2D(Collider2D other)
{
    Debug.Log($"[ActiveZone] OnTriggerExit2D: {other.gameObject.name}");
    FruitObject fruit = other.GetComponent<FruitObject>();
    if (fruit != null) fruitsInZone.Remove(fruit);
}

    public void RemoveFruit(FruitObject fruit) => fruitsInZone.Remove(fruit);
}