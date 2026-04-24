using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// วางบน Slot_Target
/// ต้องมี BoxCollider2D (isTrigger = true) ติดอยู่
/// Physics 2D Layer ของ fruit prefab ต้องชน Layer ของ Slot_Target ด้วย
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class FruitBeltActiveZone : MonoBehaviour
{
    private readonly List<FruitBeltObject> _fruitsInZone = new();

    private void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    /// <summary>คืน FruitBeltObject ที่อยู่ใกล้กึ่งกลาง zone มากที่สุด</summary>
    public FruitBeltObject GetClosestFruit()
    {
        FruitBeltObject closest = null;
        float minDist = float.MaxValue;

        for (int i = _fruitsInZone.Count - 1; i >= 0; i--)
        {
            if (_fruitsInZone[i] == null) { _fruitsInZone.RemoveAt(i); continue; }
            float dist = Mathf.Abs(_fruitsInZone[i].transform.position.x - transform.position.x);
            if (dist < minDist) { minDist = dist; closest = _fruitsInZone[i]; }
        }

        return closest;
    }

    public void RemoveFruit(FruitBeltObject fruit)
    {
        _fruitsInZone.Remove(fruit);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var fruit = other.GetComponent<FruitBeltObject>();
        if (fruit != null && fruit.gameObject.activeInHierarchy)
        {
            string id = fruit.Data != null ? fruit.Data.fruitId : "unknown";
            Debug.Log($"[FruitBelt] Fruit ENTER zone: {id}");
            
            if (!_fruitsInZone.Contains(fruit))
                _fruitsInZone.Add(fruit);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var fruit = other.GetComponent<FruitBeltObject>();
        if (fruit != null)
        {
            // ป้องกันการเข้าถึง Data ถ้า object กำลังถูกทำลาย หรือโดน Reset ไปแล้ว
            string id = "unknown";
            try {
                if (fruit != null && fruit.Data != null) id = fruit.Data.fruitId;
            } catch { }

            Debug.Log($"[FruitBelt] Fruit EXIT zone: {id}");
            _fruitsInZone.Remove(fruit);
        }
    }
}
