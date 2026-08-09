namespace Domain;

/// <summary>
/// El estado de un envido cantado que espera respuesta. <see cref="ValorSiQuiero"/> es
/// lo que se lleva el ganador si se quiere; <see cref="ValorSiNoQuiero"/> es lo que se
/// lleva el que cantó si el rival no quiere (el valor del último canto ya querido, o 1).
/// <see cref="Responde"/> es el equipo al que le toca contestar.
/// </summary>
public sealed record EstadoEnvido(
    EnvidoCanto Ultimo,
    int ValorSiQuiero,
    int ValorSiNoQuiero,
    EquipoId Responde);
