using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Refresh
{
    public class BoostBar : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject freshTimeIndicator;

        private void Awake()
        {
            if (fillImage != null)
            {
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
            }
        }

        private void OnEnable()
        {
            BoostMode.BoostPointsChanged += OnBoostPointsChanged;
            BoostMode.BoostActivated += OnBoostActivated;
            BoostMode.BoostEnded += OnBoostEnded;
            SyncWithCurrentState();
        }

        private void OnDisable()
        {
            BoostMode.BoostPointsChanged -= OnBoostPointsChanged;
            BoostMode.BoostActivated -= OnBoostActivated;
            BoostMode.BoostEnded -= OnBoostEnded;
        }

        private void SyncWithCurrentState()
        {
            var boostMode = BoostMode.Instance;
            if (boostMode == null) boostMode = FindFirstObjectByType<BoostMode>();
            if (boostMode == null) return;

            UpdateFill(boostMode.BoostPoints, boostMode.Threshold);
            SetFreshTimeIndicator(boostMode.IsBoostActive);
        }

        private void OnBoostPointsChanged(float current, float max)
        {
            UpdateFill(current, max);
        }

        private void OnBoostActivated()
        {
            SetFreshTimeIndicator(true);
        }

        private void OnBoostEnded()
        {
            UpdateFill(0f, 1f);
            SetFreshTimeIndicator(false);
        }

        private void UpdateFill(float current, float max)
        {
            if (fillImage == null) return;
            fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (scoreText != null)
                scoreText.text = $"Boost: {current:F1} / {max:F1}";
        }

        private void SetFreshTimeIndicator(bool active)
        {
            if (freshTimeIndicator != null)
                freshTimeIndicator.SetActive(active);
        }
    }
}
