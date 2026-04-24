using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Interfaces.MainMenu
{
    public class SceneTransitionService : MonoBehaviour
    {
        private enum TransitionLookMode
        {
            SolidOrSprite = 0,
            Gradient = 1
        }

        private static SceneTransitionService _instance;

        [Header("Transition Look")]
        [SerializeField] private TransitionLookMode lookMode = TransitionLookMode.SolidOrSprite;
        [SerializeField] private Color transitionColor = Color.black;
        [SerializeField] private Sprite transitionSprite;
        [SerializeField] private Gradient transitionGradient = new Gradient();
        [SerializeField, Min(4)] private int gradientResolution = 256;
        [SerializeField] private bool gradientHorizontal = true;

        [Header("Transition Timing")]
        [SerializeField, Min(0.05f)] private float coverDuration = 0.45f;
        [SerializeField, Min(0.05f)] private float revealDuration = 0.45f;
        [SerializeField] private Ease coverEase = Ease.OutCubic;
        [SerializeField] private Ease revealEase = Ease.InCubic;

        private RectTransform _transitionRect;
        private Image _transitionImage;
        private bool _isTransitioning;
        private Texture2D _generatedGradientTexture;
        private Sprite _generatedGradientSprite;

        public static void TransitionToScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("SceneTransitionService: target scene name is empty.");
                return;
            }

            EnsureInstance().StartTransition(sceneName, null);
        }

        public static void TransitionToScene(string sceneName, PlayableDirector timelineBeforeTransition)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("SceneTransitionService: target scene name is empty.");
                return;
            }

            EnsureInstance().StartTransition(sceneName, timelineBeforeTransition);
        }

        private static SceneTransitionService EnsureInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindFirstObjectByType<SceneTransitionService>();
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("SceneTransitionService");
            _instance = go.AddComponent<SceneTransitionService>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildOverlayIfNeeded();
        }

        private void OnDestroy()
        {
            ReleaseGeneratedGradientResources();
        }

        private void StartTransition(string sceneName, PlayableDirector timelineBeforeTransition)
        {
            if (_isTransitioning)
            {
                return;
            }

            StartCoroutine(PlayTransition(sceneName, timelineBeforeTransition));
        }

        private IEnumerator PlayTransition(string sceneName, PlayableDirector timelineBeforeTransition)
        {
            _isTransitioning = true;
            BuildOverlayIfNeeded();

            if (timelineBeforeTransition != null)
            {
                yield return PlayTimelineAndWait(timelineBeforeTransition);
            }

            float width = UpdateOverlaySize();
            _transitionRect.anchoredPosition = new Vector2(-width, 0f);

            yield return _transitionRect
                .DOAnchorPosX(0f, coverDuration)
                .SetEase(coverEase)
                .SetUpdate(true)
                .WaitForCompletion();

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            if (loadOperation == null)
            {
                Debug.LogError($"SceneTransitionService: could not load scene '{sceneName}'. Check Build Settings.");
                _isTransitioning = false;
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            width = UpdateOverlaySize();
            _transitionRect.anchoredPosition = Vector2.zero;

            yield return _transitionRect
                .DOAnchorPosX(-width, revealDuration)
                .SetEase(revealEase)
                .SetUpdate(true)
                .WaitForCompletion();

            _isTransitioning = false;
        }

        private static IEnumerator PlayTimelineAndWait(PlayableDirector timelineDirector)
        {
            if (timelineDirector == null || !timelineDirector.isActiveAndEnabled || timelineDirector.playableAsset == null)
            {
                yield break;
            }

            bool finished = false;
            void HandleStopped(PlayableDirector stoppedDirector)
            {
                if (stoppedDirector == timelineDirector)
                {
                    finished = true;
                }
            }

            timelineDirector.stopped += HandleStopped;
            timelineDirector.time = 0d;
            timelineDirector.Evaluate();
            timelineDirector.Play();

            while (!finished)
            {
                yield return null;
            }

            timelineDirector.stopped -= HandleStopped;
        }

        private void BuildOverlayIfNeeded()
        {
            if (_transitionRect != null && _transitionImage != null)
            {
                ApplyVisualStyle();
                return;
            }

            var canvasGo = new GameObject("TransitionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var canvasRect = canvas.GetComponent<RectTransform>();

            var imageGo = new GameObject("TransitionImage", typeof(Image));
            imageGo.transform.SetParent(canvasGo.transform, false);

            _transitionRect = imageGo.GetComponent<RectTransform>();
            _transitionRect.anchorMin = new Vector2(0f, 0f);
            _transitionRect.anchorMax = new Vector2(0f, 1f);
            _transitionRect.pivot = new Vector2(0f, 0.5f);

            _transitionImage = imageGo.GetComponent<Image>();
            ApplyVisualStyle();

            float width = Mathf.Max(Screen.width, canvasRect.rect.width);
            _transitionRect.sizeDelta = new Vector2(width, 0f);
            _transitionRect.anchoredPosition = new Vector2(-width, 0f);
        }

        private float UpdateOverlaySize()
        {
            var canvasRect = _transitionRect.parent as RectTransform;
            float width = canvasRect != null ? canvasRect.rect.width : Screen.width;
            width = Mathf.Max(width, 1f);

            _transitionRect.sizeDelta = new Vector2(width, 0f);
            return width;
        }

        private void ApplyVisualStyle()
        {
            if (lookMode == TransitionLookMode.Gradient)
            {
                ApplyGradientStyle();
                return;
            }

            _transitionImage.color = transitionColor;
            _transitionImage.sprite = transitionSprite;
            _transitionImage.type = Image.Type.Simple;
            _transitionImage.preserveAspect = false;
        }

        private void ApplyGradientStyle()
        {
            int safeResolution = Mathf.Max(4, gradientResolution);
            int width = gradientHorizontal ? safeResolution : 1;
            int height = gradientHorizontal ? 1 : safeResolution;

            if (_generatedGradientTexture != null)
            {
                Destroy(_generatedGradientTexture);
                _generatedGradientTexture = null;
            }

            if (_generatedGradientSprite != null)
            {
                Destroy(_generatedGradientSprite);
                _generatedGradientSprite = null;
            }

            _generatedGradientTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] colors = new Color[width * height];
            for (int i = 0; i < safeResolution; i++)
            {
                float t = safeResolution > 1 ? (float)i / (safeResolution - 1) : 0f;
                Color sampled = transitionGradient.Evaluate(t);
                colors[i] = sampled;
            }

            _generatedGradientTexture.SetPixels(colors);
            _generatedGradientTexture.Apply(false, false);

            _generatedGradientSprite = Sprite.Create(
                _generatedGradientTexture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f);

            _transitionImage.color = Color.white;
            _transitionImage.sprite = _generatedGradientSprite;
            _transitionImage.type = Image.Type.Simple;
            _transitionImage.preserveAspect = false;
        }

        private void ReleaseGeneratedGradientResources()
        {
            if (_generatedGradientSprite != null)
            {
                Destroy(_generatedGradientSprite);
                _generatedGradientSprite = null;
            }

            if (_generatedGradientTexture != null)
            {
                Destroy(_generatedGradientTexture);
                _generatedGradientTexture = null;
            }
        }
    }
}

