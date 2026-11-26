using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tarjeta de un pedido: nombre del juguete, color de papel, moño y barra de tiempo.
/// </summary>
public class OrderCardUI : MonoBehaviour
{
    public TMP_Text toyText;
    public Image paperSwatch;
    public Image bowSwatch;
    public GameObject bowGroup;     // se oculta si el pedido no lleva moño
    public RectTransform timeFill;  // barra que se encoge (anchorMax.x)
    public Image timeFillImage;

    public Order Order { get; private set; }

    public void Setup(Order order)
    {
        Order = order;
        toyText.text = UIColors.ToyName(order.toy);
        paperSwatch.color = UIColors.Paper(order.paper);

        bool hasBow = order.bow != BowKind.None;
        bowGroup.SetActive(hasBow);
        if (hasBow) bowSwatch.color = UIColors.Bow(order.bow);
    }

    void Update()
    {
        if (Order == null) return;
        float t = Mathf.Clamp01(Order.TimePercent);
        timeFill.anchorMax = new Vector2(t, 1f);
        timeFillImage.color = Color.Lerp(UIColors.Hex("#C62828"), UIColors.Hex("#66BB6A"), t);
    }
}
