namespace Domain;

/// <summary>
/// Un tanto ya resuelto pero pendiente de acreditar. La flor y el envido se resuelven en
/// el momento (se sabe quién ganó y cuánto), pero se acreditan al cerrar la mano en el
/// orden flor → envido → truco (F4, B6).
/// </summary>
public readonly record struct Cobro(EquipoId Equipo, int Puntos);
