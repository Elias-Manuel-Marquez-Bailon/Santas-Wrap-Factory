using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Paused, Ended }

/// <summary>
/// Estados del juego, cronómetro del nivel y cambio de escenas.
/// Esc = pausa.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>Nivel elegido (se conserva al cambiar de escena).</summary>
    public static int SelectedLevel = 0;

    public LevelData[] levels;
    public string menuSceneName = "MainMenu";

    public LevelData CurrentLevel { get; private set; }
    public GameState State { get; private set; }
    public float TimeLeft { get; private set; }

    public bool IsPlaying => State == GameState.Playing;
    public bool HasNextLevel => SelectedLevel < levels.Length - 1;

    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        SelectedLevel = Mathf.Clamp(SelectedLevel, 0, levels.Length - 1);
        CurrentLevel = levels[SelectedLevel];
        TimeLeft = CurrentLevel.levelDuration;
        State = GameState.Playing;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) TogglePause();

        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            SetState(GameState.Ended);
        }
    }

    void SetState(GameState s)
    {
        State = s;
        Time.timeScale = s == GameState.Paused ? 0f : 1f;
        OnStateChanged?.Invoke(s);
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }

    /// <summary>Cuántas estrellas da un puntaje en el nivel actual (0 a 3).</summary>
    public int GetStars(int score)
    {
        if (score >= CurrentLevel.threeStars) return 3;
        if (score >= CurrentLevel.twoStars) return 2;
        if (score >= CurrentLevel.oneStar) return 1;
        return 0;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        if (HasNextLevel) SelectedLevel++;
        Restart();
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
