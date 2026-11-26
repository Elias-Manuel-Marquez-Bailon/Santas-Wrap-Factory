using UnityEngine;

/// <summary>
/// Mesa de ensamblado: juguete + caja = caja con juguete.
/// - Con un juguete o una caja en la mano: lo coloca en la mesa.
/// - Cuando tiene ambos, trabaja ~2 s (barra de progreso).
/// - Al terminar espera a que el jugador recoja el resultado con las manos vacías.
/// - Con las manos vacías y sin resultado: devuelve el último item colocado.
/// </summary>
public class AssemblyTable : Station
{
    public float workTime = 2f;
    public Item resultPrefab;        // prefab "caja con juguete"
    public Transform slotPoint;      // donde se ven los items sobre la mesa
    public ProgressBar progressBar;

    private Item toySlot, boxSlot, result;
    private float timer;
    private bool working;

    void Start() { progressBar?.Show(false); }

    public override bool CanAccept(Item item)
    {
        if (working || result != null || item == null) return false;
        return (item.type == ItemType.Toy && toySlot == null)
            || (item.type == ItemType.Box && boxSlot == null);
    }

    public override void Interact(PlayerCarry player)
    {
        if (player.IsHolding)
        {
            if (!CanAccept(player.HeldItem)) return;
            Item item = player.TakeItem();
            if (item.type == ItemType.Toy) { toySlot = item; item.AttachTo(slotPoint); item.transform.localPosition = new Vector3(-0.3f, 0, 0); }
            else                           { boxSlot = item; item.AttachTo(slotPoint); item.transform.localPosition = new Vector3(0.3f, 0, 0); }

            if (toySlot != null && boxSlot != null) StartWork();
        }
        else if (result != null)
        {
            player.PickUp(result);
            result = null;
        }
        else if (!working)
        {
            // Devolver algo colocado por error
            if (boxSlot != null) { player.PickUp(boxSlot); boxSlot = null; }
            else if (toySlot != null) { player.PickUp(toySlot); toySlot = null; }
        }
    }

    protected override string GetHint()
    {
        if (result != null) return "<color=#2E7D32>¡Listo!</color>\nE para recoger";
        if (working) return "Ensamblando...";
        if (toySlot == null && boxSlot == null) return "Mesa de ensamblado\nJuguete + Caja";
        return toySlot == null ? "Falta: juguete" : "Falta: caja";
    }

    void StartWork()
    {
        working = true;
        timer = 0f;
        progressBar?.Show(true);
        progressBar?.SetProgress(0f);
    }

    void Update()
    {
        if (!working) return;
        timer += Time.deltaTime;
        progressBar?.SetProgress(timer / workTime);
        if (timer >= workTime) Finish();
    }

    void Finish()
    {
        working = false;
        progressBar?.Show(false);

        // El resultado hereda el tipo de juguete
        result = Instantiate(resultPrefab);
        result.type = ItemType.BoxWithToy;
        result.toy = toySlot.toy;
        result.AttachTo(slotPoint);

        // La "cabeza" que se asoma toma el color del juguete
        Renderer toyRend = toySlot.GetComponent<Renderer>();
        if (toyRend != null && result.transform.childCount > 0)
        {
            Renderer head = result.transform.GetChild(0).GetComponent<Renderer>();
            if (head != null) head.sharedMaterial = toyRend.sharedMaterial;
        }

        Destroy(toySlot.gameObject);
        Destroy(boxSlot.gameObject);
        toySlot = boxSlot = null;
    }
}
