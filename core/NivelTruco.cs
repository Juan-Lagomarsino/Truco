namespace Domain;

/// <summary>
/// El nivel de truco querido en una mano y cuánto vale.
/// RULES_Afinadas.md §"El grite de Truco/Retruco/ValeCuatro": la mano vale 1 si no se
/// gritó nada, o 2, 3 o 4 según el último canto querido.
/// </summary>
public enum NivelTruco
{
    Nada = 1,
    Truco = 2,
    Retruco = 3,
    ValeCuatro = 4,
}
