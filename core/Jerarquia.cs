namespace Domain;

/// <summary>
/// La jerarquía de fuerza de las cartas dada la muestra.
/// RULES_Afinadas.md §"Jerarquia en formato programacion": la tabla de 19 niveles,
/// de mejor a peor, incluido el 12 espejo.
///
/// Es una de las tres funciones sobre una carta (identidad, fuerza, tantos) y NO es
/// monótona con los tantos: el 11 y el 10 de la muestra tienen distinta fuerza pero
/// los mismos tantos. Ver core-dominio.
/// </summary>
public static class Jerarquia
{
    /// <summary>
    /// Fuerza de una carta dada la muestra. Más alto = más fuerte. Dos cartas con la
    /// misma fuerza empatan (van parda).
    /// </summary>
    public static int Fuerza(Carta carta, Muestra muestra) => 20 - Nivel(carta, muestra);

    // Nivel 1 = la más fuerte, 19 = la más débil. Sigue la tabla del documento al pie.
    private static int Nivel(Carta carta, Muestra muestra)
    {
        int n = carta.Numero;
        Palo p = carta.Palo;
        Palo y = muestra.PaloDePiezas;

        // Piezas: 2,4,5,11,10 del palo de la muestra (niveles 1-5).
        if (p == y)
        {
            int pieza = IndiceDePieza(n);
            if (pieza >= 0) return 1 + pieza;

            // El 12 del palo de la muestra espeja a la muestra, pero SOLO si la muestra
            // es pieza. En ese caso ocupa el nivel de la pieza que copia.
            if (n == 12 && muestra.EsPieza)
                return 1 + IndiceDePieza(muestra.Carta.Numero);
        }

        // Matas (niveles 6-9).
        if (n == 1 && p == Palo.Espada) return 6;
        if (n == 1 && p == Palo.Basto) return 7;
        if (n == 7 && p == Palo.Espada) return 8;
        if (n == 7 && p == Palo.Oro) return 9;

        // Chicas, negras y blancas (niveles 10-19).
        if (n == 3) return 10;                                  // (3, ∀ p)
        if (n == 2 && p != y) return 11;                        // (2, p != muestra)
        if (n == 1 && p != Palo.Espada && p != Palo.Basto) return 12; // (1, ni espada ni basto)
        if (n == 12) return 13;                                 // 12 común (el espejo ya se resolvió arriba)
        if (n == 11 && p != y) return 14;                       // (11, p != muestra)
        if (n == 10 && p != y) return 15;                       // (10, p != muestra)
        if (n == 7 && p != Palo.Espada && p != Palo.Oro) return 16;   // (7, ni espada ni oro)
        if (n == 6) return 17;                                  // (6, ∀ p)
        if (n == 5 && p != y) return 18;                        // (5, p != muestra)
        if (n == 4 && p != y) return 19;                        // (4, p != muestra)

        throw new InvalidOperationException(
            $"Carta sin nivel de jerarquía: {n} de {p} con muestra {muestra.Carta.Numero} de {y}.");
    }

    /// <summary>Índice de un número en el orden de fuerza de las piezas (2,4,5,11,10), o -1.</summary>
    private static int IndiceDePieza(int numero)
    {
        var piezas = Muestra.NumerosDePieza; // [2,4,5,11,10], en orden de fuerza
        for (int i = 0; i < piezas.Count; i++)
            if (piezas[i] == numero) return i;
        return -1;
    }
}
