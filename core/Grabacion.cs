namespace Domain;

/// <summary>
/// Todo lo necesario para reconstruir una partida completa, paso a paso, desde cero: los
/// parámetros con que se creó (ver <see cref="Partido.Nueva"/>) y la secuencia de acciones
/// que se le aplicaron en orden. El reparto de cada mano ya es determinista por semilla
/// (PREGUNTAS_ABIERTAS.md, D3), así que no hace falta grabar nada del estado intermedio:
/// ver <see cref="Grabador"/> para la reproducción.
///
/// Construirla es responsabilidad de quien juega la partida (el fuzz, el bot, la consola),
/// no de <see cref="Partido"/>: <c>Aplicar</c> sigue devolviendo sólo el estado siguiente,
/// nunca un historial. Basta con acumular en una lista al lado del estado.
/// </summary>
public sealed record Grabacion
{
    /// <summary>A cuántos puntos se jugó el partido. Ver <see cref="Partido.Nueva"/>.</summary>
    public required int Largo { get; init; }

    /// <summary>La semilla base del barajado con la que se creó el partido.</summary>
    public required int Semilla { get; init; }

    /// <summary>El repartidor inicial, o null para usar el default de <see cref="Partido.Nueva"/>.</summary>
    public JugadorId? RepartidorInicial { get; init; }

    /// <summary>Cuántos jugadores había en la mesa.</summary>
    public required int CantidadJugadores { get; init; }

    /// <summary>Las acciones aplicadas, en el orden en que se jugaron.</summary>
    public required IReadOnlyList<Accion> Acciones { get; init; }
}
