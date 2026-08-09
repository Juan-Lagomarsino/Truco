namespace Domain;

/// <summary>
/// El reductor de la partida: la única autoridad sobre qué se puede hacer y cómo
/// cambia el estado. <see cref="AccionesLegales"/> dice qué acciones son válidas,
/// <see cref="Aplicar"/> las ejecuta (y lanza si son ilegales). Todo es puro y
/// determinista: no baraja con azar externo ni guarda estado estático.
///
/// Alcance actual: 1v1, sólo tirar cartas. Gana la mano el que gana dos bazas y suma
/// un punto (truco liso, sin cantos). Cantos, flor e irse al mazo vienen después.
/// </summary>
public static class Partido
{
    /// <summary>Crea una partida nueva y reparte la primera mano.</summary>
    public static EstadoPartida Nueva(
        int largo, int semilla, JugadorId? repartidorInicial = null, int cantidadJugadores = 2)
    {
        if (cantidadJugadores < 2)
            throw new ArgumentOutOfRangeException(nameof(cantidadJugadores), cantidadJugadores, "Hacen falta al menos dos jugadores.");

        var repartidor = repartidorInicial ?? new JugadorId(0);
        if (repartidor.Valor >= cantidadJugadores)
            throw new ArgumentOutOfRangeException(nameof(repartidorInicial), repartidor.Valor, "El repartidor no existe en la mesa.");

        return RepartirMano(numeroDeMano: 0, repartidor, new Contador(largo), semilla, cantidadJugadores);
    }

    /// <summary>Las acciones que puede hacer <paramref name="jugador"/> en este momento.</summary>
    public static IReadOnlyList<Accion> AccionesLegales(EstadoPartida e, JugadorId jugador)
    {
        if (e.Terminado || !jugador.Equals(e.Turno))
            return Array.Empty<Accion>();

        return e.Manos[jugador.Valor]
            .Select(carta => (Accion)new TirarCarta(jugador, carta))
            .ToList();
    }

    /// <summary>Aplica una acción y devuelve el estado nuevo. Lanza si la acción es ilegal.</summary>
    public static EstadoPartida Aplicar(EstadoPartida e, Accion accion)
    {
        if (e.Terminado)
            throw new InvalidOperationException("El partido ya terminó.");

        return accion switch
        {
            TirarCarta t => AplicarTirar(e, t),
            _ => throw new ArgumentException($"Acción no soportada: {accion.GetType().Name}.", nameof(accion)),
        };
    }

    private static EstadoPartida AplicarTirar(EstadoPartida e, TirarCarta t)
    {
        if (!t.Jugador.Equals(e.Turno))
            throw new InvalidOperationException($"No es el turno del jugador {t.Jugador.Valor}.");
        if (!e.Manos[t.Jugador.Valor].Contains(t.Carta))
            throw new InvalidOperationException("El jugador no tiene esa carta.");

        var manos = SacarCarta(e.Manos, t.Jugador, t.Carta);
        var jugadas = e.JugadasBaza.Append(new Jugada(t.Jugador, t.Carta)).ToList();

        // La baza sigue: pasa el turno al siguiente jugador.
        if (jugadas.Count < e.CantidadJugadores)
            return e with { Manos = manos, JugadasBaza = jugadas, Turno = Siguiente(t.Jugador, e.CantidadJugadores) };

        // Baza completa: resolverla.
        var resultado = Baza.Resolver(jugadas.Select(j => j.Carta).ToList(), e.Muestra);
        var ganadorBaza = resultado.EsParda
            ? GanadorBaza.Parda
            : GanadorBaza.De(e.EquipoDe(jugadas[resultado.Ganador].Jugador));

        var bazasGanadas = e.BazasGanadas.Append(ganadorBaza).ToList();
        var resultadoMano = Mano.Resolver(bazasGanadas, e.EquipoDe(e.JugadorMano));

        if (resultadoMano.EstaDefinida)
            return CerrarMano(e, manos, bazasGanadas, resultadoMano.Ganador);

        // La mano sigue: la próxima baza la abre el ganador, o el mano si fue parda (D2).
        var abridor = resultado.EsParda ? e.JugadorMano : jugadas[resultado.Ganador].Jugador;
        return e with
        {
            Manos = manos,
            BazasGanadas = bazasGanadas,
            JugadasBaza = new List<Jugada>(),
            Abridor = abridor,
            Turno = abridor,
        };
    }

    // Cierra la mano: acredita el punto del truco liso y reparte la siguiente, salvo que
    // el partido haya terminado.
    private static EstadoPartida CerrarMano(
        EstadoPartida e, IReadOnlyList<IReadOnlyList<Carta>> manos,
        IReadOnlyList<GanadorBaza> bazasGanadas, EquipoId ganador)
    {
        var contador = e.Contador.Sumar(ganador, 1);

        if (contador.Termino)
            return e with { Manos = manos, BazasGanadas = bazasGanadas, JugadasBaza = new List<Jugada>(), Contador = contador };

        return RepartirMano(
            numeroDeMano: e.NumeroDeMano + 1,
            repartidor: Siguiente(e.Repartidor, e.CantidadJugadores),
            contador,
            e.Semilla,
            e.CantidadJugadores);
    }

    // Reparte una mano nueva de forma determinista desde la semilla y el número de mano.
    private static EstadoPartida RepartirMano(
        int numeroDeMano, JugadorId repartidor, Contador contador, int semilla, int cantidadJugadores)
    {
        var barajador = new BarajadorConSemilla(SemillaDeMano(semilla, numeroDeMano));
        var reparto = Mazo.Completo().Barajar(barajador).Repartir(cantidadJugadores);
        var mano = new JugadorId((repartidor.Valor + 1) % cantidadJugadores);

        return new EstadoPartida
        {
            Contador = contador,
            Semilla = semilla,
            NumeroDeMano = numeroDeMano,
            CantidadJugadores = cantidadJugadores,
            Repartidor = repartidor,
            Muestra = reparto.Muestra,
            Manos = reparto.Manos,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }

    private static IReadOnlyList<IReadOnlyList<Carta>> SacarCarta(
        IReadOnlyList<IReadOnlyList<Carta>> manos, JugadorId jugador, Carta carta) =>
        manos
            .Select((mano, indice) => indice == jugador.Valor
                ? (IReadOnlyList<Carta>)mano.Where(c => !c.Equals(carta)).ToList()
                : mano)
            .ToList();

    private static JugadorId Siguiente(JugadorId jugador, int cantidadJugadores) =>
        new((jugador.Valor + 1) % cantidadJugadores);

    // Semilla determinista por mano: la base combinada con el número de mano.
    private static int SemillaDeMano(int semilla, int numeroDeMano) => unchecked(semilla + numeroDeMano);
}
