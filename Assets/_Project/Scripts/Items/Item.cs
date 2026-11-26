using UnityEngine;

/// <summary>
/// Objeto que el jugador puede recoger y cargar.
/// Guarda su tipo y atributos (juguete, color de papel, moño).
/// El collider debe ser Trigger para que el jugador no choque con items en el piso.
/// </summary>
public class Item : MonoBehaviour
{
    [Header("Datos del item")]
    public ItemType type = ItemType.Toy;
    public ToyKind toy = ToyKind.None;
    public PaperColor paper = PaperColor.None;
    public BowKind bow = BowKind.None;

    /// <summary>True mientras alguien lo sostiene o una estación lo tiene.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Lo pega a un punto (mano del jugador o estación).</summary>
    public void AttachTo(Transform point)
    {
        IsHeld = true;
        transform.SetParent(point);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>Lo deja suelto en el mundo en una posición.</summary>
    public void Release(Vector3 worldPosition)
    {
        IsHeld = false;
        transform.SetParent(null);
        transform.position = worldPosition;
        transform.rotation = Quaternion.identity;
    }
}
