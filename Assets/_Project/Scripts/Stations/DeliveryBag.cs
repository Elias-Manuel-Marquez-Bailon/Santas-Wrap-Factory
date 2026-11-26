using UnityEngine;

/// <summary>
/// Costal de Santa: recibe regalos terminados, los valida contra los pedidos
/// activos (OrderManager) y los destruye. Si no coincide con ningún pedido, no cuenta.
/// Muestra un aviso en pantalla con el resultado.
/// </summary>
public class DeliveryBag : Station
{
    static readonly Color Green = new Color(0.4f, 0.9f, 0.4f);
    static readonly Color Red = new Color(1f, 0.35f, 0.3f);

    public override bool CanAccept(Item item) => item != null && item.type == ItemType.GiftReady;

    protected override string GetHint() => "Costal de Santa\nEntrega aquí";

    public override void Interact(PlayerCarry player)
    {
        if (!player.IsHolding) return;

        if (!CanAccept(player.HeldItem))
        {
            FeedbackUI.Instance?.Show("Solo se entregan regalos envueltos", Red);
            return;
        }

        Item gift = player.TakeItem();
        int points = 0;
        bool ok = OrderManager.Instance != null && OrderManager.Instance.TryDeliver(gift, out points);

        string desc = $"{UIColors.ToyName(gift.toy)} / papel {UIColors.PaperName(gift.paper)} / {UIColors.BowName(gift.bow)}";
        if (ok)
        {
            FeedbackUI.Instance?.Show($"¡Correcto! +{points}", Green);
            Debug.Log($"[Santa] ¡Pedido correcto! {desc} (+{points})");
        }
        else
        {
            FeedbackUI.Instance?.Show($"¡No coincide con ningún pedido!\n<size=60%>{desc}</size>", Red);
            Debug.Log($"[Santa] El regalo no coincide con ningún pedido: {desc}");
        }
        Destroy(gift.gameObject);
    }
}
