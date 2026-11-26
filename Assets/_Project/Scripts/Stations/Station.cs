using TMPro;
using UnityEngine;

/// <summary>
/// Clase base de todas las estaciones.
/// Cada estación decide qué pasa al interactuar y qué items acepta.
/// Opcional: un texto de ayuda flotante (hintText) que dice qué falta o qué hacer.
/// </summary>
public abstract class Station : MonoBehaviour
{
    [Tooltip("Texto 3D flotante encima de la estación (opcional)")]
    public TMP_Text hintText;

    /// <summary>Lo que pasa cuando el jugador presiona E / Espacio frente a la estación.</summary>
    public abstract void Interact(PlayerCarry player);

    /// <summary>¿La estación puede recibir este item ahora?</summary>
    public virtual bool CanAccept(Item item) => false;

    /// <summary>Texto de ayuda según el estado actual. Vacío = no mostrar.</summary>
    protected virtual string GetHint() => "";

    protected virtual void LateUpdate()
    {
        if (hintText == null) return;
        hintText.text = GetHint();
        if (Camera.main != null) hintText.transform.rotation = Camera.main.transform.rotation; // mirar a la cámara
    }
}
