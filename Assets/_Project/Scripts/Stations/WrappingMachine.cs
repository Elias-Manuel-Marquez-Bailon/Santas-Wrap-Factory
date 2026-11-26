using UnityEngine;

/// <summary>
/// Máquina envolvedora: caja con juguete + papel (+ moño opcional) = regalo terminado.
/// - Coloca caja con juguete, papel y moño en cualquier orden.
/// - Con los 3: empieza sola (~4 s).
/// - Con caja + papel sin moño: presiona E con manos vacías para envolver sin moño (nivel 1).
/// - Al terminar, recoge el regalo con las manos vacías.
/// </summary>
public class WrappingMachine : Station
{
    public float workTime = 4f;
    public Item giftPrefab;          // prefab de regalo (raíz = caja, hijo "Bow" = moño)
    public Transform slotPoint;
    public ProgressBar progressBar;

    [Header("Materiales del regalo (orden: Red, Green, Blue / Gold, Silver)")]
    public Material[] paperMaterials = new Material[3];
    public Material[] bowMaterials = new Material[2];

    private Item boxSlot, paperSlot, bowSlot, result;
    private float timer;
    private bool working;

    void Start() { progressBar?.Show(false); }

    public override bool CanAccept(Item item)
    {
        if (working || result != null || item == null) return false;
        switch (item.type)
        {
            case ItemType.BoxWithToy: return boxSlot == null;
            case ItemType.Paper:      return paperSlot == null;
            case ItemType.Bow:        return bowSlot == null;
            default:                  return false;
        }
    }

    public override void Interact(PlayerCarry player)
    {
        if (player.IsHolding)
        {
            if (!CanAccept(player.HeldItem)) return;
            Item item = player.TakeItem();
            item.AttachTo(slotPoint);
            switch (item.type)
            {
                case ItemType.BoxWithToy: boxSlot = item;   item.transform.localPosition = Vector3.zero; break;
                case ItemType.Paper:      paperSlot = item; item.transform.localPosition = new Vector3(-0.4f, -0.2f, 0); break;
                case ItemType.Bow:        bowSlot = item;   item.transform.localPosition = new Vector3(0.4f, 0, 0); break;
            }
            if (boxSlot != null && paperSlot != null && bowSlot != null) StartWork();
        }
        else if (result != null)
        {
            player.PickUp(result);
            result = null;
        }
        else if (!working)
        {
            // Manos vacías: envolver sin moño si hay caja + papel; si no, devolver un item
            if (boxSlot != null && paperSlot != null) StartWork();
            else if (bowSlot != null)   { player.PickUp(bowSlot); bowSlot = null; }
            else if (paperSlot != null) { player.PickUp(paperSlot); paperSlot = null; }
            else if (boxSlot != null)   { player.PickUp(boxSlot); boxSlot = null; }
        }
    }

    protected override string GetHint()
    {
        if (result != null) return "<color=#2E7D32>¡Listo!</color>\nE para recoger";
        if (working) return "Envolviendo...";
        if (boxSlot == null && paperSlot == null && bowSlot == null)
            return "Envolvedora\nCaja con juguete + Papel (+ Moño)";

        if (boxSlot != null && paperSlot != null) // solo falta decidir el moño
            return "Agrega moño, o\n<color=#C62828>E con manos vacías = sin moño</color>";

        string falta = "";
        if (boxSlot == null) falta += "caja con juguete";
        if (paperSlot == null) falta += (falta.Length > 0 ? ", " : "") + "papel";
        return "Falta: " + falta;
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

        result = Instantiate(giftPrefab);
        result.type = ItemType.GiftReady;
        result.toy = boxSlot.toy;
        result.paper = paperSlot.paper;
        result.bow = bowSlot != null ? bowSlot.bow : BowKind.None;
        ApplyLook(result);
        result.AttachTo(slotPoint);

        Destroy(boxSlot.gameObject);
        Destroy(paperSlot.gameObject);
        if (bowSlot != null) Destroy(bowSlot.gameObject);
        boxSlot = paperSlot = bowSlot = null;
    }

    /// <summary>Pinta el regalo con el color de papel y el moño elegido.</summary>
    void ApplyLook(Item gift)
    {
        int p = (int)gift.paper - 1;
        if (p >= 0 && p < paperMaterials.Length && paperMaterials[p] != null)
            gift.GetComponent<Renderer>().sharedMaterial = paperMaterials[p];

        Transform bowT = gift.transform.Find("Bow");
        if (bowT == null) return;
        int b = (int)gift.bow - 1;
        bowT.gameObject.SetActive(b >= 0);
        if (b >= 0 && b < bowMaterials.Length && bowMaterials[b] != null)
            foreach (Renderer r in bowT.GetComponentsInChildren<Renderer>()) r.sharedMaterial = bowMaterials[b];
    }
}
