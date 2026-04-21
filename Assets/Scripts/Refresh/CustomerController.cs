using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refresh
{
    public class CustomerController : MonoBehaviour
    {
        public enum CustomerState
        {
            Entering,
            Waiting,
            Leaving,
            Satisfied
        }

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private HeatBar heatBar;
        [SerializeField] private OrderBubble orderBubble;

        [Header("Order")]
        [SerializeField] private List<DrinkData> availableDrinks = new List<DrinkData>();

        [Header("Path")]
        [SerializeField] private Transform waitingPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private float spawnOffsetFromWaiting = 8f;

        [Header("Movement")]
        [SerializeField] private float moveDuration = 1.75f;
        [SerializeField] private float satisfiedDelay = 0.75f;

        [Header("Animator Parameters")]
        [SerializeField] private string movingBool = "IsMoving";
        [SerializeField] private string enterTrigger = "Enter";
        [SerializeField] private string leaveTrigger = "Leave";
        [SerializeField] private string satisfiedTrigger = "Satisfied";

        private Coroutine _stateRoutine;
        private DrinkData _currentOrder;

        public CustomerState State { get; private set; }
        public DrinkData CurrentOrder => _currentOrder;

        private void Start()
        {
            BeginCustomerLifecycle();
        }

        private void OnEnable()
        {
            if (heatBar != null)
            {
                heatBar.OnHeatDepleted += CustomerLeave;
            }
        }

        private void OnDisable()
        {
            if (heatBar != null)
            {
                heatBar.OnHeatDepleted -= CustomerLeave;
            }
        }

        public void BeginCustomerLifecycle()
        {
            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
            }

            _stateRoutine = StartCoroutine(CustomerLifecycleRoutine());
        }

        public bool TryServeDrink(DrinkData servedDrink)
        {
            if (State != CustomerState.Waiting || servedDrink == null)
            {
                return false;
            }

            if (servedDrink != _currentOrder)
            {
                return false;
            }

            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
            }

            _stateRoutine = StartCoroutine(SatisfiedRoutine());
            return true;
        }

        public void CustomerLeave()
        {
            if (State == CustomerState.Leaving)
            {
                return;
            }

            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
            }

            _stateRoutine = StartCoroutine(LeaveRoutine());
        }

        private IEnumerator CustomerLifecycleRoutine()
        {
            State = CustomerState.Entering;
            _currentOrder = PickRandomDrink();

            var targetWaitingPosition = GetWaitingPosition();
            var spawnPosition = targetWaitingPosition + Vector3.left * spawnOffsetFromWaiting;
            transform.position = spawnPosition;

            SetMovingAnimation(true);
            SetTrigger(enterTrigger);
            yield return MoveTo(targetWaitingPosition, moveDuration);
            SetMovingAnimation(false);

            State = CustomerState.Waiting;
            orderBubble?.ShowOrder(_currentOrder);
            heatBar?.Activate(this);
        }

        private IEnumerator SatisfiedRoutine()
        {
            State = CustomerState.Satisfied;
            heatBar?.StopDrain();
            SetTrigger(satisfiedTrigger);
            yield return new WaitForSeconds(satisfiedDelay);
            yield return LeaveRoutine();
        }

        private IEnumerator LeaveRoutine()
        {
            State = CustomerState.Leaving;

            heatBar?.StopDrain();
            orderBubble?.Hide();

            SetTrigger(leaveTrigger);
            SetMovingAnimation(true);

            var targetExitPosition = GetExitPosition();
            yield return MoveTo(targetExitPosition, moveDuration);

            SetMovingAnimation(false);
            gameObject.SetActive(false);
        }

        private DrinkData PickRandomDrink()
        {
            if (availableDrinks == null || availableDrinks.Count == 0)
            {
                return null;
            }

            var valid = new List<DrinkData>();
            for (var i = 0; i < availableDrinks.Count; i++)
            {
                if (availableDrinks[i] != null)
                {
                    valid.Add(availableDrinks[i]);
                }
            }

            if (valid.Count == 0)
            {
                return null;
            }

            var index = Random.Range(0, valid.Count);
            return valid[index];
        }

        private IEnumerator MoveTo(Vector3 targetPosition, float duration)
        {
            var startPosition = transform.position;
            var elapsed = 0f;

            if (duration <= 0f)
            {
                transform.position = targetPosition;
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
        }

        private Vector3 GetWaitingPosition()
        {
            if (waitingPoint != null)
            {
                return waitingPoint.position;
            }

            return transform.position;
        }

        private Vector3 GetExitPosition()
        {
            if (exitPoint != null)
            {
                return exitPoint.position;
            }

            return GetWaitingPosition() + Vector3.right * spawnOffsetFromWaiting;
        }

        private void SetMovingAnimation(bool isMoving)
        {
            if (animator == null || string.IsNullOrWhiteSpace(movingBool))
            {
                return;
            }

            animator.SetBool(movingBool, isMoving);
        }

        private void SetTrigger(string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return;
            }

            animator.SetTrigger(triggerName);
        }
    }
}

