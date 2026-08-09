namespace Domain;

/// <summary>
/// La mano ya se decidió y está en la ventana de cierre, esperando denuncias de flor
/// escondida antes de acreditar y repartir. Guarda el resultado del truco (a quién y
/// cuánto) para aplicarlo cuando se cierre la ventana.
/// </summary>
public sealed record CierrePendiente(EquipoId GanadorTruco, int PuntosTruco);
