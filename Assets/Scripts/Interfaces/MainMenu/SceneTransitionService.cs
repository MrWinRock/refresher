using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionService : MonoBehaviour
{
    private static SceneTransitionService instance;

    [Header("Transition Look")]
    [SerializeField] private Color transitionColor = Color.black;
    [SerializeField] private Sprite transitionSprite;

    [Header("Transition Timing")]
    [SerializeField, Min(0.05f)] private float coverDuration = 0.45f;
    [SerializeField, Min(0.05f)] private float revealDuration = 0.45f;
    [SerializeField] private Ease coverEase = Ease.OutCubic;
    [SerializeField] private Ease revealEase = Ease.InCubic;

    private RectTransform transitionRect;
    private Image transitionImage;
    private bool isTransitioning;

    public static void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneTransitionService: target scene name is empty.");
            return;
        }

        EnsureInstance().StartTransition(sceneName);
    }

    private static SceneTransitionService EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindFirstObjectByType<SceneTransitionService>();
        if (instance != null)
        {
            return instance;
        }

        var go = new GameObject("SceneTransitionService");
        instance = go.AddComponent<SceneTransitionService>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlayIfNeeded();
    }

    private void StartTransition(string sceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(PlayTransition(sceneName));
    }

    private IEnumerator PlayTransition(string sceneName)
    {
        isTransitioning = true;
        BuildOverlayIfNeeded();

        float width = UpdateOverlaySize();
        transitionRect.anchoredPosition = new Vector2(-width, 0f);

        yield return transitionRect
            .DOAnchorPosX(0f, coverDuration)
            .SetEase(coverEase)
            .SetUpdate(true)
            .WaitForCompletion();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            Debug.LogError($"SceneTransitionService: could not load scene '{sceneName}'. Check Build Settings.");
            isTransitioning = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        width = UpdateOverlaySize();
        transitionRect.anchoredPosition = Vector2.zero;

        yield return transitionRect
            .DOAnchorPosX(-width, revealDuration)
            .SetEase(revealEase)
            .SetUpdate(true)
            .WaitForCompletion();

        isTransitioning = false;
    }

    private void BuildOverlayIfNeeded()
    {
        if (transitionRect != null && transitionImage != null)
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

        transitionRect = imageGo.GetComponent<RectTransform>();
        transitionRect.anchorMin = new Vector2(0f, 0f);
        transitionRect.anchorMax = new Vector2(0f, 1f);
        transitionRect.pivot = new Vector2(0f, 0.5f);

        transitionImage = imageGo.GetComponent<Image>();
        ApplyVisualStyle();

        float width = Mathf.Max(Screen.width, canvasRect.rect.width);
        transitionRect.sizeDelta = new Vector2(width, 0f);
        transitionRect.anchoredPosition = new Vector2(-width, 0f);
    }

    private float UpdateOverlaySize()
    {
        var canvasRect = transitionRect.parent as RectTransform;
        float width = canvasRect != null ? canvasRect.rect.width : Screen.width;
        width = Mathf.Max(width, 1f);

        transitionRect.sizeDelta = new Vector2(width, 0f);
        return width;
    }

    private void ApplyVisualStyle()
    {
        transitionImage.color = transitionColor;
        transitionImage.sprite = transitionSprite;
        transitionImage.type = Image.Type.Simple;
        transitionImage.preserveAspect = false;
    }
}

