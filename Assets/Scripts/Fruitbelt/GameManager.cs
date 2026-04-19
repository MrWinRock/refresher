using System.Collections;
using UnityEngine;

public enum GameState { Idle, Loading, Playing, Result }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FruitPoolManager    fruitPoolManager;
    [SerializeField] private BeltController      beltController;
    [SerializeField] private TargetQueueManager  targetQueueManager;
    [SerializeField] private ScoreManager        scoreManager;
    [SerializeField] private UIManager           uiManager;
    [SerializeField] private InputHandler        inputHandler;

    public GameState CurrentState { get; private set; } = GameState.Idle;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()  => inputHandler.OnSpacePressed += HandleSpacePressed;
    void OnDisable() => inputHandler.OnSpacePressed -= HandleSpacePressed;

    public void StartGame()
    {
        if (CurrentState == GameState.Playing) return;
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        SetState(GameState.Loading);
        fruitPoolManager.InitializePool();
        scoreManager.Reset();
        targetQueueManager.GenerateQueue();
        uiManager.ShowTargetQueue(targetQueueManager.GetQueueSnapshot());

        yield return null;

        SetState(GameState.Playing);
        beltController.StartBelt();
        inputHandler.EnableListening();
    }

    private void HandleSpacePressed()
    {
        if (CurrentState != GameState.Playing) return;

        FruitData pressed  = beltController.GetActiveFruit();
        FruitData expected = targetQueueManager.Peek();
        bool isMatch = pressed != null && expected != null
                       && pressed.fruitId == expected.fruitId;

        scoreManager.Register(isMatch);
        uiManager.ShowMatchResult(isMatch);
        targetQueueManager.Dequeue();
        uiManager.ShowTargetQueue(targetQueueManager.GetQueueSnapshot());

        if (targetQueueManager.IsEmpty())
            StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        beltController.StopBelt();
        inputHandler.DisableListening();

        yield return new WaitForSeconds(0.3f);

        float fever = scoreManager.CalculateFever();
        SetState(GameState.Result);
        uiManager.ShowResult(scoreManager.Point, fever);
    }

    private void SetState(GameState next)
    {
        CurrentState = next;
        uiManager.UpdateStateDisplay(next);
    }
}