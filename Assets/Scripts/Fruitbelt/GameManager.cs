using System;
using System.Collections;
using UnityEngine;

public enum GameState { Idle, Loading, Playing, Result }

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FruitPoolManager   fruitPoolManager;
    [SerializeField] private BeltController     beltController;
    [SerializeField] private ActiveZone         activeZone;
    [SerializeField] private TargetQueueManager targetQueueManager;
    [SerializeField] private BoostMode          boostMode;
    [SerializeField] private UIManager          uiManager;
    [SerializeField] private InputHandler       inputHandler;

    private PointManager pointManager;
    private int currentSlotIndex = 0;   // เพิ่ม field นี้

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
        pointManager = new PointManager(targetQueueManager.Count);

        currentSlotIndex = 0;           // reset ทุกรอบ

        uiManager.ShowTargetQueue(targetQueueManager.GetQueueSnapshot());

        yield return null;

        SetState(GameState.Playing);
        beltController.StartBelt();
        inputHandler.EnableListening();
    }

    private void HandleSpacePressed()
    {
        if (CurrentState != GameState.Playing) return;

        FruitData pressed  = activeZone.GetActiveFruit();
        FruitData expected = targetQueueManager.Peek();

        Debug.Log($"[GameManager] pressed  = {(pressed  != null ? pressed.fruitId  : "NULL")}");
        Debug.Log($"[GameManager] expected = {(expected != null ? expected.fruitId : "NULL")}");



        bool isMatch = pressed != null && expected != null
                       && pressed.fruitId == expected.fruitId;

        pointManager.AddPoints(isMatch ? 1f : 0f);
        if (isMatch) boostMode.AddBoostPoints(1f);

        uiManager.RevealSlot(currentSlotIndex, isMatch, isMatch ? null : pressed);
        targetQueueManager.Dequeue();
        currentSlotIndex++;

        if (targetQueueManager.IsEmpty())
            StartCoroutine(EndGame());
    }

    public event Action<int, float, BoostMode, PointManager> OnGameEnd;

    private IEnumerator EndGame()
    {
        beltController.StopBelt();
        inputHandler.DisableListening();

        yield return new WaitForSeconds(0.3f);

        float fever = pointManager.CalculatePoints();
        SetState(GameState.Result);

        // ส่งทุกอย่างออกไปให้ระบบภายนอกจัดการต่อ
        OnGameEnd?.Invoke((int)pointManager.TotalPoints, fever, boostMode, pointManager);
    }

    private void SetState(GameState next)
    {
        CurrentState = next;
        uiManager.UpdateStateDisplay(next);
    }
}