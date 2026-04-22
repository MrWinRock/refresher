using System;
using System.Collections;
using UnityEngine;

public class BoostMode : MonoBehaviour
{
    public static BoostMode Instance { get; private set; }  // เปลี่ยนเป็น public เพื่อให้ GameManager เข้าถึงได้

    [SerializeField] private int   defaultThreshold = 5;   // กด hit ครบกี่ครั้งถึง boost
    [SerializeField] private float boostDuration    = 10f;

    private float boostPoints;
    private bool  isBoosting = false;

    public event Action OnBoostStart;   // GameManager / UIManager subscribe ได้
    public event Action OnBoostEnd;

    public float BoostPoints  => boostPoints;
    public bool  IsBoosting   => isBoosting;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddBoostPoints(float points)
    {
        if (isBoosting) return;  // กำลัง boost อยู่ ไม่ต้องนับเพิ่ม

        boostPoints += points;
        if (boostPoints >= defaultThreshold) ApplyBoost();
    }

    public void ResetBoost()
    {
        StopAllCoroutines();
        boostPoints = 0;
        isBoosting  = false;
    }

    private void ApplyBoost()
    {
        isBoosting = true;
        boostPoints = 0;
        OnBoostStart?.Invoke();
        StartCoroutine(BoostCooldown());
    }

    private IEnumerator BoostCooldown()
    {
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
        OnBoostEnd?.Invoke();
    }
}