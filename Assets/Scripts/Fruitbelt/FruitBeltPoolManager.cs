using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// จัดการ object pool ของผลไม้บนสายพาน
/// วางบน Belt หรือ IngredientsBelt
/// </summary>
public class FruitBeltPoolManager : MonoBehaviour
{
    [Header("Fruit Prefab & Pool")]
    [SerializeField] private GameObject fruitPrefab;
    [SerializeField] private Transform  poolParent;
    [SerializeField] private int        poolSizePerFruit = 5;

    [Header("Fruit Data — ลาก FruitData assets ทั้งหมดที่ต้องการใช้")]
    [SerializeField] private FruitData[] fruitDataList;

    [Header("Target Boost — ผลไม้ที่เป็น target จะ spawn บ่อยขึ้น")]
    [SerializeField] private float targetBoostMultiplier = 2f;

    private readonly Dictionary<string, Queue<FruitBeltObject>> _pool = new();

    public FruitData[] AllFruits => fruitDataList;
    public Transform   PoolParent => poolParent;

    public void InitializePool()
{
        foreach (var queue in _pool.Values)
            foreach (var obj in queue)
                if (obj != null) Destroy(obj.gameObject);
        _pool.Clear();

        foreach (var data in fruitDataList)
        {
            var queue = new Queue<FruitBeltObject>();
            for (int i = 0; i < poolSizePerFruit; i++)
                queue.Enqueue(CreateInstance(data));
            _pool[data.fruitId] = queue;
        }
    }

    /// <summary>Spawn ผลไม้แบบ weighted random โดย boost ชนิดที่เป็น target</summary>
    public FruitBeltObject SpawnWeighted(Vector3 position, HashSet<string> targetIds = null)
    {
        float total = 0f;
        foreach (var d in fruitDataList)
        {
            float w = d.spawnWeight;
            if (targetIds != null && targetIds.Contains(d.fruitId)) w *= targetBoostMultiplier;
            total += w;
        }

        float roll       = Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var d in fruitDataList)
        {
            float w = d.spawnWeight;
            if (targetIds != null && targetIds.Contains(d.fruitId)) w *= targetBoostMultiplier;
            cumulative += w;
            if (roll <= cumulative) return Spawn(d, position);
        }

        return Spawn(fruitDataList[0], position);
    }

    public void ReturnToPool(FruitBeltObject obj, FruitBeltActiveZone zone = null)
    {
        if (obj == null) return;

        // cache id ก่อน reset เพราะ ResetObject จะล้าง Data
        string id = obj.Data?.fruitId ?? string.Empty;

        zone?.RemoveFruit(obj);
        obj.ResetObject();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent);

        if (_pool.TryGetValue(id, out var queue))
            queue.Enqueue(obj);
        else
            Destroy(obj.gameObject);
    }

    private FruitBeltObject Spawn(FruitData data, Vector3 position)
    {
        if (!_pool.TryGetValue(data.fruitId, out var queue))
        {
            Debug.LogWarning($"[FruitBeltPool] fruitId '{data.fruitId}' not found in pool");
            return null;
        }

        var obj = queue.Count > 0 ? queue.Dequeue() : CreateInstance(data);
        obj.Initialize(data);
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        return obj;
    }

    private FruitBeltObject CreateInstance(FruitData data)
    {
        var go  = Instantiate(fruitPrefab, poolParent);
        go.SetActive(false);
        var obj = go.GetComponent<FruitBeltObject>();
        obj.Initialize(data);
        return obj;
    }
}
