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
            Image img = slot.GetComponentInChildren<Image>();
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
            hitFeedback?.SetActive(true);
            Invoke(nameof(HideHitFeedback), 0.4f);
        }
        else
        {
            missFeedback?.SetActive(true);
            Invoke(nameof(HideMissFeedback), 0.4f);
        }
    }

    public void ShowResult(int point, float fever)
    {
        resultPanel?.SetActive(true);
        if (resultScoreText != null) resultScoreText.text = $"Score: {point}";
        if (resultFeverText  != null) resultFeverText.text  = $"Fever: {(fever * 100f):0}%";
        if (feverSlider      != null) feverSlider.value     = fever;
    }

    public void UpdateStateDisplay(GameState state)
    {
        if (stateLabel != null) stateLabel.text = state.ToString();
        if (resultPanel != null) resultPanel.SetActive(state == GameState.Result);
    }

    public void SetInteractable(bool on) { /* ขยายได้ตามต้องการ */ }

    // ── Private ──────────────────────────────────────────────────

    private void HideHitFeedback()  => hitFeedback?.SetActive(false);
    private void HideMissFeedback() => missFeedback?.SetActive(false);
}