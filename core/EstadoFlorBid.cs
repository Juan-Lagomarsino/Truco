namespace Domain;

/// <summary>
/// Un bid de flor esperando respuesta: Con Flor Envido o Contra Flor al Resto.
/// <see cref="Responde"/> es el equipo que tiene que contestar (tiene flor).
/// </summary>
public sealed record EstadoFlorBid(bool EsContraFlorAlResto, EquipoId Responde);
