using UnityEngine;

/// <summary>
/// Dispensador: si el jugador tiene las manos vacías, le da un item nuevo.
/// Se configura con un prefab y los atributos que tendrá el item creado.
/// </summary>
public class Dispenser : MonoBehaviour, IInteractable
{
    [Tooltip("Prefab del item que entrega (debe tener el componente Item)")]
    public Item itemPrefab;

    [Header("Atributos que se aplican al item creado")]
    public ItemType type = ItemType.Toy;
    public ToyKind toy = ToyKind.None;
    public PaperColor paper = PaperColor.None;
    public BowKind bow = BowKind.None;

    public void Interact(PlayerCarry player)
    {
        if (player.IsHolding || itemPrefab == null) return;

        Item nuevo = Instantiate(itemPrefab);
        nuevo.type = type;
        nuevo.toy = toy;
        nuevo.paper = paper;
        nuevo.bow = bow;
        player.PickUp(nuevo);
    }
}
