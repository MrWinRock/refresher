using System.Collections.Generic;
using UnityEngine;

public class TargetQueueManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int minCount = 2;
    [SerializeField] private int maxCount = 5;

    [Header("Blacklist — ประเภทที่ห้ามเป็น target")]
    [SerializeField] private FruitType[] blacklistedTypes;  // ลาก enum ใส่ใน Inspector

    [Header("References")]
    [SerializeField] private FruitPoolManager fruitPoolManager;

    private Queue<FruitData> targetQueue = new();

    public void GenerateQueue(int count = -1)
    {
        targetQueue.Clear();

        int size = count > 0 ? count : Random.Range(minCount, maxCount + 1);

        // กรองเอาเฉพาะที่ไม่อยู่ใน blacklist
        FruitData[] allFruits = fruitPoolManager.GetAllowedFruitData(); // ← เปลี่ยนตรงนี้
        FruitData[] allowedFruits = GetAllowedFruits(allFruits);

        if (allowedFruits.Length == 0)
        {
            Debug.LogError("[TargetQueueManager] ไม่มี FruitData ที่ allowed เลย — เช็ก blacklist");
            return;
        }

        for (int i = 0; i < size; i++)
            targetQueue.Enqueue(allowedFruits[Random.Range(0, allowedFruits.Length)]);

        Debug.Log($"[Queue] Generated {size} targets จาก {allowedFruits.Length} ชนิดที่ allowed");
    }

    public FruitData Peek()
    {
        if (targetQueue.Count == 0) return null;
        return targetQueue.Peek();
    }

    public void Dequeue()
    {
        if (targetQueue.Count > 0) targetQueue.Dequeue();
    }

    public FruitData[] GetQueueSnapshot() => targetQueue.ToArray();
    public bool IsEmpty() => targetQueue.Count == 0;
    public int  Count    => targetQueue.Count;

    // ── Helper ───────────────────────────────────────────────────

    private FruitData[] GetAllowedFruits(FruitData[] all)
    {
        var allowed = new List<FruitData>();
        foreach (var data in all)
            if (!IsBlacklisted(data.fruitType))
                allowed.Add(data);
        return allowed.ToArray();
    }

    private bool IsBlacklisted(FruitType type)
    {
        foreach (var t in blacklistedTypes)
            if (t == type) return true;
        return false;
    }
}