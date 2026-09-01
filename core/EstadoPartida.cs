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

    /// <summary>Cuántos jugadores hay en la mesa: 2 (1v1), 4 (2v2) o 6 (a 6).</summary>
    public required int CantidadJugadores { get; init; }

    /// <summary>El jugador que repartió esta mano. El mano es el siguiente.</summary>
    public required JugadorId Repartidor { get; init; }

    /// <summary>La carta que define el palo de las piezas en esta mano.</summary>
    public required Muestra Muestra { get; init; }

    /// <summary>Cartas que le quedan a cada jugador, indexadas por <see cref="JugadorId.Valor"/>.</summary>
    public required IReadOnlyList<IReadOnlyList<Carta>> Manos { get; init; }

    /// <summary>Las tres cartas con que arrancó la mano cada jugador. El envido y la flor se cuentan sobre éstas, no sobre lo que queda.</summary>
    public IReadOnlyList<IReadOnlyList<Carta>> ManosIniciales { get; init; } = Array.Empty<IReadOnlyList<Carta>>();

    /// <summary>
    /// Los jugadores que participan de esta mano, en orden de asiento. Vacío = todos
    /// (redondilla, 1v1, 2v2, 3v3). En un pico a pico del modo de a 6 sólo juega la pareja.
    /// </summary>
    public IReadOnlyList<JugadorId> Activos { get; init; } = Array.Empty<JugadorId>();

    /// <summary>
    /// La fase del ciclo del modo de a 6 (redondilla ↔ pico a pico). En el resto de los
    /// modos es siempre Redondilla. Ver <see cref="FaseCiclo"/>.
    /// </summary>
    public FaseCiclo Fase { get; init; } = FaseCiclo.Redondilla;

    /// <summary>
    /// Cuál de los tres picos se está jugando (0..2). Sólo tiene sentido cuando
    /// <see cref="Fase"/> es PicoAPico; en redondilla es 0.
    /// </summary>
    public int IndicePico { get; init; }

    /// <summary>Resultado de las bazas ya cerradas en esta mano.</summary>
    public required IReadOnlyList<GanadorBaza> BazasGanadas { get; init; }

    /// <summary>Cartas jugadas en la baza en curso, en orden de juego.</summary>
    public required IReadOnlyList<Jugada> JugadasBaza { get; init; }

    /// <summary>Quién abrió (tiró primero) la baza en curso.</summary>
    public required JugadorId Abridor { get; init; }

    /// <summary>De quién es el turno de jugar.</summary>
    public required JugadorId Turno { get; init; }

    /// <summary>El nivel de truco querido en esta mano. Vale 1 mientras sea Nada.</summary>
    public NivelTruco Truco { get; init; } = NivelTruco.Nada;

    /// <summary>El nivel que se acaba de cantar y espera respuesta, o null si no hay canto pendiente.</summary>
    public NivelTruco? TrucoPendiente { get; init; }

    /// <summary>Cuando hay un canto pendiente, el equipo que tiene que responder.</summary>
    public EquipoId? EquipoResponde { get; init; }

    /// <summary>El equipo que puede revirar el truco (el que quiso el último canto). Null en Nada: puede empezar cualquiera en su turno.</summary>
    public EquipoId? EquipoQuePuedeRevirar { get; init; }

    /// <summary>El envido cantado que espera respuesta, o null si no hay ninguno.</summary>
    public EstadoEnvido? EnvidoPendiente { get; init; }

    /// <summary>El envido ya se jugó (se quiso o no se quiso) en esta mano: no se puede tocar de nuevo.</summary>
    public bool EnvidoJugado { get; init; }

    /// <summary>Ya se cantó y resolvió una flor en esta mano. Anula el envido y bloquea otra flor.</summary>
    public bool FlorResuelta { get; init; }

    /// <summary>El cobro de flor resuelto, pendiente de acreditar al cerrar la mano (F4).</summary>
    public Cobro? CobroFlor { get; init; }

    /// <summary>El cobro de envido resuelto, pendiente de acreditar al cerrar la mano (F4).</summary>
    public Cobro? CobroEnvido { get; init; }

    /// <summary>Un bid de flor (Con Flor Envido / Contra Flor al Resto) esperando respuesta.</summary>
    public EstadoFlorBid? FlorPendiente { get; init; }

    /// <summary>La mano está en la ventana de cierre (denuncias de flor escondida) antes de acreditar.</summary>
    public CierrePendiente? Cierre { get; init; }

    /// <summary>En la ventana de cierre, los jugadores que todavía pueden denunciar o pasar.</summary>
    public IReadOnlyList<JugadorId> DenunciasPendientes { get; init; } = Array.Empty<JugadorId>();

    /// <summary>El partido terminó cuando un equipo llegó al largo.</summary>
    public bool Terminado => Contador.Termino;

    /// <summary>Hay un truco esperando quiero / no quiero.</summary>
    public bool HayCantoPendiente => TrucoPendiente is not null;

    /// <summary>Hay un envido esperando respuesta.</summary>
    public bool HayEnvidoPendiente => EnvidoPendiente is not null;

    /// <summary>Hay un bid de flor esperando respuesta.</summary>
    public bool HayFlorPendiente => FlorPendiente is not null;

    /// <summary>
    /// El jugador mano de esta ronda: el que está a la derecha del repartidor. En un pico a
    /// pico los tres picos tienen de mano a los tres jugadores consecutivos al repartidor, así
    /// que el mano del pico k es repartidor+1+k.
    /// </summary>
    public JugadorId JugadorMano => new(
        (Repartidor.Valor + 1 + (Fase == FaseCiclo.PicoAPico ? IndicePico : 0)) % CantidadJugadores);

    /// <summary>El equipo de un jugador. Los equipos se sientan intercalados, así que es su índice módulo 2.</summary>
    public EquipoId EquipoDe(JugadorId jugador) => new(jugador.Valor % 2);
}
