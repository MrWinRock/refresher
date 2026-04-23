using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] private HeatBar heatBar;
        [SerializeField] private OrderBubble orderBubble;
        [SerializeField] private TextBubble textBubble;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer visualRootSpriteRenderer;

        [Header("Prompt")]
        [SerializeField] private string startMinigamePromptText = "SPACEBAR";

        [Header("Order")]
        [SerializeField] private List<DrinkData> availableDrinks = new List<DrinkData>();

        [Header("Path")]
        [SerializeField] private Transform waitingPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private float spawnOffsetFromWaiting = 8f;

        [Header("Movement")]
        [SerializeField] private float moveDuration = 1.75f;
        [SerializeField] private float satisfiedDelay = 0.75f;
        [SerializeField] private Ease moveEase = Ease.Linear;

        [Header("DOTween Animation")]
        [SerializeField] private Ease enterScaleEase = Ease.OutBack;
        [SerializeField] private float enterScaleDuration = 0.22f;
        [SerializeField] private float walkBobStrength = 0.08f;
        [SerializeField] private float walkBobDuration = 0.18f;
        [SerializeField] private float satisfiedPunchScale = 0.18f;
        [SerializeField] private float satisfiedPunchDuration = 0.3f;
        [SerializeField] private float leaveTiltZ = -14f;
        [SerializeField] private float leaveTiltDuration = 0.2f;

        [Header("Fever Reaction")]
        [SerializeField] private float feverReactionDisplayDuration = 2.0f;
        [SerializeField] private float feverReactionScale = 1.5f;

        private Coroutine _stateRoutine;
        private DrinkData _currentOrder;
        private Tween _moveTween;
        private Tween _walkBobTween;
        private Tween _reactionTween;
        private bool _isInitialized;
        private bool _hasLifecycleStarted;
        private CharacterDefinition _characterData;
        private bool _isFever;

        public CustomerState State { get; private set; }
        public DrinkData CurrentOrder => _currentOrder;

        public event System.Action<CustomerController> OnCustomerLeft;

        private void Start()
{
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            ResolveVisualSpriteRenderer();

            TryStartLifecycle();
        }

        public void Initialize(Transform waiting, Transform exit, CharacterDefinition characterData, bool isFever)
        {
            waitingPoint = waiting;
            exitPoint = exit;
            _characterData = characterData;
            _isFever = isFever;
            _isInitialized = true;

            UpdateVisualState();
            TryStartLifecycle();
        }

        private void UpdateVisualState()
        {
            if (_characterData == null) return;

            Sprite spriteToUse = null;
            if (State == CustomerState.Satisfied || State == CustomerState.Leaving)
            {
                spriteToUse = _isFever ? _characterData.feverHappySprite : _characterData.happySprite;
            }
            else
            {
                spriteToUse = _isFever ? _characterData.feverNormalSprite : _characterData.normalSprite;
            }

            if (spriteToUse == null && _isFever) 
            {
                // Fallback to normal if fever sprite is missing
                spriteToUse = (State == CustomerState.Satisfied || State == CustomerState.Leaving) 
                    ? _characterData.happySprite : _characterData.normalSprite;
            }

            SetVisualSprite(spriteToUse);
        }

        public void SetVisualSprite(Sprite sprite)
        {
            ResolveVisualSpriteRenderer();
            if (visualRootSpriteRenderer == null)
            {
                return;
            }

            visualRootSpriteRenderer.sprite = sprite;
            visualRootSpriteRenderer.enabled = sprite != null;
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
            _hasLifecycleStarted = false;
            KillAllTweens();
            HidePromptBubble();
            HideHeatBar();

            if (heatBar != null)
            {
                heatBar.OnHeatDepleted -= CustomerLeave;
            }
        }

        public void BeginCustomerLifecycle()
        {
            _hasLifecycleStarted = true;
            KillAllTweens();

            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
            }

            _stateRoutine = StartCoroutine(CustomerLifecycleRoutine());
        }

        private void TryStartLifecycle()
        {
            if (_hasLifecycleStarted)
            {
                return;
            }

            // Auto-start works for either explicit spawner initialization or manual scene assignment.
            if (_isInitialized || (waitingPoint != null && exitPoint != null))
            {
                BeginCustomerLifecycle();
            }
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

            OnCustomerLeft?.Invoke(this);

            KillAllTweens();

            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
            }

            _stateRoutine = StartCoroutine(LeaveRoutine());
        }

        private IEnumerator CustomerLifecycleRoutine()
        {
            State = CustomerState.Entering;
            UpdateVisualState();
            _currentOrder = PickRandomDrink();
            HideHeatBar();

            var targetWaitingPosition = GetWaitingPosition();
            var spawnPosition = targetWaitingPosition + Vector3.left * spawnOffsetFromWaiting;
            transform.position = spawnPosition;
            ResetVisualTransform();

            yield return PlayEnterTween();
            StartWalkBob();
            yield return MoveTo(targetWaitingPosition, moveDuration);
            StopWalkBob();

            State = CustomerState.Waiting;
            UpdateVisualState();
            orderBubble?.ShowOrder(_currentOrder);
            textBubble?.ShowText(startMinigamePromptText);
            ShowAndActivateHeatBar();
        }

        private IEnumerator SatisfiedRoutine()
        {
            State = CustomerState.Satisfied;
            heatBar?.StopDrain();
            HideHeatBar();
            HidePromptBubble();
            StopWalkBob();
            
            UpdateVisualState(); // Switch to happy sprite

            if (_isFever && _characterData != null && _characterData.feverReactionActionSprite != null)
            {
                yield return ShowFeverActionReaction();
            }
            else
            {
                yield return PlaySatisfiedTween();
            }

            yield return new WaitForSeconds(satisfiedDelay);
            yield return LeaveRoutine();
        }

        private IEnumerator ShowFeverActionReaction()
        {
            var feverUI = FindFirstObjectByType<FeverActionUI>();
            if (feverUI == null)
            {
                yield return PlaySatisfiedTween();
                yield break;
            }

            if (!string.IsNullOrEmpty(_characterData.feverReactionSceneObjectName))
            {
                yield return feverUI.ShowReactionByName(_characterData.feverReactionSceneObjectName, _characterData.feverReactionActionSprite, feverReactionDisplayDuration);
            }
            else if (_characterData.feverReactionPrefab != null)
            {
                yield return feverUI.ShowReaction(_characterData.feverReactionPrefab, feverReactionDisplayDuration);
            }
            else
            {
                yield return PlaySatisfiedTween();
            }
        }

        private IEnumerator LeaveRoutine()
        {
            State = CustomerState.Leaving;

            heatBar?.StopDrain();
            HideHeatBar();
            orderBubble?.Hide();
            HidePromptBubble();

            yield return PlayLeaveTween();
            StartWalkBob();

            var targetExitPosition = GetExitPosition();
            yield return MoveTo(targetExitPosition, moveDuration);

            StopWalkBob();
            Destroy(gameObject);
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
            KillActiveMoveTween();

            if (duration <= 0f)
            {
                transform.position = targetPosition;
                yield break;
            }

            _moveTween = transform.DOMove(targetPosition, duration).SetEase(moveEase);
            yield return _moveTween.WaitForCompletion();

            transform.position = targetPosition;
            _moveTween = null;
        }

        private void KillActiveMoveTween()
        {
            if (_moveTween == null)
            {
                return;
            }

            if (_moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            _moveTween = null;
        }

        private void KillReactionTween()
        {
            if (_reactionTween == null)
            {
                return;
            }

            if (_reactionTween.IsActive())
            {
                _reactionTween.Kill();
            }

            _reactionTween = null;
        }

        private void StartWalkBob()
        {
            StopWalkBob();

            if (visualRoot == null || walkBobStrength <= 0f || walkBobDuration <= 0f)
            {
                return;
            }

            _walkBobTween = visualRoot.DOLocalMoveY(walkBobStrength, walkBobDuration)
                .SetRelative(true)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void StopWalkBob()
        {
            if (_walkBobTween != null && _walkBobTween.IsActive())
            {
                _walkBobTween.Kill();
            }

            _walkBobTween = null;
            if (visualRoot != null)
            {
                visualRoot.localPosition = Vector3.zero;
            }
        }

        private IEnumerator PlayEnterTween()
        {
            if (visualRoot == null || enterScaleDuration <= 0f)
            {
                yield break;
            }

            KillReactionTween();
            visualRoot.localScale = Vector3.one * 0.85f;
            _reactionTween = visualRoot.DOScale(Vector3.one, enterScaleDuration).SetEase(enterScaleEase);
            yield return _reactionTween.WaitForCompletion();
            _reactionTween = null;
        }

        private IEnumerator PlaySatisfiedTween()
        {
            if (visualRoot == null || satisfiedPunchDuration <= 0f || satisfiedPunchScale <= 0f)
            {
                yield break;
            }

            KillReactionTween();
            _reactionTween = visualRoot.DOPunchScale(Vector3.one * satisfiedPunchScale, satisfiedPunchDuration, 10, 0.8f);
            yield return _reactionTween.WaitForCompletion();
            _reactionTween = null;
        }

        private IEnumerator PlayLeaveTween()
        {
            if (visualRoot == null || leaveTiltDuration <= 0f)
            {
                yield break;
            }

            KillReactionTween();
            _reactionTween = visualRoot.DOLocalRotate(new Vector3(0f, 0f, leaveTiltZ), leaveTiltDuration).SetEase(Ease.OutSine);
            yield return _reactionTween.WaitForCompletion();
            _reactionTween = null;
        }

        private void ResetVisualTransform()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }

        private void KillAllTweens()
        {
            KillActiveMoveTween();
            StopWalkBob();
            KillReactionTween();
            ResetVisualTransform();
        }

        private void ResolveVisualSpriteRenderer()
        {
            if (visualRootSpriteRenderer != null)
            {
                return;
            }

            if (visualRoot == null)
            {
                return;
            }

            visualRootSpriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
            if (visualRootSpriteRenderer == null)
            {
                visualRootSpriteRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void HidePromptBubble()
        {
            textBubble?.Hide();
        }

        private void ShowAndActivateHeatBar()
        {
            if (heatBar == null)
            {
                return;
            }

            heatBar.gameObject.SetActive(true);
            heatBar.Activate(this);
        }

        private void HideHeatBar()
        {
            if (heatBar == null)
            {
                return;
            }

            heatBar.StopDrain();
            heatBar.gameObject.SetActive(false);
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

    }
}

