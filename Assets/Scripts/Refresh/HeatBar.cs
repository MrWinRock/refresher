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

        private float _currentHeat;
        private bool _isDraining;
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
                return;
            }

            _currentHeat -= drainRate * Time.deltaTime;
            UpdateFill();

            if (_currentHeat <= 0f)
            {
                _currentHeat = 0f;
                _isDraining = false;
                UpdateFill();
                OnHeatDepleted?.Invoke();
                _boundCustomer?.CustomerLeave();
            }
        }

        public void Activate(CustomerController customer)
        {
            _boundCustomer = customer;
            ResetHeat();
            _isDraining = true;
        }

        public void StopDrain()
        {
            _isDraining = false;
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

