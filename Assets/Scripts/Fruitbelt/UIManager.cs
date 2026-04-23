using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Recipe Slots")]
    [SerializeField] private TargetSlot[] ingredientSlots;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer hitFeedback;
    [SerializeField] private SpriteRenderer missFeedback;
    
    public void ShowTargetQueue(FruitData[] queue)
    {
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (ingredientSlots[i] == null) continue;

            if (i < queue.Length)
            {
                ingredientSlots[i].gameObject.SetActive(true);
                ingredientSlots[i].Setup(queue[i]);
            }
            else
            {
                ingredientSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void RevealSlot(int index, bool isHit, FruitData wrongFruit = null)
    {
        if (index < 0 || index >= ingredientSlots.Length) return;
        if (ingredientSlots[index] == null) return;

        if (isHit)
            ingredientSlots[index].Reveal();
        else
        {
            if (wrongFruit != null)
                ingredientSlots[index].ShowWrongFruit(wrongFruit);
            else
                ingredientSlots[index].Miss();
        }
    }

    public void ShowMatchResult(bool isHit)
    {
        if (isHit  && hitFeedback  != null) StartCoroutine(FlashFeedback(hitFeedback));
        if (!isHit && missFeedback != null) StartCoroutine(FlashFeedback(missFeedback));
    }

    public void UpdateStateDisplay(GameState state) { }

    private IEnumerator FlashFeedback(SpriteRenderer sr)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        yield return new WaitForSeconds(0.3f);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
    }
}