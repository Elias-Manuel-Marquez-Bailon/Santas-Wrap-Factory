using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta la estación o item más cercano frente al jugador.
/// E / Espacio: usar estación, recoger item o soltar lo que carga.
/// Resalta lo interactuable con un marcador amarillo encima.
/// </summary>
[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    public float detectDistance = 0.9f; // qué tan adelante se centra la detección
    public float detectRadius = 0.8f;

    private PlayerCarry carry;
    private Transform highlight;      // marcador visual
    private Component currentTarget;  // Station o Item

    void Awake()
    {
        carry = GetComponent<PlayerCarry>();
        CreateHighlight();
    }

    void Update()
    {
        currentTarget = FindTarget();
        UpdateHighlight();

        Keyboard kb = Keyboard.current;
        if (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            DoInteract();
    }

    private void DoInteract()
    {
        if (currentTarget is Station station)
            station.Interact(carry);           // la estación decide qué hacer
        else if (currentTarget is Item item)
            carry.PickUp(item);                // recoger item del piso
        else if (carry.IsHolding)
            carry.Drop();                      // nada enfrente: soltar
    }

    /// <summary>Busca lo más cercano frente al jugador. Las estaciones tienen prioridad.</summary>
    private Component FindTarget()
    {
        Vector3 center = transform.position + transform.forward * detectDistance;
        Collider[] hits = Physics.OverlapSphere(center, detectRadius, ~0, QueryTriggerInteraction.Collide);

        Component best = null;
        float bestDist = float.MaxValue;
        bool bestIsStation = false;

        foreach (Collider c in hits)
        {
            // Estación
            Station station = c.GetComponentInParent<Station>();
            if (station != null)
            {
                float d = Vector3.Distance(center, c.ClosestPoint(center));
                if (!bestIsStation || d < bestDist) { best = station; bestDist = d; bestIsStation = true; }
                continue;
            }

            // Item suelto (solo si tenemos manos vacías y no hay estación)
            if (bestIsStation || carry.IsHolding) continue;
            Item item = c.GetComponentInParent<Item>();
            if (item != null && !item.IsHeld)
            {
                float d = Vector3.Distance(center, item.transform.position);
                if (d < bestDist) { best = item; bestDist = d; }
            }
        }
        return best;
    }

    private void CreateHighlight()
    {
        GameObject h = GameObject.CreatePrimitive(PrimitiveType.Cube);
        h.name = "InteractHighlight";
        Destroy(h.GetComponent<Collider>());
        h.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);
        var r = h.GetComponent<Renderer>();
        r.material.color = Color.yellow;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        highlight = h.transform;
        highlight.gameObject.SetActive(false);
    }

    private void UpdateHighlight()
    {
        if (currentTarget == null) { highlight.gameObject.SetActive(false); return; }

        // Colocar el marcador arriba del objeto
        Bounds b = new Bounds(currentTarget.transform.position, Vector3.zero);
        foreach (Renderer r in currentTarget.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        highlight.position = new Vector3(b.center.x, b.max.y + 0.3f, b.center.z);
        highlight.gameObject.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * detectDistance, detectRadius);
    }
}
