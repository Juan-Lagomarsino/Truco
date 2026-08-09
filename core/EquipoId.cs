namespace Domain;

/// <summary>
/// Identidad de un equipo dentro de un partido. En 1v1 cada jugador es su propio
/// equipo. Es sólo un identificador estable; la composición de cada equipo (qué
/// jugadores lo forman y cómo se sientan) la maneja el reductor.
/// </summary>
public readonly record struct EquipoId
{
    public int Valor { get; }

    public EquipoId(int valor)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "El id de equipo no puede ser negativo.");
        Valor = valor;
    }
}
