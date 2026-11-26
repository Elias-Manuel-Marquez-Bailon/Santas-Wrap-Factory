using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tarjeta de un pedido: juguete (nombre + color), color de papel, moño (o "Sin moño") y barra de tiempo.
/// </summary>
public class OrderCardUI : MonoBehaviour
{
    public TMP_Text toyText;
    public Image toySwatch;         // color del juguete
    public Image paperSwatch;
    public Image bowSwatch;
    public TMP_Text bowLabel;       // "Moño" o "Sin moño"
    public RectTransform timeFill;  // barra que se encoge (anchorMax.x)
    public Image timeFillImage;

    public Order Order { get; private set; }

    public void Setup(Order order)
    {
        Order = order;
        toyText.text = UIColors.ToyName(order.toy);
        toySwatch.color = UIColors.Toy(order.toy);
        paperSwatch.color = UIColors.Paper(order.paper);

        bool hasBow = order.bow != BowKind.None;
        bowSwatch.gameObject.SetActive(hasBow);
        if (hasBow) bowSwatch.color = UIColors.Bow(order.bow);
        bowLabel.text = hasBow ? (order.bow == BowKind.Gold ? "Moño dorado" : "Moño plateado") : "Sin moño";
    }

    void Update()
    {
        if (Order == null) return;
        float t = Mathf.Clamp01(Order.TimePercent);
        timeFill.anchorMax = new Vector2(t, 1f);
        timeFillImage.color = Color.Lerp(UIColors.Hex("#C62828"), UIColors.Hex("#66BB6A"), t);
    }
}
