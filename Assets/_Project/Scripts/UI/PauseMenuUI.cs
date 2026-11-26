using UnityEngine;

/// <summary>
/// Panel de pausa (Esc): Continuar / Menú.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        panel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += OnState;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= OnState;
    }

    void OnState(GameState s) => panel.SetActive(s == GameState.Paused);

    // Botones
    public void Resume() => GameManager.Instance.TogglePause();
    public void Menu() => GameManager.Instance.GoToMenu();
}
