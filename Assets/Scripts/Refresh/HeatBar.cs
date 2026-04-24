using System;
using UnityEngine;
using UnityEngine.UI;

namespace Refresh
{
    public class HeatBar : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;

        [Header("Heat")]
        [SerializeField] private float maxHeat = 1f;
        [SerializeField] private float drainRate = 0.1f;
        [SerializeField] private bool startActive;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip potHeatSfx;
        [SerializeField, Range(0, 1)] private float potHeatThreshold = 0.25f;

        private float _currentHeat;
        private bool _isDraining;
        private bool _isPotHeatPlaying;
        private CustomerController _boundCustomer;

        public event Action OnHeatDepleted;

        public float CurrentHeat => _currentHeat;
        public float NormalizedHeat => maxHeat <= 0f ? 0f : _currentHeat / maxHeat;

        private void OnEnable()
        {
            ResetHeat();
            _isDraining = startActive;
        }

        private void Update()
        {
            if (!_isDraining)
            {
                StopPotHeat();
                return;
            }

            _currentHeat -= drainRate * Time.deltaTime;
            UpdateFill();

            // PotHeat logic
            if (!_isPotHeatPlaying && NormalizedHeat <= potHeatThreshold && _currentHeat > 0)
            {
                StartPotHeat();
            }

            if (_currentHeat <= 0f)
            {
                _currentHeat = 0f;
                _isDraining = false;
                StopPotHeat();
                UpdateFill();
                OnHeatDepleted?.Invoke();
                _boundCustomer?.CustomerLeave();
            }
        }

        private void StartPotHeat()
        {
            if (audioSource != null && potHeatSfx != null)
            {
                Debug.Log($"[HeatBar] Starting PotHeat sound. Normalized: {NormalizedHeat}");
                audioSource.clip = potHeatSfx;
                audioSource.loop = true;
                audioSource.Play();
                _isPotHeatPlaying = true;
            }
            else
            {
                Debug.LogWarning($"[HeatBar] Cannot start PotHeat. Source: {audioSource != null}, Clip: {potHeatSfx != null}");
            }
        }

        private void StopPotHeat()
        {
            if (_isPotHeatPlaying)
            {
                Debug.Log("[HeatBar] Stopping PotHeat sound.");
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                _isPotHeatPlaying = false;
            }
        }

        public void Activate(CustomerController customer)
        {
            _boundCustomer = customer;
            ResetHeat();
            _isDraining = true;
            _isPotHeatPlaying = false;
        }

        public void StopDrain()
        {
            _isDraining = false;
            StopPotHeat();
        }

        public void ResetHeat()
        {
            _currentHeat = Mathf.Max(0f, maxHeat);
            UpdateFill();
        }

        private void UpdateFill()
        {
            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = NormalizedHeat;
        }
    }
}

