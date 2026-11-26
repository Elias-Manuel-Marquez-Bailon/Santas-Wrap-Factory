using System;
using UnityEngine;

/// <summary>
/// Lleva el puntaje del nivel actual.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;

    void Awake() { Instance = this; }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Add(int amount)
    {
        Score = Mathf.Max(0, Score + amount); // no baja de 0
        OnScoreChanged?.Invoke(Score);
    }
}
