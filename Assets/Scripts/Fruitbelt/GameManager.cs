using System.Collections;
using UnityEngine;

public enum GameState { Idle, Loading, Playing, Result }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FruitPoolManager   fruitPoolManager;
    [SerializeField] private BeltController     beltController;
    [SerializeField] private TargetQueueManager targetQueueManager;
    [SerializeField] private BoostMode          boostMode;   // แทน ScoreManager
    [SerializeField] private UIManager          uiManager;
    [SerializeField] private InputHandler       inputHandler;

    private PointManager pointManager;  // plain class สร้างใน GameLoop

    public GameState CurrentState { get; private set; } = GameState.Idle;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        inputHandler.OnSpacePressed += HandleSpacePressed;
        boostMode.OnBoostStart      += HandleBoostStart;
        boostMode.OnBoostEnd        += HandleBoostEnd;
    }

    void OnDisable()
    {
        inputHandler.OnSpacePressed -= HandleSpacePressed;
        boostMode.OnBoostStart      -= HandleBoostStart;
        boostMode.OnBoostEnd        -= HandleBoostEnd;
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Playing) return;
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        SetState(GameState.Loading);

        fruitPoolManager.InitializePool();
        boostMode.ResetBoost();
        targetQueueManager.GenerateQueue();

        // สร้าง PointManager ใหม่ทุกรอบ โดยใช้จำนวน target เป็น max
        pointManager = new PointManager(targetQueueManager.Count);

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

        // อัปเดต PointManager
        pointManager.AddPoints(isMatch ? 1f : 0f);

        // อัปเดต BoostMode (fever) — hit เท่านั้นที่สะสม boost
        if (isMatch) boostMode.AddBoostPoints(1f);

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

        float fever = pointManager.FeverScore;  // point / object.count
        SetState(GameState.Result);
        uiManager.ShowResult((int)pointManager.TotalPoints, fever);
    }

    private void HandleBoostStart() => uiManager.ShowBoostEffect(true);
    private void HandleBoostEnd()   => uiManager.ShowBoostEffect(false);

    private void SetState(GameState next)
    {
        CurrentState = next;
        uiManager.UpdateStateDisplay(next);
    }
}