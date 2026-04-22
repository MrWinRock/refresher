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
    [SerializeField] private BoostMode          boostMode;
    [SerializeField] private UIManager          uiManager;
    [SerializeField] private InputHandler       inputHandler;

    private PointManager pointManager;

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
        targetQueueManager.GenerateQueue();

        // สร้าง PointManager ใหม่ทุกรอบ ใช้จำนวน target เป็น max
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

        // PointManager — เพิ่ม 1 ถ้าถูก, 0 ถ้าผิด
        pointManager.AddPoints(isMatch ? 1f : 0f);

        // BoostMode — เพิ่มเฉพาะตอน hit
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

        // ใช้ CalculatePoints() ตรงๆ ตามที่ PointManager มีให้
        float fever = pointManager.CalculatePoints();
        SetState(GameState.Result);
        uiManager.ShowResult((int)pointManager.TotalPoints, fever);
    }

    private void SetState(GameState next)
    {
        CurrentState = next;
        uiManager.UpdateStateDisplay(next);
    }
}