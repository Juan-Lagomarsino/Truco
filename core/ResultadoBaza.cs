namespace Domain;

/// <summary>
/// El resultado de resolver una baza: o gana una jugada concreta, o es parda.
/// La parda se modela explícitamente (no con null): <see cref="EsParda"/> lo dice,
/// y pedir el <see cref="Ganador"/> de una parda es un error.
/// </summary>
public readonly record struct ResultadoBaza
{
    // Índice de la jugada ganadora, o -1 si es parda.
    private readonly int _ganador;

    private ResultadoBaza(int ganador) => _ganador = ganador;

    /// <summary>La baza quedó empardada: el máximo de fuerza lo comparten dos o más cartas.</summary>
    public static readonly ResultadoBaza Parda = new(-1);

    /// <summary>Ganó la jugada en la posición <paramref name="indiceJugada"/> de la baza.</summary>
    public static ResultadoBaza Gana(int indiceJugada)
    {
        if (indiceJugada < 0)
            throw new ArgumentOutOfRangeException(nameof(indiceJugada), indiceJugada, "El índice no puede ser negativo.");
        return new ResultadoBaza(indiceJugada);
    }

    /// <summary>True si la baza quedó parda (el máximo de fuerza lo comparten equipos distintos).</summary>
    public bool EsParda => _ganador < 0;

    /// <summary>Índice de la jugada ganadora. Lanza si la baza es parda.</summary>
    public int Ganador => EsParda
        ? throw new InvalidOperationException("Una parda no tiene ganador.")
        : _ganador;
}
