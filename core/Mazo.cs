namespace Domain;

/// <summary>
/// El mazo español de 40 cartas como pila ordenada e inmutable. El índice 0 es el
/// tope: la próxima carta a repartir. Toda operación devuelve un Mazo nuevo; nada muta.
///
/// Modela el flujo de la mesa: <see cref="Barajar"/> (lo hace el que reparte),
/// <see cref="Cortar"/> (lo hace el rival, para evitar el "paquete") y
/// <see cref="Repartir"/> (tres cartas a cada uno y recién ahí la muestra).
/// </summary>
public sealed class Mazo
{
    /// <summary>Cartas que recibe cada jugador. §"Como se juega cada ronda".</summary>
    public const int CartasPorMano = 3;

    // Los diez números del mazo español, sin 8 ni 9. §"Jerarquia en formato programacion".
    private static readonly int[] NumerosDelMazo = { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };

    private readonly IReadOnlyList<Carta> _cartas;

    private Mazo(IReadOnlyList<Carta> cartas) => _cartas = cartas;

    /// <summary>Las cartas en orden actual. Índice 0 = tope (la próxima a repartir).</summary>
    public IReadOnlyList<Carta> Cartas => _cartas;

    /// <summary>Cuántas cartas quedan en el mazo.</summary>
    public int Cantidad => _cartas.Count;

    /// <summary>
    /// Las 40 cartas en orden canónico (por palo y número), sin 8 ni 9 y sin duplicados.
    /// Es el mazo antes de barajar.
    /// </summary>
    public static Mazo Completo()
    {
        var cartas = new List<Carta>(NumerosDelMazo.Length * 4);
        foreach (Palo palo in Enum.GetValues<Palo>())
            foreach (int numero in NumerosDelMazo)
                cartas.Add(new Carta(numero, palo));
        return new Mazo(cartas);
    }

    /// <summary>Devuelve un mazo con las mismas cartas en el orden que decida el barajador.</summary>
    public Mazo Barajar(IBarajador barajador) => new Mazo(barajador.Barajar(_cartas));

    /// <summary>
    /// El corte: el rival levanta las primeras <paramref name="posicion"/> cartas y las
    /// manda abajo. Queda <c>[posicion..]</c> seguido de <c>[0..posicion)</c>. Evita que
    /// el que baraja se arme el "paquete".
    /// <paramref name="posicion"/> ∈ [1, Cantidad-1]: un corte deja cartas de los dos lados.
    /// </summary>
    public Mazo Cortar(int posicion)
    {
        if (posicion < 1 || posicion > _cartas.Count - 1)
            throw new ArgumentOutOfRangeException(
                nameof(posicion), posicion,
                "El corte tiene que dejar cartas de los dos lados.");

        var cortado = new List<Carta>(_cartas.Count);
        cortado.AddRange(_cartas.Skip(posicion));
        cortado.AddRange(_cartas.Take(posicion));
        return new Mazo(cortado);
    }

    /// <summary>
    /// Reparte tres cartas a cada jugador y recién después da vuelta la muestra, como en
    /// la mesa real. Reparto round-robin: una carta a cada jugador por vuelta, tres vueltas
    /// (la carta i va al jugador i % jugadores). La muestra es la carta que sigue a las
    /// repartidas; el resto queda en el mazo.
    /// </summary>
    public Reparto Repartir(int cantidadJugadores)
    {
        if (cantidadJugadores < 1)
            throw new ArgumentOutOfRangeException(
                nameof(cantidadJugadores), cantidadJugadores, "Tiene que haber al menos un jugador.");

        int aRepartir = cantidadJugadores * CartasPorMano;
        if (aRepartir + 1 > _cartas.Count)
            throw new InvalidOperationException(
                $"No alcanzan las cartas: {cantidadJugadores} jugadores necesitan {aRepartir} cartas más la muestra.");

        var manos = new List<Carta>[cantidadJugadores];
        for (int j = 0; j < cantidadJugadores; j++)
            manos[j] = new List<Carta>(CartasPorMano);

        for (int i = 0; i < aRepartir; i++)
            manos[i % cantidadJugadores].Add(_cartas[i]);

        var muestra = new Muestra(_cartas[aRepartir]);
        var resto = new Mazo(_cartas.Skip(aRepartir + 1).ToList());

        return new Reparto(
            manos.Select(m => (IReadOnlyList<Carta>)m).ToList(),
            muestra,
            resto);
    }
}
