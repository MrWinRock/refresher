using UnityEngine;

/// <summary>
/// วางบน Prefab ของผลไม้บนสายพาน
/// ต้องมี SpriteRenderer + Collider2D (isTrigger) ติดอยู่ด้วย
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FruitBeltObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public FruitData Data { get; private set; }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void Initialize(FruitData data)
    {
        Data = data;
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color  = data.tintColor;
    }

    public void ResetObject()
    {
        Data = null;
        if (spriteRenderer != null) spriteRenderer.sprite = null;
    }
}
