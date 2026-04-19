using System.Collections.Generic;
using UnityEngine;

public class FruitPoolManager : MonoBehaviour
{
    [Header("Fruit Data")]
    [SerializeField] private FruitData[] fruitDataList;

    [Header("Pool Settings")]
    [SerializeField] private GameObject fruitPrefab;
    [SerializeField] private int poolSizePerFruit = 5;
    [SerializeField] private Transform poolParent;

    private Dictionary<string, Queue<FruitObject>> pool = new();

    public void InitializePool()
    {
        foreach (var queue in pool.Values)
            foreach (var obj in queue)
                Destroy(obj.gameObject);
        pool.Clear();

        foreach (FruitData data in fruitDataList)
        {
            var queue = new Queue<FruitObject>();
            for (int i = 0; i < poolSizePerFruit; i++)
                queue.Enqueue(CreateInstance(data));
            pool[data.fruitId] = queue;
        }
    }

    public FruitObject Spawn(string fruitId, Vector3 position)
    {
        if (!pool.TryGetValue(fruitId, out var queue))
        {
            Debug.LogWarning($"[Pool] fruitId '{fruitId}' not found");
            return null;
        }
        FruitObject obj = queue.Count > 0 ? queue.Dequeue() : ExpandPool(fruitId);
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public FruitObject SpawnWeighted(Vector3 position)
    {
        float totalWeight = 0f;
        foreach (var d in fruitDataList) totalWeight += d.spawnWeight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var d in fruitDataList)
        {
            cumulative += d.spawnWeight;
            if (roll <= cumulative) return Spawn(d.fruitId, position);
        }
        return Spawn(fruitDataList[0].fruitId, position);
    }

    public void ReturnToPool(FruitObject obj)
    {
        if (obj == null) return;
        obj.ResetObject();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent);
        if (pool.TryGetValue(obj.Data?.fruitId ?? "", out var queue))
            queue.Enqueue(obj);
        else
            Destroy(obj.gameObject);
    }

    public FruitData[] GetAllFruitData() => fruitDataList;

    private FruitObject CreateInstance(FruitData data)
    {
        var go = Instantiate(fruitPrefab, poolParent);
        go.SetActive(false);
        var obj = go.GetComponent<FruitObject>();
        obj.Initialize(data);
        return obj;
    }

    private FruitObject ExpandPool(string fruitId)
    {
        FruitData data = System.Array.Find(fruitDataList, d => d.fruitId == fruitId);
        Debug.LogWarning($"[Pool] Expanding pool for '{fruitId}'");
        return CreateInstance(data);
    }
}