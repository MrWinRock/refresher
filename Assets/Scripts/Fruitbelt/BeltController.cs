using System.Collections.Generic;
using UnityEngine;

public class BeltController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FruitPoolManager fruitPoolManager;

    [Header("Belt Settings")]
    [SerializeField] private float beltSpeed       = 200f;  // px/sec (UI) หรือ unit/sec (World)
    [SerializeField] private int   visibleSlotCount = 7;
    [SerializeField] private float slotSpacing      = 80f;
    [SerializeField] private Transform spawnPoint;          // ด้านขวาสุด
    [SerializeField] private float despawnX;                // ด้านซ้ายสุด — ผลไม้ที่ผ่านจุดนี้จะ return to pool

    private List<FruitObject> beltFruits = new();
    private bool isRunning = false;

    // ── Public API ───────────────────────────────────────────────

    public void StartBelt()
    {
        isRunning = true;
        // pre-fill belt ให้เต็มก่อนเริ่ม
        for (int i = 0; i < visibleSlotCount; i++)
        {
            Vector3 pos = spawnPoint.position + Vector3.left * slotSpacing * i;
            SpawnNext(pos);
        }
    }

    public void StopBelt() => isRunning = false;

    /// <summary>
    /// คืนผลไม้ที่อยู่ตรงกลาง belt (active slot) — เรียกโดย GameManager ตอน Space กด
    /// </summary>
    public FruitData GetActiveFruit()
    {
        if (beltFruits.Count == 0) return null;
        int midIndex = beltFruits.Count / 2;
        return beltFruits[midIndex]?.Data;
    }

    // ── MonoBehaviour ────────────────────────────────────────────

    private void Update()
    {
        if (!isRunning) return;

        MoveFruits();
        RecycleFruits();
    }

    // ── Private ──────────────────────────────────────────────────

    private void MoveFruits()
    {
        float delta = beltSpeed * Time.deltaTime;
        foreach (var fruit in beltFruits)
        {
            if (fruit == null) continue;
            fruit.transform.position += Vector3.left * delta;
        }
    }

    private void RecycleFruits()
    {
        // เช็คผลไม้ที่เลยขอบซ้ายออกไปแล้ว
        for (int i = beltFruits.Count - 1; i >= 0; i--)
        {
            if (beltFruits[i] == null) { beltFruits.RemoveAt(i); continue; }

            if (beltFruits[i].transform.position.x < despawnX)
            {
                fruitPoolManager.ReturnToPool(beltFruits[i]);
                beltFruits.RemoveAt(i);

                // spawn ใหม่ทางขวา
                SpawnNext(spawnPoint.position);
            }
        }
    }

    private void SpawnNext(Vector3 position)
    {
        FruitObject obj = fruitPoolManager.SpawnWeighted(position);
        if (obj != null) beltFruits.Add(obj);
    }
}