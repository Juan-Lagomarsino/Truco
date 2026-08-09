namespace Domain;

/// <summary>Una carta puesta en la mesa por un jugador dentro de la baza en curso.</summary>
public readonly record struct Jugada(JugadorId Jugador, Carta Carta);
