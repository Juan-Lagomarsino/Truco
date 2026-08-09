namespace Domain;

/// <summary>
/// El estado completo e inmutable de una partida. Todo lo que hace falta para dibujarla,
/// serializarla y seguir jugándola está acá; los cambios se expresan con <c>with</c>.
///
/// Alcance actual: 1v1 y sólo cartas (sin cantos). La estructura ya contempla varios
/// jugadores (equipos intercalados por <see cref="EquipoDe"/>), pero el 2v2 se cierra
/// en el Paso 16.
/// </summary>
public sealed record EstadoPartida
{
    /// <summary>El puntaje del partido.</summary>
    public required Contador Contador { get; init; }

    /// <summary>Semilla base del barajado. Cada mano reparte desde una semilla derivada de ésta.</summary>
    public required int Semilla { get; init; }

    /// <summary>Número de mano jugada, desde 0. Junto con la semilla determina el reparto.</summary>
    public required int NumeroDeMano { get; init; }

    public required int CantidadJugadores { get; init; }

    /// <summary>El jugador que repartió esta mano. El mano es el siguiente.</summary>
    public required JugadorId Repartidor { get; init; }

    public required Muestra Muestra { get; init; }

    /// <summary>Cartas que le quedan a cada jugador, indexadas por <see cref="JugadorId.Valor"/>.</summary>
    public required IReadOnlyList<IReadOnlyList<Carta>> Manos { get; init; }

    /// <summary>Resultado de las bazas ya cerradas en esta mano.</summary>
    public required IReadOnlyList<GanadorBaza> BazasGanadas { get; init; }

    /// <summary>Cartas jugadas en la baza en curso, en orden de juego.</summary>
    public required IReadOnlyList<Jugada> JugadasBaza { get; init; }

    /// <summary>Quién abrió (tiró primero) la baza en curso.</summary>
    public required JugadorId Abridor { get; init; }

    /// <summary>De quién es el turno de jugar.</summary>
    public required JugadorId Turno { get; init; }

    /// <summary>El partido terminó cuando un equipo llegó al largo.</summary>
    public bool Terminado => Contador.Termino;

    /// <summary>El jugador mano de esta ronda: el que está seguido al repartidor.</summary>
    public JugadorId JugadorMano => new((Repartidor.Valor + 1) % CantidadJugadores);

    /// <summary>El equipo de un jugador. Los equipos se sientan intercalados, así que es su índice módulo 2.</summary>
    public EquipoId EquipoDe(JugadorId jugador) => new(jugador.Valor % 2);
}
