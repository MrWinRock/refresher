using DG.Tweening;
using UnityEngine;

/// <summary>
/// วางบน Ingredients01–05 ใต้ RecipeRoot
/// ต้องมี SpriteRenderer ติดอยู่บน GameObject เดียวกันหรือ child
/// </summary>
public class FruitBeltIngredientSlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fruitRenderer;

    [Header("Animation")]
    [SerializeField] private float collectPunchStrength = 0.3f;
    [SerializeField] private float collectPunchDuration = 0.35f;
    [SerializeField] private float missShakeDuration    = 0.3f;
    [SerializeField] private float missShakeStrength    = 0.15f;

    private static readonly Color Silhouette = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Vector3 _originLocalPosition;
    private Vector3 _originLocalScale;

    public FruitData TargetFruit { get; private set; }
    public bool IsCollected      { get; private set; }

    private void Awake()
    {
        if (fruitRenderer == null)
            fruitRenderer = GetComponentInChildren<SpriteRenderer>();

        _originLocalPosition = transform.localPosition;
        _originLocalScale    = transform.localScale;
    }

    /// <summary>แสดง silhouette ของส่วนผสมที่ต้องเก็บ</summary>
    public void Setup(FruitData data)
    {
        TargetFruit = data;
        IsCollected = false;
        gameObject.SetActive(true);

        transform.DOKill();
        transform.localPosition = _originLocalPosition;
        
        // Use absolute scale from FruitData to be consistent with belt objects
        transform.localScale = data.recipeScale;

        if (fruitRenderer != null)
{
            fruitRenderer.sprite = data.sprite;
            fruitRenderer.color  = Silhouette;
        }
    }

    /// <summary>เรียกเมื่อ player เลือก ingredient ถูก</summary>
    public void Collect()
    {
        if (IsCollected) return;
        IsCollected = true;

        transform.DOKill();

        if (fruitRenderer != null)
            fruitRenderer.DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad);

        transform.DOPunchScale(Vector3.one * collectPunchStrength, collectPunchDuration, 1, 0.5f);
    }

    /// <summary>เรียกเมื่อ player เลือก ingredient ผิด</summary>
    public void Miss()
    {
        if (IsCollected) return;
        IsCollected = true;

        transform.DOKill();

        if (fruitRenderer != null)
            fruitRenderer.DOColor(new Color(0.8f, 0.2f, 0.2f, 1f), 0.2f).SetEase(Ease.OutQuad);

        transform.DOShakePosition(
            missShakeDuration,
            new Vector3(missShakeStrength, 0f, 0f),
            vibrato:    20,
            randomness: 0f
        ).OnComplete(() => transform.localPosition = _originLocalPosition);
    }

    /// <summary>ซ่อน slot ที่ไม่ได้ใช้ในรอบนี้</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        TargetFruit = null;
        IsCollected = false;
    }
}
