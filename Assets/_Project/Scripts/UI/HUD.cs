using TMPro;
using UnityEngine;

/// <summary>
/// Muestra nivel, tiempo restante y puntaje.
/// </summary>
public class HUD : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text timeText;
    public TMP_Text scoreText;

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            levelText.text = gm.CurrentLevel.levelName;
            int t = Mathf.CeilToInt(gm.TimeLeft);
            timeText.text = $"{t / 60}:{t % 60:00}";
            timeText.color = t <= 15 ? UIColors.Hex("#EF5350") : Color.white; // rojo al final
        }
        if (ScoreManager.Instance != null)
            scoreText.text = $"Puntos: {ScoreManager.Instance.Score}";
    }
}
