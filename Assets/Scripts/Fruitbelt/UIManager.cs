using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Target Queue — World Space")]
    [SerializeField] private Transform  targetSlotParent;
    [SerializeField] private GameObject targetSlotPrefab;
    [SerializeField] private float      slotSpacing = 1.2f;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer hitFeedback;
    [SerializeField] private SpriteRenderer missFeedback;

    private List<TargetSlot> activeSlots = new();

    public void ShowTargetQueue(FruitData[] queue)
    {
        foreach (Transform child in targetSlotParent)
            Destroy(child.gameObject);
        activeSlots.Clear();

        float totalWidth = (queue.Length - 1) * slotSpacing;
        float startX     = -totalWidth / 2f;

        for (int i = 0; i < queue.Length; i++)
        {
            Vector3 pos = targetSlotParent.position + new Vector3(startX + i * slotSpacing, 0f, 0f);
            GameObject go   = Instantiate(targetSlotPrefab, pos, Quaternion.identity, targetSlotParent);
            TargetSlot slot = go.GetComponent<TargetSlot>();
            slot.Setup(queue[i]);
            activeSlots.Add(slot);
        }
    }

    public void RevealSlot(int index, bool isHit)
    {
        if (index < 0 || index >= activeSlots.Count) return;
        if (isHit) activeSlots[index].Reveal();
        else       activeSlots[index].Miss();
    }

    public void ShowMatchResult(bool isHit)
    {
        if (isHit  && hitFeedback  != null) StartCoroutine(FlashFeedback(hitFeedback));
        if (!isHit && missFeedback != null) StartCoroutine(FlashFeedback(missFeedback));
    }

    public void UpdateStateDisplay(GameState state) { }  // เหลือไว้ให้ GameManager เรียกได้ ไม่ทำอะไร

    private IEnumerator FlashFeedback(SpriteRenderer sr)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        yield return new WaitForSeconds(0.3f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
    }
}