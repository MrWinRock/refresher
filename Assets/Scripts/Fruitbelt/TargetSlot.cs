using DG.Tweening;
using UnityEngine;

public class TargetSlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fruitRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;

    [Header("Reveal Settings")]
    [SerializeField] private float punchStrength  = 0.4f;
    [SerializeField] private float punchDuration  = 0.35f;
    [SerializeField] private float jumpHeight     = 0.4f;
    [SerializeField] private float jumpDuration   = 0.3f;
    [SerializeField] private float colorDuration  = 0.2f;
    [SerializeField] private float glowDuration   = 0.4f;

    private static readonly Color Silhouette = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Vector3 originPosition;
    private Vector3 originScale;

    private void Awake()
    {
        originPosition = transform.localPosition;
        originScale    = transform.localScale;
    }

    public void Setup(FruitData data)
    {
        transform.DOKill();
        transform.localPosition = originPosition;
        transform.localScale    = originScale;
        transform.localRotation = Quaternion.identity;

        fruitRenderer.sprite = data.sprite;
        fruitRenderer.color  = Silhouette;

        if (glowRenderer != null)
            glowRenderer.color = Color.clear;
    }

    public void Reveal()
    {
        transform.DOKill();
        fruitRenderer.DOKill();

        // 1. สีจาก silhouette → ปกติ
        fruitRenderer.DOColor(Color.white, colorDuration)
            .SetEase(Ease.OutQuad);

        // 2. Punch Scale
        transform.DOPunchScale(
            punch:       Vector3.one * punchStrength,
            duration:    punchDuration,
            vibrato:     1,
            elasticity:  0.5f
        );

        // 3. Jump — ขึ้นแล้วตกกลับตำแหน่งเดิม
        Sequence jumpSeq = DOTween.Sequence();
        jumpSeq.Append(
            transform.DOLocalMoveY(originPosition.y + jumpHeight, jumpDuration * 0.5f)
                     .SetEase(Ease.OutQuad)
        );
        jumpSeq.Append(
            transform.DOLocalMoveY(originPosition.y, jumpDuration * 0.5f)
                     .SetEase(Ease.InQuad)
        );

        // 4. Glow วาบแล้วหาย
        if (glowRenderer != null)
        {
            glowRenderer.DOKill();
            glowRenderer.color = new Color(1f, 1f, 0.5f, 0.8f);
            glowRenderer.DOFade(0f, glowDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    public void Miss()
    {
        transform.DOKill();
        transform.DOShakePosition(
            duration:   0.3f,
            strength:   new Vector3(0.15f, 0f, 0f),
            vibrato:    20,
            randomness: 0f
        ).OnComplete(() => transform.localPosition = originPosition);
    }

    public void ShowWrongFruit(FruitData wrongFruitData)
    {
        if (wrongFruitData == null) return;
        
        transform.DOKill();
        fruitRenderer.sprite = wrongFruitData.sprite;
        fruitRenderer.color  = Color.red;
        
        // Shake animation
        transform.DOShakePosition(
            duration:   0.3f,
            strength:   new Vector3(0.15f, 0f, 0f),
            vibrato:    20,
            randomness: 0f
        ).OnComplete(() => transform.localPosition = originPosition);
    }
}