/// <summary>
/// Un pedido activo: qué regalo se pide y cuánto tiempo le queda.
/// </summary>
public class Order
{
    public ToyKind toy;
    public PaperColor paper;
    public BowKind bow;
    public float timeLimit;
    public float timeLeft;

    public Order(ToyKind toy, PaperColor paper, BowKind bow, float timeLimit)
    {
        this.toy = toy;
        this.paper = paper;
        this.bow = bow;
        this.timeLimit = timeLimit;
        timeLeft = timeLimit;
    }

    /// <summary>Fracción de tiempo restante (1 = recién creado, 0 = expirado).</summary>
    public float TimePercent => timeLimit > 0f ? timeLeft / timeLimit : 0f;

    /// <summary>¿Este regalo cumple el pedido?</summary>
    public bool Matches(Item item)
    {
        return item != null
            && item.type == ItemType.GiftReady
            && item.toy == toy
            && item.paper == paper
            && item.bow == bow;
    }
}
