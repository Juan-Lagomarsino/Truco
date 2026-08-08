namespace Domain;

/// <summary>
/// Resultado de repartir una mano: las manos de cada jugador en orden de reparto,
/// la muestra que se dio vuelta después de repartir, y lo que quedó en el mazo.
/// RULES_Afinadas.md §"La Muestra": la muestra sale después del reparto y no la
/// tiene ningún jugador.
/// </summary>
public sealed record Reparto(
    IReadOnlyList<IReadOnlyList<Carta>> Manos,
    Muestra Muestra,
    Mazo Resto);
