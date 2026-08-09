namespace Domain;

/// <summary>
/// Identidad de un jugador (una silla en la mesa), de 0 a cantidadJugadores-1, en
/// sentido antihorario. En 1v1 son el 0 y el 1. El equipo de un jugador es su índice
/// módulo 2, porque los equipos se sientan intercalados.
/// </summary>
public readonly record struct JugadorId
{
    public int Valor { get; }

    public JugadorId(int valor)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "El id de jugador no puede ser negativo.");
        Valor = valor;
    }
}
