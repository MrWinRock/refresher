using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Target Queue")]
    [SerializeField] private Transform     targetSlotParent;
    [SerializeField] private GameObject    targetSlotPrefab;   // prefab ที่มี Image + outline

    [Header("Score & Fever")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI feverText;
    [SerializeField] private Slider          feverSlider;

    [Header("Feedback")]
    [SerializeField] private GameObject hitFeedback;
    [SerializeField] private GameObject missFeedback;

    [Header("State UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private TextMeshProUGUI resultFeverText;
    [SerializeField] private TextMeshProUGUI stateLabel;

    [Header("Boost / Fever")]
    [SerializeField] private GameObject boostEffectOverlay;  // particle / glow effect


    // ── Public API ───────────────────────────────────────────────

    public void ShowTargetQueue(FruitData[] queue)
    {
        // ล้าง slot เก่า
        foreach (Transform child in targetSlotParent)
            Destroy(child.gameObject);

        // สร้าง slot ใหม่
        foreach (FruitData data in queue)
        {
            GameObject slot = Instantiate(targetSlotPrefab, targetSlotParent);
            SpriteRenderer img = slot.GetComponentInChildren<SpriteRenderer>();
            if (img != null) img.sprite = data.sprite;
        }
    }

    public void ShowBoostEffect(bool isActive)
{
    boostEffectOverlay?.SetActive(isActive);
}
 
    public void ShowMatchResult(bool isHit)
    {
        if (isHit)
        {
            // ตรวจสอบว่ามี Reference หรือไม่ก่อนเรียกใช้งาน
            if (hitFeedback != null) 
            {
                hitFeedback.SetActive(true);
                Invoke(nameof(HideHitFeedback), 0.4f);
            }
        }
        else
        {
            if (missFeedback != null)
            {
                missFeedback.SetActive(true);
                Invoke(nameof(HideMissFeedback), 0.4f);
            }
        }
    }

    public void ShowResult(int point, float fever)
    {
        if (!ValidateResultReferences()) return;
        
        resultPanel.SetActive(true);
        resultScoreText.text = $"Score: {point}";
        resultFeverText.text = $"Fever: {(fever * 100f):0}%";
        if (feverSlider != null) feverSlider.value = fever;
    }

    public void UpdateStateDisplay(GameState state)
    {
        if (!ValidateResultReferences()) return;
        
        stateLabel.text = state.ToString();
        resultPanel.SetActive(state == GameState.Result);
    }

    public void SetInteractable(bool on) { /* ขยายได้ตามต้องการ */ }

    // ── Private ──────────────────────────────────────────────────

    private void HideHitFeedback()  => hitFeedback?.SetActive(false);
    private void HideMissFeedback() => missFeedback?.SetActive(false);

    private bool ValidateResultReferences()
    {
        if (resultPanel == null)
        {
            Debug.LogWarning("[UIManager] resultPanel is not assigned!");
            return false;
        }
        
        if (resultScoreText == null)
        {
            Debug.LogWarning("[UIManager] resultScoreText is not assigned!");
            return false;
        }
        
        if (resultFeverText == null)
        {
            Debug.LogWarning("[UIManager] resultFeverText is not assigned!");
            return false;
        }
        
        if (stateLabel == null)
        {
            Debug.LogWarning("[UIManager] stateLabel is not assigned!");
            return false;
        }
        
        return true;
    }
}