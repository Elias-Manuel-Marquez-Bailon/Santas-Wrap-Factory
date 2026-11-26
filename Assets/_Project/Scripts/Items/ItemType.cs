// Enums que describen qué es un item y sus atributos.

/// <summary>Etapa del item dentro del flujo del regalo.</summary>
public enum ItemType
{
    Toy,         // juguete suelto
    Box,         // caja vacía
    Paper,       // papel de envolver
    Bow,         // moño
    BoxWithToy,  // caja con juguete (sale de la mesa de ensamblado)
    GiftReady    // regalo terminado (sale de la envolvedora)
}

/// <summary>Tipos de juguete.</summary>
public enum ToyKind { None, Bear, Car, Robot, Ball }

/// <summary>Colores de papel.</summary>
public enum PaperColor { None, Red, Green, Blue }

/// <summary>Tipos de moño.</summary>
public enum BowKind { None, Gold, Silver }
