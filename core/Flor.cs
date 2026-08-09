namespace Domain;

/// <summary>
/// Detecta y cuenta la flor de una mano de tres cartas, dada la muestra.
/// RULES_Afinadas.md §"El canto de la flor".
///
/// Formas de flor: tres del mismo palo; una pieza + dos del mismo palo entre ellas;
/// dos piezas + cualquier carta; tres piezas. El 12 espejo cuenta como pieza (C1).
///
/// Recuento con precedencia por cantidad de piezas (B1): una pieza siempre aporta su
/// valor entero, y las piezas secundarias aportan sus unidades (el último dígito).
///
/// Esta clase no decide si la flor se canta ni cómo se enfrentan dos flores: eso es
/// del reductor. Acá sólo está el hecho de tener flor y cuánto vale.
/// </summary>
public static class Flor
{
    // Una carta que no es pieza vale a lo sumo 7; una pieza vale 27 o más.
    private static bool EsPieza(int valorTanto) => valorTanto > 7;

    /// <summary>¿La mano tiene flor?</summary>
    public static bool Hay(IReadOnlyList<Carta> mano, Muestra muestra)
    {
        var cartas = Clasificar(mano, muestra);
        int piezas = cartas.Count(c => c.EsPieza);

        if (piezas >= 2) return true;                       // dos o tres piezas

        if (piezas == 1)                                    // una pieza + dos del mismo palo entre ellas
        {
            var otras = cartas.Where(c => !c.EsPieza).ToArray();
            return otras[0].Palo == otras[1].Palo;
        }

        return cartas.All(c => c.Palo == cartas[0].Palo);   // tres del mismo palo
    }

    /// <summary>Los tantos de la flor. Lanza si la mano no tiene flor. Cae en [20, 47].</summary>
    public static int De(IReadOnlyList<Carta> mano, Muestra muestra)
    {
        if (!Hay(mano, muestra))
            throw new InvalidOperationException("Esta mano no tiene flor.");

        var cartas = Clasificar(mano, muestra);
        var piezas = cartas.Where(c => c.EsPieza).OrderByDescending(c => c.Valor).ToArray();
        var otras = cartas.Where(c => !c.EsPieza).ToArray();

        return piezas.Length switch
        {
            3 => piezas[0].Valor + Unidades(piezas[1].Valor) + Unidades(piezas[2].Valor),
            2 => piezas[0].Valor + Unidades(piezas[1].Valor) + otras[0].Valor,
            1 => piezas[0].Valor + otras[0].Valor + otras[1].Valor,
            _ => 20 + cartas.Sum(c => c.Valor),
        };
    }

    // Las unidades de una pieza son su último dígito: 30→0, 29→9, 28→8, 27→7.
    private static int Unidades(int valorPieza) => valorPieza % 10;

    private static (Palo Palo, int Valor, bool EsPieza)[] Clasificar(IReadOnlyList<Carta> mano, Muestra muestra)
    {
        if (mano.Count != 3)
            throw new ArgumentException("Una mano de flor tiene tres cartas.", nameof(mano));

        return mano
            .Select(c =>
            {
                int valor = Tantos.De(c, muestra);
                return (c.Palo, Valor: valor, EsPieza: EsPieza(valor));
            })
            .ToArray();
    }
}
