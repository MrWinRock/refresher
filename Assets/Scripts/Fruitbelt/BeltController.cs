using System.Collections.Generic;
using UnityEngine;

public class BeltController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FruitPoolManager fruitPoolManager;
    [SerializeField] private Camera gameCamera;

    [Header("Belt Settings")]
    [SerializeField] private float beltSpeed = 4f;
    [SerializeField] private int visibleSlotCount = 7;
    [SerializeField] private float slotSpacing = 1.2f;

    [Header("2.5D Settings")]
    [SerializeField] private float beltZ = 0f;
    [SerializeField] private float depthScale = 0.05f;

    private List<FruitObject> beltFruits = new();
    private bool isRunning = false;
    private float spawnX;
    private float despawnX;

    public void StartBelt()
    {
        CalculateScreenBounds();
        isRunning = true;

        for (int i = 0; i < visibleSlotCount; i++)
        {
            float x = spawnX - slotSpacing * i;
            SpawnAt(x);
        }
    }

    public void StopBelt() => isRunning = false;

    private void Update()
    {
        if (!isRunning) return;
        MoveFruits();
        RecycleFruits();
    }

    private void CalculateScreenBounds()
    {
        if (gameCamera == null) gameCamera = Camera.main;

        float camZ = gameCamera.transform.position.z;
        float distToZ = Mathf.Abs(camZ - beltZ);

        Vector3 rightEdge = gameCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, distToZ));
        Vector3 leftEdge  = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, distToZ));

        spawnX   = rightEdge.x + slotSpacing;
        despawnX = leftEdge.x  - slotSpacing;
    }

    private void MoveFruits()
    {
        float delta = beltSpeed * Time.deltaTime;
        foreach (var fruit in beltFruits)
        {
            if (fruit == null) continue;
            fruit.transform.position += Vector3.left * delta;
            Apply25DScale(fruit);
        }
    }

    private void Apply25DScale(FruitObject fruit)
    {
        float distFromCenter = Mathf.Abs(fruit.transform.position.x);
        float scale = Mathf.Clamp(1f - distFromCenter * depthScale, 0.6f, 1f);
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