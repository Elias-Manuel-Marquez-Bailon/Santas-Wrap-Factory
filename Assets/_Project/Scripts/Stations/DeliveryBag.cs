using UnityEngine;

/// <summary>
/// Costal de Santa: recibe regalos terminados y los destruye.
/// En la Fase 2 validará contra el OrderManager y sumará puntos.
/// </summary>
public class DeliveryBag : Station
{
    public override bool CanAccept(Item item) => item != null && item.type == ItemType.GiftReady;

    public override void Interact(PlayerCarry player)
    {
        if (!player.IsHolding || !CanAccept(player.HeldItem)) return;

        Item gift = player.TakeItem();
        // TODO Fase 2: OrderManager.TryDeliver(gift) y sumar/restar puntos
        Debug.Log($"[Santa] Regalo entregado: {gift.toy} / papel {gift.paper} / moño {gift.bow}");
        Destroy(gift.gameObject);
    }
}
