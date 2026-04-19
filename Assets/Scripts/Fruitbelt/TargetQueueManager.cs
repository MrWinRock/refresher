// TargetQueueManager.cs
using System.Collections.Generic;
using UnityEngine;

public class TargetQueueManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int minCount = 2;
    [SerializeField] private int maxCount = 6;

    [Header("References")]
    [SerializeField] private FruitPoolManager fruitPoolManager;

    private Queue<FruitData> targetQueue = new();

    // ── Public API ───────────────────────────────────────────────

    /// <summary>
    /// สุ่มผลไม้แล้วใส่ Queue — เรียกโดย GameManager ตอน Loading
    /// </summary>
    public void GenerateQueue(int count = -1)
    {
        targetQueue.Clear();

        int size = count > 0 ? count : Random.Range(minCount, maxCount + 1);
        FruitData[] allFruits = fruitPoolManager.GetAllFruitData();

        if (allFruits == null || allFruits.Length == 0)
        {
            Debug.LogError("[TargetQueueManager] No FruitData available from pool");
            return;
        }

        for (int i = 0; i < size; i++)
        {
            FruitData picked = allFruits[Random.Range(0, allFruits.Length)];
            targetQueue.Enqueue(picked);
        }

        Debug.Log($"[TargetQueueManager] Queue generated — {size} targets");
    }

    /// <summary>
    /// ดูผลไม้ถัดไปโดยไม่ลบออก — ใช้เปรียบเทียบตอน Space กด
    /// </summary>
    public FruitData Peek()
    {
        if (targetQueue.Count == 0)
        {
            Debug.LogWarning("[TargetQueueManager] Peek() called on empty queue");
            return null;
        }
        return targetQueue.Peek();
    }

    /// <summary>
    /// ลบผลไม้แรกออกจาก Queue — เรียกหลัง GameManager ตรวจสอบ match แล้ว
    /// </summary>
    public void Dequeue()
    {
        if (targetQueue.Count == 0)
        {
            Debug.LogWarning("[TargetQueueManager] Dequeue() called on empty queue");
            return;
        }
        targetQueue.Dequeue();
    }

    /// <summary>
    /// ส่ง snapshot ของ Queue ปัจจุบัน — ใช้โดย UIManager แสดงเป้าหมายทั้งหมด
    /// </summary>
    public FruitData[] GetQueueSnapshot() => targetQueue.ToArray();

    public bool IsEmpty() => targetQueue.Count == 0;
    public int Count     => targetQueue.Count;
}