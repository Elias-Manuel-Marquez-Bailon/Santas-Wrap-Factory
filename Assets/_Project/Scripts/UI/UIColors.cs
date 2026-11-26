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
