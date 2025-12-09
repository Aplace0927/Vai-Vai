using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public double Score { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // 씬 넘어가도 유지하고 싶으면
    }

    public void AddScore(double amount)
    {
        Score += amount;
        Debug.Log(Score);
        // UI 갱신 등
    }

    public void ResetScore()
    {
        Score = 0;
    }
}