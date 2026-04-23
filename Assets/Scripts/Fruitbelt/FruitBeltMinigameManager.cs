using System;
using System.Collections;
using System.Collections.Generic;
using Refresh;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ตัวควบคุมหลักของ FruitBelt Minigame
/// วางบน GameObject ระดับบนสุด (เช่น IngredientsBelt หรือ GameObject ใหม่)
///
/// Flow:
///   StartMinigame()
///     → สุ่ม DrinkData → แสดงบน DrinkRoot
///     → สุ่ม 2-5 FruitData เป็น recipe → แสดง silhouette ใน Ingredients01-05
///     → เริ่ม belt
///     → player กด Space เมื่อ fruit ที่ต้องการอยู่ที่ Slot_Target
///     → เก็บครบ → จบ → ยิง OnMinigameComplete(normalizedScore)
/// </summary>
public class FruitBeltMinigameManager : MonoBehaviour
{
    [Header("Order Display")]
    [SerializeField] private SpriteRenderer drinkRenderer;   // SpriteRenderer บน DrinkRoot
    [SerializeField] private DrinkData[]    availableDrinks; // ลาก DrinkData assets ทั้งหมด

    [Header("Recipe Slots — ลาก Ingredients01–05 ตามลำดับ")]
    [SerializeField] private FruitBeltIngredientSlot[] ingredientSlots;

    [Header("Systems")]
    [SerializeField] private FruitBeltController beltController;
    [SerializeField] private FruitBeltActiveZone activeZone;
    [SerializeField] private FruitBeltPoolManager poolManager;

    [Header("Recipe Settings")]
    [SerializeField] private int minIngredients = 2;
    [SerializeField] private int maxIngredients = 5;

    [Header("Input")]
    [SerializeField] private Key confirmKey = Key.Space;

    [Header("Auto Start")]
    [SerializeField] private bool autoStartOnEnable = false;

    // จำนวนส่วนผสมที่เก็บถูกต้อง (Raw Score) ตามที่ User ต้องการ
    public event Action<float> OnMinigameComplete;
    public bool IsPlaying => _isPlaying;

    private FruitData[] _recipe;
    private int         _currentIndex;
    private int         _correctHits;
    private bool        _isPlaying;
    private bool        _acceptingInput;  // true = พร้อมรับ Space
    private int         _inputReadyFrame; // frame ที่จะเริ่มรับ input
    private bool        _isFreshTimeMode;

    public void SetFreshTimeMode(bool active) => _isFreshTimeMode = active;

    // ── Unity ─────────────────────────────────────────────────────

    private void OnEnable()
    {
        if (autoStartOnEnable)
            StartMinigame();
    }

    private void Update()
    {
        if (!_acceptingInput) return;
        if (Time.frameCount < _inputReadyFrame) return;
        
        bool spacePressed = false;
        if (Keyboard.current != null)
        {
            spacePressed = Keyboard.current[confirmKey].wasPressedThisFrame;
        }
        else
        {
            // Fallback to Legacy Input for projects with Input System issues or "Both" mode
            spacePressed = Input.GetKeyDown(KeyCode.Space);
        }

        if (spacePressed)
        {
            Debug.Log($"[FruitBelt] Input detected (Space) on frame {Time.frameCount}");
            HandleConfirmInput();
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (poolManager    == null) Debug.LogWarning("[FruitBelt] PoolManager ยังไม่ได้ assign",    this);
        if (beltController == null) Debug.LogWarning("[FruitBelt] BeltController ยังไม่ได้ assign", this);
        if (activeZone     == null) Debug.LogWarning("[FruitBelt] ActiveZone ยังไม่ได้ assign",     this);
        if (ingredientSlots == null || ingredientSlots.Length == 0)
            Debug.LogWarning("[FruitBelt] IngredientSlots ว่างอยู่", this);
    }
    #endif

    // ── Public API ────────────────────────────────────────────────

    public void StartMinigame(DrinkData overrideDrink = null)
    {
        if (_isPlaying) return;
        StartCoroutine(MinigameRoutine(overrideDrink));
    }

    public void StopMinigame()
    {
        if (!_isPlaying) return;
        StopAllCoroutines();
        _acceptingInput = false;
        beltController?.StopBelt();
        _isPlaying = false;
    }

    // ── Internal ──────────────────────────────────────────────────

    private IEnumerator MinigameRoutine(DrinkData overrideDrink = null)
    {
        _isPlaying      = true;
        _acceptingInput = false;
        _currentIndex   = 0;
        _correctHits    = 0;

        // ── Guard: ตรวจ reference ที่จำเป็นทั้งหมดก่อนเริ่ม ──────────
        if (poolManager == null)
        {
            Debug.LogError("[FruitBelt] PoolManager ไม่ได้ assign — เช็ค Inspector", this);
            _isPlaying = false; yield break;
        }
        if (beltController == null)
        {
            Debug.LogError("[FruitBelt] BeltController ไม่ได้ assign — เช็ค Inspector", this);
            _isPlaying = false; yield break;
        }
        if (activeZone == null)
        {
            Debug.LogError("[FruitBelt] ActiveZone ไม่ได้ assign — เช็ค Inspector", this);
            _isPlaying = false; yield break;
        }
        if (ingredientSlots == null || ingredientSlots.Length == 0)
        {
            Debug.LogError("[FruitBelt] IngredientSlots ว่างอยู่ — ลาก Ingredients01-05 ใส่ Inspector", this);
            _isPlaying = false; yield break;
        }

        // 1. เลือก DrinkData
        var drink = overrideDrink != null ? overrideDrink : PickRandom(availableDrinks);
        if (drinkRenderer != null)
            drinkRenderer.sprite = drink != null ? drink.drinkIcon : null;

        // 2. สร้าง recipe
        _recipe = GenerateRecipe();
        if (_recipe.Length == 0)
        {
            Debug.LogError("[FruitBelt] ไม่มี FruitData ใน PoolManager — ลาก FruitData assets ใส่ fruitDataList", this);
            _isPlaying = false; yield break;
        }

        // 3. Setup ingredient slots
        var targetIds = new HashSet<string>();
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (ingredientSlots[i] == null) continue;

            if (i < _recipe.Length)
            {
                ingredientSlots[i].Setup(_recipe[i]);
                targetIds.Add(_recipe[i].fruitId);
            }
            else
            {
                ingredientSlots[i].Hide();
            }
        }

        // 4. เริ่ม belt
        poolManager.InitializePool();
        beltController.StartBelt(targetIds);

        // กำหนด frame ที่จะเริ่มรับ input (+2 เพื่อให้พ้น frame ที่กด Space เริ่มเกม)
        // วิธีนี้ทำงานใน Update() ล้วนๆ ไม่ขึ้นกับ Coroutine resume timing
        _inputReadyFrame = Time.frameCount + 2;
        _acceptingInput  = true;

        // 5. รอให้ Update() เก็บครบ
        yield return new WaitUntil(() => _currentIndex >= _recipe.Length);

        // 6. จบ
        _acceptingInput = false;
        yield return new WaitForSeconds(0.5f);
        beltController.StopBelt();
        _isPlaying = false;

        // ส่งคะแนนเป็นจำนวนที่ถูก (Raw Score) เพื่อให้สอดคล้องกับ "สุ่มคะแนนรวมตามจำนวนส่วนผสม"
        OnMinigameComplete?.Invoke((float)_correctHits);
        }

    private void HandleConfirmInput()
    {
        var fruit = activeZone.GetClosestFruit();
        if (fruit == null)
        {
            Debug.Log("[FruitBelt] No fruit found in ActiveZone.");
            return;
        }

        if (fruit.Data == null)
        {
            Debug.Log("[FruitBelt] Fruit found but has NO Data.");
            return;
        }

        var target = _recipe[_currentIndex];
        Debug.Log($"[FruitBelt] Pressed: {fruit.Data.fruitId}, Expected: {target.fruitId}");

        bool isCorrect = _isFreshTimeMode || (fruit.Data.fruitId == target.fruitId);

        if (isCorrect)
        {
            Debug.Log("[FruitBelt] MATCH! (or FreshTime) Collecting...");
            _correctHits++;
            ingredientSlots[_currentIndex].Collect();
        }
        else
        {
            Debug.Log("[FruitBelt] MISMATCH!");
            ingredientSlots[_currentIndex].Miss();
        }

        // เคลียร์ fruit ออกจากสายพาน
        poolManager.ReturnToPool(fruit, activeZone);

        // เลื่อนไปยังส่วนผสมถัดไปเสมอ ไม่ว่าจะถูกหรือผิด
        _currentIndex++;
    }

    private FruitData[] GenerateRecipe()
    {
        var all = poolManager.AllFruits;
        if (all == null || all.Length == 0) return Array.Empty<FruitData>();

        // Filter out blacklisted items: sea weed, shell, shrimp, Squid
        var filtered = new List<FruitData>();
        foreach (var f in all)
        {
            if (f == null) continue;
            if (IsBlacklisted(f)) continue;
            filtered.Add(f);
        }

        if (filtered.Count == 0) return Array.Empty<FruitData>();

        int maxSlots = ingredientSlots != null ? ingredientSlots.Length : maxIngredients;
        int count    = Mathf.Clamp(UnityEngine.Random.Range(minIngredients, maxIngredients + 1), minIngredients, maxSlots);

        // Fisher-Yates shuffle แล้วเอา count ตัวแรก (ซ้ำได้ถ้า all < count)
        var pool     = new List<FruitData>(filtered);
        var result   = new FruitData[count];

        for (int i = 0; i < count; i++)
        {
            int j      = UnityEngine.Random.Range(i % pool.Count, pool.Count);
            (pool[i % pool.Count], pool[j]) = (pool[j], pool[i % pool.Count]);
            result[i]  = pool[i % pool.Count];
        }

        return result;
    }

    private bool IsBlacklisted(FruitData data)
    {
        if (data == null) return true;
        string id = (data.fruitId ?? "").ToLower();
        string name = (data.fruitName ?? "").ToLower();

        // ตรวจสอบทั้ง ID และ Name ว่ามีคำต้องห้ามหรือไม่
        return id.Contains("sea weed") || name.Contains("sea weed") ||
               id.Contains("seaweed") || name.Contains("seaweed") ||
               id.Contains("shell") || name.Contains("shell") ||
               id.Contains("shrimp") || name.Contains("shrimp") ||
               id.Contains("squid") || name.Contains("squid");
    }

        private static T PickRandom<T>(T[] array) where T : class
        {
        if (array == null || array.Length == 0) return null;
        return array[UnityEngine.Random.Range(0, array.Length)];
        }
}
