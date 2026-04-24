using UnityEngine;

public class FruitObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public FruitData Data { get; private set; }

    public void Initialize(FruitData data)
    {
        Data = data;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.sprite;
            spriteRenderer.color  = data.tintColor;
        }
    }

    public void ResetObject()
    {
        Data = null;
        if (spriteRenderer != null) spriteRenderer.sprite = null;
    }
}