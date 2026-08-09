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
        if (e.Terminado)
            return Array.Empty<Accion>();

        // Con un canto pendiente, sólo responde el equipo al que le toca: quiero o no quiero.
        if (e.HayCantoPendiente)
        {
            return e.EquipoDe(jugador) == e.EquipoResponde
                ? new Accion[] { new Quiero(jugador), new NoQuiero(jugador) }
                : Array.Empty<Accion>();
        }

        if (!jugador.Equals(e.Turno))
            return Array.Empty<Accion>();

        var acciones = e.Manos[jugador.Valor]
            .Select(carta => (Accion)new TirarCarta(jugador, carta))
            .ToList();

        if (PuedeCantarTruco(e, jugador))
            acciones.Add(new CantarTruco(jugador));

        return acciones;
    }

    // Se puede cantar/revirar el truco en tu turno, si no se llegó al vale cuatro, y sólo
    // si arranca cualquiera (Nada) o tu equipo es el que quiso el canto anterior.
    private static bool PuedeCantarTruco(EstadoPartida e, JugadorId jugador) =>
        jugador.Equals(e.Turno)
        && e.Truco != NivelTruco.ValeCuatro
        && (e.EquipoQuePuedeRevirar is null || e.EquipoDe(jugador) == e.EquipoQuePuedeRevirar);

    /// <summary>Aplica una acción y devuelve el estado nuevo. Lanza si la acción es ilegal.</summary>
    public static EstadoPartida Aplicar(EstadoPartida e, Accion accion)
    {
        if (e.Terminado)
            throw new InvalidOperationException("El partido ya terminó.");

        return accion switch
        {
            TirarCarta t => AplicarTirar(e, t),
            CantarTruco c => AplicarCantarTruco(e, c),
            Quiero q => AplicarQuiero(e, q),
            NoQuiero n => AplicarNoQuiero(e, n),
            _ => throw new ArgumentException($"Acción no soportada: {accion.GetType().Name}.", nameof(accion)),
        };
    }

    private static EstadoPartida AplicarCantarTruco(EstadoPartida e, CantarTruco c)
    {
        if (e.HayCantoPendiente)
            throw new InvalidOperationException("Ya hay un canto esperando respuesta.");
        if (!PuedeCantarTruco(e, c.Jugador))
            throw new InvalidOperationException($"El jugador {c.Jugador.Valor} no puede cantar truco ahora.");

        // Propone el nivel siguiente; responde el equipo rival.
        return e with
        {
            TrucoPendiente = SiguienteNivel(e.Truco),
            EquipoResponde = OtroEquipo(e.EquipoDe(c.Jugador)),
        };
    }

    private static EstadoPartida AplicarQuiero(EstadoPartida e, Quiero q)
    {
        ValidarRespuesta(e, q.Jugador);

        // El canto queda querido y el equipo que quiso pasa a ser el que puede revirar.
        return e with
        {
            Truco = e.TrucoPendiente!.Value,
            TrucoPendiente = null,
            EquipoResponde = null,
            EquipoQuePuedeRevirar = e.EquipoDe(q.Jugador),
        };
    }

    private static EstadoPartida AplicarNoQuiero(EstadoPartida e, NoQuiero n)
    {
        ValidarRespuesta(e, n.Jugador);

        // El que cantó (el rival del que responde) se lleva el valor del último canto querido.
        var ganador = OtroEquipo(e.EquipoDe(n.Jugador));
        return CerrarMano(e, ganador, ValorTruco(e.Truco));
    }

    private static void ValidarRespuesta(EstadoPartida e, JugadorId jugador)
    {
        if (!e.HayCantoPendiente)
            throw new InvalidOperationException("No hay ningún canto para responder.");
        if (e.EquipoDe(jugador) != e.EquipoResponde)
            throw new InvalidOperationException($"El jugador {jugador.Valor} no es quien tiene que responder.");
    }

    private static EstadoPartida AplicarTirar(EstadoPartida e, TirarCarta t)
    {
        if (e.HayCantoPendiente)
            throw new InvalidOperationException("Hay un canto sin responder; no se puede tirar carta.");
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
        {
            var eTrasBaza = e with { Manos = manos, BazasGanadas = bazasGanadas, JugadasBaza = new List<Jugada>() };
            return CerrarMano(eTrasBaza, resultadoMano.Ganador, ValorTruco(e.Truco));
        }

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

    // Cierra la mano: acredita los puntos al ganador y reparte la siguiente, salvo que el
    // partido haya terminado.
    private static EstadoPartida CerrarMano(EstadoPartida e, EquipoId ganador, int puntos)
    {
        var contador = e.Contador.Sumar(ganador, puntos);

        if (contador.Termino)
            return e with { Contador = contador };

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

    // El otro equipo (siempre son dos: 0 y 1).
    private static EquipoId OtroEquipo(EquipoId equipo) => new(1 - equipo.Valor);

    private static int ValorTruco(NivelTruco nivel) => (int)nivel;

    private static NivelTruco SiguienteNivel(NivelTruco nivel) => nivel switch
    {
        NivelTruco.Nada => NivelTruco.Truco,
        NivelTruco.Truco => NivelTruco.Retruco,
        NivelTruco.Retruco => NivelTruco.ValeCuatro,
        _ => throw new InvalidOperationException("El vale cuatro no se puede revirar."),
    };

    // Semilla determinista por mano: la base combinada con el número de mano.
    private static int SemillaDeMano(int semilla, int numeroDeMano) => unchecked(semilla + numeroDeMano);
}
