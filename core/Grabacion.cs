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
    public required int Largo { get; init; }
    public required int Semilla { get; init; }
    public JugadorId? RepartidorInicial { get; init; }
    public required int CantidadJugadores { get; init; }
    public required IReadOnlyList<Accion> Acciones { get; init; }
}
