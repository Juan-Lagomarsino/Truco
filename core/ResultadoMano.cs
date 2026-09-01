namespace Domain;

/// <summary>
/// El resultado de resolver una mano: o ya está definida y la ganó un equipo, o
/// todavía está indefinida (falta jugar alguna baza). El reductor usa
/// <see cref="EstaDefinida"/> para saber si tiene que seguir jugando bazas.
/// </summary>
public readonly record struct ResultadoMano
{
    // Valor del equipo ganador, o -1 si la mano todavía no está definida.
    private readonly int _ganador;

    private ResultadoMano(int ganador) => _ganador = ganador;

    /// <summary>La mano todavía no está definida: falta jugar al menos una baza.</summary>
    public static readonly ResultadoMano Indefinida = new(-1);

    /// <summary>La mano la ganó <paramref name="equipo"/>.</summary>
    public static ResultadoMano Gana(EquipoId equipo) => new(equipo.Valor);

    /// <summary>True si la mano ya tiene un equipo ganador (no hace falta jugar más bazas).</summary>
    public bool EstaDefinida => _ganador >= 0;

    /// <summary>El equipo que ganó la mano. Lanza si todavía está indefinida.</summary>
    public EquipoId Ganador => EstaDefinida
        ? new EquipoId(_ganador)
        : throw new InvalidOperationException("La mano todavía no está definida.");
}
