namespace Domain;

/// <summary>
/// A quién le quedó una baza ya resuelta, en términos de equipo: la ganó un equipo,
/// o fue parda. Es la entrada de <see cref="Mano.Resolver"/>. El reductor traduce el
/// índice que devuelve <see cref="Baza.Resolver"/> al equipo del jugador que lo tiró.
/// La parda se modela explícita, sin null.
/// </summary>
public readonly record struct GanadorBaza
{
    // Valor del equipo ganador, o -1 si fue parda.
    private readonly int _equipo;

    private GanadorBaza(int equipo) => _equipo = equipo;

    /// <summary>La baza quedó parda.</summary>
    public static readonly GanadorBaza Parda = new(-1);

    /// <summary>La baza la ganó <paramref name="equipo"/>.</summary>
    public static GanadorBaza De(EquipoId equipo) => new(equipo.Valor);

    /// <summary>True si la baza quedó parda (nadie se la llevó).</summary>
    public bool EsParda => _equipo < 0;

    /// <summary>El equipo que ganó la baza. Lanza si fue parda.</summary>
    public EquipoId Equipo => EsParda
        ? throw new InvalidOperationException("Una parda no tiene ganador.")
        : new EquipoId(_equipo);
}
