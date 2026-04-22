using System.Collections.Generic;
using UnityEngine;

public class BeltController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FruitPoolManager fruitPoolManager;
    [SerializeField] private Camera           gameCamera;        // ลาก Main Camera ใส่

    [Header("Belt Settings")]
    [SerializeField] private float beltSpeed        = 4f;        // unit/sec (World Space)
    [SerializeField] private int   visibleSlotCount = 7;
    [SerializeField] private float slotSpacing      = 1.2f;      // ระยะห่างระหว่างผลไม้ (World unit)

    [Header("2.5D Settings")]
    [SerializeField] private float beltZ            = 0f;        // Z ของสายพาน
    [SerializeField] private float depthScale       = 0.05f;     // ผลไม้ไกลกว่าจะเล็กกว่านิดหน่อย

    private List<FruitObject> beltFruits = new();
    private bool isRunning = false;

    private float spawnX;    // ขอบขวา — คำนวณจาก Camera
    private float despawnX;  // ขอบซ้าย — คำนวณจาก Camera

    // ── Public API ───────────────────────────────────────────────

    public void StartBelt()
    {
        CalculateScreenBounds();
        isRunning = true;

        // pre-fill belt ให้เต็มก่อนเริ่ม
        for (int i = 0; i < visibleSlotCount; i++)
        {
            float x = spawnX - slotSpacing * i;
            SpawnAt(x);
        }
    }

    public void StopBelt() => isRunning = false;

    /// <summary>
    /// คืนผลไม้ที่อยู่ใกล้ X=0 ที่สุด (กึ่งกลางหน้าจอ) — เรียกโดย GameManager ตอน Space กด
    /// </summary>
    public FruitData GetActiveFruit()
    {
        if (beltFruits.Count == 0) return null;

        FruitObject closest = null;
        float minDist = float.MaxValue;

        foreach (var fruit in beltFruits)
        {
            if (fruit == null) continue;
            float dist = Mathf.Abs(fruit.transform.position.x);
            if (dist < minDist) { minDist = dist; closest = fruit; }
        }

        return closest?.Data;
    }

    // ── MonoBehaviour ────────────────────────────────────────────

    private void Update()
    {
        if (!isRunning) return;
        MoveFruits();
        RecycleFruits();
    }

    // ── Private ──────────────────────────────────────────────────

    /// <summary>
    /// คำนวณ spawnX / despawnX จาก Camera viewport — รองรับทุก resolution
    /// </summary>
    private void CalculateScreenBounds()
    {
        if (gameCamera == null) gameCamera = Camera.main;

        // แปลง viewport (0,0)-(1,1) เป็น World position ที่ระดับ Z ของ belt
        float camZ    = gameCamera.transform.position.z;
        float distToZ = Mathf.Abs(camZ - beltZ);

        Vector3 rightEdge = gameCamera.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, distToZ));
        Vector3 leftEdge = gameCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distToZ));

        spawnX  = rightEdge.x + slotSpacing;       // spawn นอกขอบขวา
        despawnX = leftEdge.x - slotSpacing;        // despawn นอกขอบซ้าย
    }

    private void MoveFruits()
    {
        float delta = beltSpeed * Time.deltaTime;
        foreach (var fruit in beltFruits)
        {
            if (fruit == null) continue;
            fruit.transform.position += Vector3.left * delta;

            // 2.5D effect — ผลไม้ที่อยู่ไกล X=0 จะเล็กลงนิดหน่อย
            Apply25DScale(fruit);
        }
    }

    private void Apply25DScale(FruitObject fruit)
    {
        float distFromCenter = Mathf.Abs(fruit.transform.position.x);
        float scale = 1f - distFromCenter * depthScale;
        scale = Mathf.Clamp(scale, 0.6f, 1f);
        fruit.transform.localScale = Vector3.one * scale;
    }

    private void RecycleFruits()
    {
        for (int i = beltFruits.Count - 1; i >= 0; i--)
        {
            if (beltFruits[i] == null) { beltFruits.RemoveAt(i); continue; }

            if (beltFruits[i].transform.position.x < despawnX)
            {
                fruitPoolManager.ReturnToPool(beltFruits[i]);
                beltFruits.RemoveAt(i);
                SpawnAt(spawnX);
            }
        }
    }

    private void SpawnAt(float x)
    {
        Vector3 pos = new Vector3(x, transform.position.y, beltZ);
        FruitObject obj = fruitPoolManager.SpawnWeighted(pos);
        if (obj != null) beltFruits.Add(obj);
    }
}