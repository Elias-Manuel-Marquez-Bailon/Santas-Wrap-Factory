using UnityEngine;

/// <summary>
/// Barra de progreso simple en el mundo (dos cubos: fondo y relleno).
/// Siempre mira a la cámara. Se muestra solo mientras la estación trabaja.
/// </summary>
public class ProgressBar : MonoBehaviour
{
    [Tooltip("Cubo de relleno; su pivote se ajusta por código para crecer de izquierda a derecha")]
    public Transform fill;
    public float width = 1f;

    void LateUpdate()
    {
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>Valor entre 0 y 1.</summary>
    public void SetProgress(float t)
    {
        t = Mathf.Clamp01(t);
        Vector3 s = fill.localScale;
        s.x = width * t;
        fill.localScale = s;
        fill.localPosition = new Vector3(-width * 0.5f + s.x * 0.5f, 0f, -0.01f);
    }

    public void Show(bool visible) => gameObject.SetActive(visible);
}
