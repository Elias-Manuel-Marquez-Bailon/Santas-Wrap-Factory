using UnityEngine;

/// <summary>
/// Punto de mano del jugador. Sostiene UN solo item a la vez.
/// </summary>
public class PlayerCarry : MonoBehaviour
{
    [Tooltip("Transform donde se coloca el item sostenido")]
    public Transform holdPoint;

    [Tooltip("Qué tan adelante del jugador se suelta el item")]
    public float dropDistance = 1f;

    public Item HeldItem { get; private set; }
    public bool IsHolding => HeldItem != null;

    /// <summary>Recoge un item si las manos están vacías.</summary>
    public bool PickUp(Item item)
    {
        if (IsHolding || item == null) return false;
        HeldItem = item;
        item.AttachTo(holdPoint);
        return true;
    }

    /// <summary>Quita el item de la mano sin soltarlo al piso (para dárselo a una estación).</summary>
    public Item TakeItem()
    {
        Item item = HeldItem;
        HeldItem = null;
        return item;
    }

    /// <summary>Suelta el item al piso frente al jugador.</summary>
    public void Drop()
    {
        if (!IsHolding) return;
        Vector3 pos = transform.position + transform.forward * dropDistance;
        pos.y = 0.25f; // a ras del piso
        TakeItem().Release(pos);
    }
}
