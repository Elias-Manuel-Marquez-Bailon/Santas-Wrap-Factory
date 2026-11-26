using UnityEngine;

/// <summary>
/// Datos de un nivel. Todos los niveles usan el mismo mapa; solo cambian estos valores.
/// Crear nuevos desde: Create > Santa > Level Data.
/// </summary>
[CreateAssetMenu(fileName = "Level_", menuName = "Santa/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("General")]
    public string levelName = "Nivel 1";
    [Tooltip("Duración del nivel en segundos")]
    public float levelDuration = 150f;

    [Header("Pedidos")]
    [Tooltip("Segundos entre cada pedido nuevo")]
    public float orderInterval = 20f;
    [Tooltip("Segundos que tiene el jugador para completar cada pedido")]
    public float orderTimeLimit = 60f;
    public int maxActiveOrders = 4;
    [Tooltip("Si es falso, los pedidos no llevan moño")]
    public bool useBow = false;
    public ToyKind[] allowedToys = { ToyKind.Bear, ToyKind.Car };
    public PaperColor[] allowedPapers = { PaperColor.Red, PaperColor.Green, PaperColor.Blue };

    [Header("Estrellas (puntos mínimos)")]
    public int oneStar = 40;
    public int twoStars = 80;
    public int threeStars = 120;
}
