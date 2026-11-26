using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera pedidos al azar según el LevelData actual, los hace expirar
/// y valida las entregas del costal.
/// </summary>
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Puntaje")]
    public int pointsPerDelivery = 20;
    [Tooltip("Bonus máximo por entregar rápido (se escala con el tiempo restante)")]
    public int maxSpeedBonus = 10;
    public int expirePenalty = -10;

    [Tooltip("Segundos antes del primer pedido")]
    public float firstOrderDelay = 1f;

    public readonly List<Order> activeOrders = new List<Order>();

    public event Action<Order> OnOrderAdded;
    public event Action<Order> OnOrderRemoved;

    private float spawnTimer;

    void Awake()
    {
        Instance = this;
        spawnTimer = firstOrderDelay;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;
        LevelData level = gm.CurrentLevel;

        // Crear pedidos cada cierto tiempo
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = level.orderInterval;
            if (activeOrders.Count < level.maxActiveOrders) SpawnOrder(level);
        }

        // Restar tiempo y expirar (recorrer al revés para poder quitar)
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            Order o = activeOrders[i];
            o.timeLeft -= Time.deltaTime;
            if (o.timeLeft <= 0f)
            {
                ScoreManager.Instance?.Add(expirePenalty);
                FeedbackUI.Instance?.Show($"¡Pedido expirado! {expirePenalty}", new Color(1f, 0.35f, 0.3f));
                RemoveOrder(o);
            }
        }
    }

    void SpawnOrder(LevelData level)
    {
        ToyKind toy = level.allowedToys[UnityEngine.Random.Range(0, level.allowedToys.Length)];
        PaperColor paper = level.allowedPapers[UnityEngine.Random.Range(0, level.allowedPapers.Length)];
        BowKind bow = level.useBow
            ? (UnityEngine.Random.value < 0.5f ? BowKind.Gold : BowKind.Silver)
            : BowKind.None;

        Order o = new Order(toy, paper, bow, level.orderTimeLimit);
        activeOrders.Add(o);
        OnOrderAdded?.Invoke(o);
    }

    void RemoveOrder(Order o)
    {
        activeOrders.Remove(o);
        OnOrderRemoved?.Invoke(o);
    }

    /// <summary>
    /// Intenta entregar un regalo. Si coincide con un pedido activo (el más urgente),
    /// suma puntos y lo quita. Devuelve true si fue correcto y los puntos ganados en "points".
    /// </summary>
    public bool TryDeliver(Item gift, out int points)
    {
        points = 0;
        Order best = null;
        foreach (Order o in activeOrders)
            if (o.Matches(gift) && (best == null || o.timeLeft < best.timeLeft)) best = o;

        if (best == null) return false;

        points = pointsPerDelivery + Mathf.RoundToInt(maxSpeedBonus * best.TimePercent);
        ScoreManager.Instance?.Add(points);
        RemoveOrder(best);
        return true;
    }
}
