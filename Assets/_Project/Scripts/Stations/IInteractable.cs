/// <summary>
/// Cualquier cosa con la que el jugador puede interactuar con E / Espacio
/// (estaciones: dispensador, mesa, envolvedora, costal).
/// </summary>
public interface IInteractable
{
    void Interact(PlayerCarry player);
}
