using TMPro;
using UnityEngine;

/// <summary>
/// Mensaje grande y temporal en pantalla ("¡Correcto! +25", "¡No coincide!", etc.).
/// Uso: FeedbackUI.Instance?.Show("texto", color);
/// </summary>
public class FeedbackUI : MonoBehaviour
{
    public static FeedbackUI Instance { get; private set; }

    public TMP_Text text;
    public float duration = 1.6f;

    private float timer;

    void Awake()
    {
        Instance = this;
        text.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string message, Color color)
    {
        text.text = message;
        text.color = color;
        text.gameObject.SetActive(true);
        timer = duration;
    }

    void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.unscaledDeltaTime;

        float t = timer / duration;                 // 1 → 0
        Color c = text.color;
        c.a = Mathf.Clamp01(t * 2f);                // se desvanece en la segunda mitad
        text.color = c;
        float pop = 1f + Mathf.Max(0f, t - 0.85f) * 2f; // pequeño "salto" al aparecer
        text.transform.localScale = Vector3.one * pop;

        if (timer <= 0f) text.gameObject.SetActive(false);
    }
}
