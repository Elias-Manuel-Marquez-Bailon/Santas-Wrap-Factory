using UnityEngine;

/// <summary>
/// Clase base de todas las estaciones.
/// Cada estación decide qué pasa al interactuar y qué items acepta.
/// </summary>
public abstract class Station : MonoBehaviour
{
    /// <summary>Lo que pasa cuando el jugador presiona E / Espacio frente a la estación.</summary>
    public abstract void Interact(PlayerCarry player);

    /// <summary>¿La estación puede recibir este item ahora?</summary>
    public virtual bool CanAccept(Item item) => false;
}
