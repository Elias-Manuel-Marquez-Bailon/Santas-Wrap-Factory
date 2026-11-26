using UnityEngine;

/// <summary>
/// Costal de Santa: recibe regalos terminados, los valida contra los pedidos
/// activos (OrderManager) y los destruye. Si no coincide con ningún pedido, no cuenta.
/// </summary>
public class DeliveryBag : Station
{
    public override bool CanAccept(Item item) => item != null && item.type == ItemType.GiftReady;

    public override void Interact(PlayerCarry player)
    {
        if (!player.IsHolding || !CanAccept(player.HeldItem)) return;

        Item gift = player.TakeItem();
        bool ok = OrderManager.Instance != null && OrderManager.Instance.TryDeliver(gift);
        Debug.Log(ok
            ? $"[Santa] ¡Pedido correcto! {gift.toy} / {gift.paper} / {gift.bow}"
            : $"[Santa] El regalo no coincide con ningún pedido: {gift.toy} / {gift.paper} / {gift.bow}");
        Destroy(gift.gameObject);
    }
}
