namespace Domain;

/// <summary>
/// La fase del ciclo del modo de a 6: una redondilla (3v3) alterna con un pico a pico
/// (tres manos 1v1 en secuencia). RULES_Afinadas.md §"Partidas de a 6". Sólo es
/// relevante con seis jugadores; en 1v1, 2v2 y 3v3 la fase es siempre Redondilla.
/// Redondilla es el valor por defecto (una partida arranca con una redondilla).
/// </summary>
public enum FaseCiclo
{
    Redondilla,
    PicoAPico,
}
