using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla final: puntaje, estrellas y botones Reintentar / Siguiente / Menú.
/// </summary>
public class EndScreenUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public Image[] stars;            // 3 imágenes
    public GameObject nextButton;

    void Start()
    {
        panel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += OnState;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= OnState;
    }

    void OnState(GameState s)
    {
        if (s != GameState.Ended) return;

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        int starCount = GameManager.Instance.GetStars(score);

        titleText.text = starCount > 0 ? "¡Se acabó el tiempo!" : "¡Santa está decepcionado!";
        scoreText.text = $"Puntos: {score}";
        for (int i = 0; i < stars.Length; i++)
            stars[i].color = i < starCount ? UIColors.Hex("#F9A825") : new Color(1f, 1f, 1f, 0.2f);

        nextButton.SetActive(starCount > 0 && GameManager.Instance.HasNextLevel);
        panel.SetActive(true);
    }

    // Botones
    public void Retry() => GameManager.Instance.Restart();
    public void Next() => GameManager.Instance.NextLevel();
    public void Menu() => GameManager.Instance.GoToMenu();
}
