using UnityEngine;

/// <summary>
/// Colores y nombres para mostrar en la interfaz (paleta del CLAUDE.md).
/// </summary>
public static class UIColors
{
    public static Color Paper(PaperColor p)
    {
        switch (p)
        {
            case PaperColor.Red:   return Hex("#C62828");
            case PaperColor.Green: return Hex("#2E7D32");
            case PaperColor.Blue:  return Hex("#1565C0");
            default:               return Color.gray;
        }
    }

    public static Color Bow(BowKind b)
    {
        switch (b)
        {
            case BowKind.Gold:   return Hex("#F9A825");
            case BowKind.Silver: return Hex("#B0BEC5");
            default:             return Color.clear;
        }
    }

    /// <summary>Mismos colores que los materiales de los juguetes.</summary>
    public static Color Toy(ToyKind t)
    {
        switch (t)
        {
            case ToyKind.Bear:  return Hex("#795548");
            case ToyKind.Car:   return Hex("#EF6C00");
            case ToyKind.Robot: return Hex("#78909C");
            case ToyKind.Ball:  return Hex("#AB47BC");
            default:            return Color.gray;
        }
    }

    public static string PaperName(PaperColor p)
    {
        switch (p)
        {
            case PaperColor.Red:   return "rojo";
            case PaperColor.Green: return "verde";
            case PaperColor.Blue:  return "azul";
            default:               return "ninguno";
        }
    }

    public static string BowName(BowKind b)
    {
        switch (b)
        {
            case BowKind.Gold:   return "moño dorado";
            case BowKind.Silver: return "moño plateado";
            default:             return "sin moño";
        }
    }

    public static string ToyName(ToyKind t)
    {
        switch (t)
        {
            case ToyKind.Bear:  return "Osito";
            case ToyKind.Car:   return "Carrito";
            case ToyKind.Robot: return "Robot";
            case ToyKind.Ball:  return "Pelota";
            default:            return "?";
        }
    }

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
