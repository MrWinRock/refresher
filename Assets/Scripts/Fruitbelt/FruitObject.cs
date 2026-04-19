using UnityEngine;
using UnityEngine.UI;

public class FruitObject : MonoBehaviour
{
    [SerializeField] private Image fruitImage;

    public FruitData Data { get; private set; }

    public void Initialize(FruitData data)
    {
        Data = data;
        if (fruitImage != null)
        {
            fruitImage.sprite = data.sprite;
            fruitImage.color  = data.tintColor;
        }
    }

    public void ResetObject()
    {
        Data = null;
        if (fruitImage != null) fruitImage.sprite = null;
    }
}