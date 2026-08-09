namespace Domain;

/// <summary>
/// Una acción que un jugador puede hacer en la partida. Es la única entrada del
/// reductor: Unity, el server y el bot mandan Acciones, y <see cref="Partido.Aplicar"/>
/// las resuelve. Por ahora sólo existe <see cref="TirarCarta"/>; los cantos y el irse
/// al mazo se agregan en los pasos siguientes.
/// </summary>
public abstract record Accion;

/// <summary>Un jugador tira una de sus cartas a la mesa.</summary>
public sealed record TirarCarta(JugadorId Jugador, Carta Carta) : Accion;

/// <summary>
/// Un jugador grita truco (o lo revira al nivel siguiente: retruco, vale cuatro). El
/// nivel concreto lo determina el estado; nunca se revira el propio canto.
/// </summary>
public sealed record CantarTruco(JugadorId Jugador) : Accion;

/// <summary>Un jugador toca el envido (o lo revira: envido de nuevo, real envido, falta envido).</summary>
public sealed record CantarEnvido(JugadorId Jugador, EnvidoCanto Canto) : Accion;

/// <summary>Un jugador canta su flor. Anula el envido; la flor más alta cobra 3.</summary>
public sealed record CantarFlor(JugadorId Jugador) : Accion;

/// <summary>Bid de flor "Con Flor Envido": si el rival con flor quiere, 5 a la flor más alta.</summary>
public sealed record CantarFlorEnvido(JugadorId Jugador) : Accion;

/// <summary>Bid de flor "Contra Flor al Resto": si el rival con flor quiere, la falta + las flores.</summary>
public sealed record CantarContraFlorAlResto(JugadorId Jugador) : Accion;

/// <summary>Aceptar el canto pendiente.</summary>
public sealed record Quiero(JugadorId Jugador) : Accion;

/// <summary>Rechazar el canto pendiente: el que cantó se lleva el valor del último canto querido.</summary>
public sealed record NoQuiero(JugadorId Jugador) : Accion;

/// <summary>
/// Rendirse en la mano: el rival se lleva los puntos en juego (1, o el último truco
/// querido) y la mano termina. En equipo se va todo el equipo del que se va (B7).
/// </summary>
public sealed record IrseAlMazo(JugadorId Jugador) : Accion;
