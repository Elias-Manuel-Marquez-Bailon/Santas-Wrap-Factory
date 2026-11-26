using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Muestra una tarjeta por cada pedido activo (arriba de la pantalla).
/// </summary>
public class OrderListUI : MonoBehaviour
{
    public OrderCardUI cardPrefab;
    public Transform container; // tiene un HorizontalLayoutGroup

    private readonly Dictionary<Order, OrderCardUI> cards = new Dictionary<Order, OrderCardUI>();

    void Start()
    {
        if (OrderManager.Instance == null) return;
        OrderManager.Instance.OnOrderAdded += AddCard;
        OrderManager.Instance.OnOrderRemoved += RemoveCard;
    }

    void OnDestroy()
    {
        if (OrderManager.Instance == null) return;
        OrderManager.Instance.OnOrderAdded -= AddCard;
        OrderManager.Instance.OnOrderRemoved -= RemoveCard;
    }

    void AddCard(Order order)
    {
        OrderCardUI card = Instantiate(cardPrefab, container);
        card.Setup(order);
        cards[order] = card;
    }

    void RemoveCard(Order order)
    {
        if (cards.TryGetValue(order, out OrderCardUI card))
        {
            Destroy(card.gameObject);
            cards.Remove(order);
        }
    }
}
