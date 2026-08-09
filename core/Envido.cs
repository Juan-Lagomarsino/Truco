namespace Domain;

/// <summary>
/// Calcula el envido de una mano de tres cartas, dada la muestra.
/// RULES_Afinadas.md §"El toque de envido" / "Como se cuenta".
///
/// Alcance: manos sin flor (0 o 1 pieza). Con dos o más piezas la mano es flor y el
/// envido no está definido (ver PREGUNTAS_ABIERTAS A2). La interacción "la flor anula
/// el envido" la resuelve el reductor, no este cálculo.
/// </summary>
public static class Envido
{
    // Una carta que no es pieza vale a lo sumo 7; una pieza vale 27 o más. Cualquier
    // umbral entre 7 y 27 separa; usamos "más de 7".
    private static bool EsPieza(int valorTanto) => valorTanto > 7;

    /// <summary>El envido de la mano. Sin flor, cae en [0, 37].</summary>
    public static int De(IReadOnlyList<Carta> mano, Muestra muestra)
    {
        if (mano.Count != 3)
            throw new ArgumentException("Una mano de envido tiene tres cartas.", nameof(mano));

        var cartas = mano
            .Select(c => (c.Palo, Valor: Tantos.De(c, muestra)))
            .ToArray();

        int piezas = cartas.Count(c => EsPieza(c.Valor));

        if (piezas >= 2)
            throw new InvalidOperationException(
                "Una mano con dos o más piezas es flor, no tiene envido (ver PREGUNTAS_ABIERTAS A2).");

        // Con una pieza: la pieza + la mejor de las otras dos, sin importar el palo.
        if (piezas == 1)
        {
            int pieza = cartas.First(c => EsPieza(c.Valor)).Valor;
            int mejorOtra = cartas.Where(c => !EsPieza(c.Valor)).Max(c => c.Valor);
            return pieza + mejorOtra;
        }

        // Sin piezas: si hay dos del mismo palo, 20 + esas dos; si no, la más alta.
        int mejorPar = cartas
            .GroupBy(c => c.Palo)
            .Where(g => g.Count() >= 2)
            .Select(g => g.OrderByDescending(c => c.Valor).Take(2).Sum(c => c.Valor))
            .DefaultIfEmpty(-1)
            .Max();

        return mejorPar >= 0
            ? 20 + mejorPar
            : cartas.Max(c => c.Valor);
    }
}
