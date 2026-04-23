using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ควบคุมการเคลื่อนที่ของสายพานและ spawn/recycle ผลไม้
/// วางบน Belt GameObject
/// </summary>
public class FruitBeltController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FruitBeltPoolManager poolManager;
    [SerializeField] private FruitBeltActiveZone  activeZone;
    [SerializeField] private Camera               gameCamera;

    [Header("Belt Settings")]
    [SerializeField] private float beltSpeed    = 4f;
    [SerializeField] private int   visibleCount = 7;
    [SerializeField] private float slotSpacing  = 1.5f;
    [SerializeField] private float beltZ        = 0f;

    private readonly List<FruitBeltObject> _beltFruits = new();
    private bool                _isRunning;
    private float               _spawnX;
    private float               _despawnX;
    private HashSet<string>     _targetIds;

    public void StartBelt(HashSet<string> targetIds = null)
    {
        _targetIds = targetIds;
        CalculateBounds();
        _isRunning = true;

        // pre-fill เต็มหน้าจอทันที
        for (int i = 0; i < visibleCount; i++)
            SpawnAt(_spawnX - slotSpacing * i);
    }

    public void StopBelt()
    {
        _isRunning = false;

        for (int i = _beltFruits.Count - 1; i >= 0; i--)
            poolManager.ReturnToPool(_beltFruits[i], activeZone);

        _beltFruits.Clear();
    }

    private void Update()
    {
        if (!_isRunning) return;
        MoveFruits();
        RecycleFruits();
        SpawnNewFruits();
    }

    private void CalculateBounds()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        float dist = Mathf.Abs(gameCamera.transform.position.z - beltZ);
        _spawnX   = gameCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, dist)).x + slotSpacing;
        _despawnX = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, dist)).x - slotSpacing;
    }

    private void MoveFruits()
    {
        float delta = beltSpeed * Time.deltaTime;
        foreach (var fruit in _beltFruits)
        {
            if (fruit == null) continue;
            fruit.transform.position += Vector3.left * delta;
        }
    }

    private void RecycleFruits()
    {
        for (int i = _beltFruits.Count - 1; i >= 0; i--)
        {
            if (_beltFruits[i] == null) { _beltFruits.RemoveAt(i); continue; }

            if (_beltFruits[i].transform.position.x < _despawnX)
            {
                poolManager.ReturnToPool(_beltFruits[i], activeZone);
                _beltFruits.RemoveAt(i);
            }
        }
    }

    private void SpawnNewFruits()
    {
        // หาตำแหน่ง X ของผลไม้ที่อยู่ขวาสุด
        float rightmostX = -9999f;
        foreach (var fruit in _beltFruits)
        {
            if (fruit.transform.position.x > rightmostX)
                rightmostX = fruit.transform.position.x;
        }

        // ถ้าผลไม้ขวาสุดเคลื่อนที่ไปไกลพอแล้ว ให้ spawn ตัวใหม่ที่จุดเดิม (ขยับไปทางขวาตาม spacing)
        // หรือถ้าไม่มีผลไม้เลย ให้เริ่ม spawn ที่จุดเริ่ม
        if (_beltFruits.Count == 0)
        {
            SpawnAt(_spawnX);
        }
        else if (_spawnX - rightmostX >= slotSpacing)
        {
            SpawnAt(_spawnX);
        }
    }

    private void SpawnAt(float x)
    {
        float spawnY = poolManager != null && poolManager.PoolParent != null 
            ? poolManager.PoolParent.position.y 
            : transform.position.y;
            
        var pos = new Vector3(x, spawnY, beltZ);
        var obj = poolManager.SpawnWeighted(pos, _targetIds);
        if (obj != null) _beltFruits.Add(obj);
    }
    }
