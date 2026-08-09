namespace Domain;

/// <summary>
/// Cuánto vale una carta para el tanto (envido y flor), dada la muestra.
/// RULES_Afinadas.md §"Cuanto vale cada carta".
///
/// Es la tercera función sobre una carta (identidad, fuerza, tantos) y NO es monótona
/// con la fuerza: el 11 y el 10 de la muestra difieren en fuerza pero valen los dos 27.
/// </summary>
public static class Tantos
{
    /// <summary>Valor de una carta para el tanto. Siempre en [0, 30].</summary>
    public static int De(Carta carta, Muestra muestra)
    {
        int n = carta.Numero;

        if (carta.Palo == muestra.PaloDePiezas)
        {
            // Pieza del palo de la muestra: vale su valor especial.
            if (Muestra.EsNumeroDePieza(n))
                return ValorDePieza(n);

            // El 12 del palo de la muestra espeja a la muestra sólo si la muestra es pieza,
            // y entonces vale como la pieza que copia.
            if (n == 12 && muestra.EsPieza)
                return ValorDePieza(muestra.Carta.Numero);
        }

        // No pieza: 1..7 valen su número; 10, 11 y 12 valen 0.
        return n <= 7 ? n : 0;
    }

    // Valor especial de cada pieza. §"Cuanto vale cada carta":
    // 2→30, 4→29, 5→28, 11→27, 10→27.
    private static int ValorDePieza(int numero) => numero switch
    {
        2 => 30,
        4 => 29,
        5 => 28,
        11 => 27,
        10 => 27,
        _ => throw new ArgumentOutOfRangeException(nameof(numero), numero, "No es un número de pieza."),
    };
}
