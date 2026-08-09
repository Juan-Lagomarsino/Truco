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
