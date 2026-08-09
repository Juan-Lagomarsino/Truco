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

        // Ventana de cierre: denuncias de flor escondida antes de acreditar y repartir.
        if (e.Cierre is not null)
        {
            return e.DenunciasPendientes.Any(j => j.Equals(jugador))
                ? new Accion[] { new DenunciarFlor(jugador), new Pasar(jugador) }
                : Array.Empty<Accion>();
        }

        // Un bid de flor pendiente tiene la máxima prioridad: sólo responde el rival con flor.
        if (e.HayFlorPendiente)
        {
            return e.EquipoDe(jugador) == e.FlorPendiente!.Responde
                ? new Accion[] { new Quiero(jugador), new NoQuiero(jugador) }
                : Array.Empty<Accion>();
        }

        // El envido tiene prioridad: si está pendiente, sólo responde el equipo que debe.
        if (e.HayEnvidoPendiente)
        {
            if (e.EquipoDe(jugador) != e.EnvidoPendiente!.Responde)
                return Array.Empty<Accion>();

            var respuesta = new List<Accion> { new Quiero(jugador), new NoQuiero(jugador) };
            foreach (var canto in RevirosDeEnvido(e.EnvidoPendiente.Ultimo))
                respuesta.Add(new CantarEnvido(jugador, canto));
            AgregarCantosDeFlor(e, jugador, respuesta);
            return respuesta;
        }

        // Con un truco pendiente responde el equipo al que le toca; además puede tocar envido.
        if (e.HayCantoPendiente)
        {
            if (e.EquipoDe(jugador) != e.EquipoResponde)
                return Array.Empty<Accion>();

            var respuesta = new List<Accion> { new Quiero(jugador), new NoQuiero(jugador) };
            AgregarAperturasDeEnvido(e, jugador, respuesta);
            AgregarCantosDeFlor(e, jugador, respuesta);
            return respuesta;
        }

        if (!jugador.Equals(e.Turno))
            return Array.Empty<Accion>();

        var acciones = e.Manos[jugador.Valor]
            .Select(carta => (Accion)new TirarCarta(jugador, carta))
            .ToList();

        if (PuedeCantarTruco(e, jugador))
            acciones.Add(new CantarTruco(jugador));
        AgregarAperturasDeEnvido(e, jugador, acciones);
        AgregarCantosDeFlor(e, jugador, acciones);
        acciones.Add(new IrseAlMazo(jugador)); // sin cantos pendientes y en tu turno, siempre podés irte

        return acciones;
    }

    private static void AgregarAperturasDeEnvido(EstadoPartida e, JugadorId jugador, List<Accion> acciones)
    {
        if (!PuedeIniciarEnvido(e, jugador)) return;
        acciones.Add(new CantarEnvido(jugador, EnvidoCanto.Envido));
        acciones.Add(new CantarEnvido(jugador, EnvidoCanto.RealEnvido));
        acciones.Add(new CantarEnvido(jugador, EnvidoCanto.FaltaEnvido));
    }

    // El envido se toca en la primera baza, si todavía no se jugó, no hay flor resuelta
    // (la flor anula el envido, F1) y el jugador no tiró su carta (A1).
    private static bool PuedeIniciarEnvido(EstadoPartida e, JugadorId jugador) =>
        !e.EnvidoJugado
        && !e.FlorResuelta
        && e.BazasGanadas.Count == 0
        && !e.JugadasBaza.Any(j => j.Jugador.Equals(jugador));

    private static void AgregarCantosDeFlor(EstadoPartida e, JugadorId jugador, List<Accion> acciones)
    {
        if (!PuedeCantarFlor(e, jugador)) return;
        acciones.Add(new CantarFlor(jugador));
        acciones.Add(new CantarFlorEnvido(jugador));
        acciones.Add(new CantarContraFlorAlResto(jugador));
    }

    // La flor se canta en la primera baza, antes de tirar la propia carta, si el jugador
    // la tiene y no se resolvió otra. Puede cantarla en su turno o al responder un canto.
    private static bool PuedeCantarFlor(EstadoPartida e, JugadorId jugador)
    {
        if (e.HayFlorPendiente || e.FlorResuelta || e.BazasGanadas.Count != 0) return false;
        if (e.JugadasBaza.Any(j => j.Jugador.Equals(jugador))) return false;
        if (!Flor.Hay(e.ManosIniciales[jugador.Valor], e.Muestra)) return false;

        bool esResponsable = e.HayEnvidoPendiente
            ? e.EquipoDe(jugador) == e.EnvidoPendiente!.Responde
            : e.HayCantoPendiente
                ? e.EquipoDe(jugador) == e.EquipoResponde
                : jugador.Equals(e.Turno);
        return esResponsable;
    }

    private static IReadOnlyList<EnvidoCanto> RevirosDeEnvido(EnvidoCanto ultimo) => ultimo switch
    {
        // Sin límite de envidos (B2); tras real envido sólo falta; falta es terminal.
        EnvidoCanto.Envido => new[] { EnvidoCanto.Envido, EnvidoCanto.RealEnvido, EnvidoCanto.FaltaEnvido },
        EnvidoCanto.RealEnvido => new[] { EnvidoCanto.FaltaEnvido },
        _ => Array.Empty<EnvidoCanto>(),
    };

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

        if (e.Cierre is not null && accion is not (DenunciarFlor or Pasar))
            throw new InvalidOperationException("La mano está en la ventana de cierre: sólo denunciar o pasar.");

        return accion switch
        {
            TirarCarta t => AplicarTirar(e, t),
            DenunciarFlor d => AplicarDenunciarFlor(e, d),
            Pasar p => AplicarPasar(e, p),
            CantarFlor cf => AplicarCantarFlor(e, cf),
            CantarFlorEnvido cfe => AplicarBidFlor(e, cfe.Jugador, esContra: false),
            CantarContraFlorAlResto cc => AplicarBidFlor(e, cc.Jugador, esContra: true),
            CantarEnvido ce => AplicarCantarEnvido(e, ce),
            CantarTruco c => AplicarCantarTruco(e, c),
            Quiero q => AplicarQuiero(e, q),
            NoQuiero n => AplicarNoQuiero(e, n),
            IrseAlMazo im => AplicarIrseAlMazo(e, im),
            _ => throw new ArgumentException($"Acción no soportada: {accion.GetType().Name}.", nameof(accion)),
        };
    }

    private static EstadoPartida AplicarIrseAlMazo(EstadoPartida e, IrseAlMazo im)
    {
        if (e.HayFlorPendiente || e.HayEnvidoPendiente || e.HayCantoPendiente)
            throw new InvalidOperationException("No se puede ir al mazo con un canto sin resolver.");
        if (!im.Jugador.Equals(e.Turno))
            throw new InvalidOperationException($"No es el turno del jugador {im.Jugador.Valor}.");

        // El rival se lleva lo que valía la mano (1, o el último truco querido).
        var rival = OtroEquipo(e.EquipoDe(im.Jugador));
        return TerminarMano(e, rival, ValorTruco(e.Truco));
    }

    // Antes de acreditar, abre la ventana de denuncias si hay flor escondida; si no, cierra directo.
    private static EstadoPartida TerminarMano(EstadoPartida e, EquipoId ganadorTruco, int puntosTruco)
    {
        var reclamadores = Reclamadores(e);
        if (reclamadores.Count == 0)
            return CerrarMano(e, ganadorTruco, puntosTruco);

        return e with
        {
            Cierre = new CierrePendiente(ganadorTruco, puntosTruco),
            DenunciasPendientes = reclamadores,
        };
    }

    // Jugadores cuyo rival tenía flor y no la cantó (flor escondida reclamable). Si se cantó
    // flor no hay nada escondido. Es 1v1: el rival es el otro jugador.
    private static IReadOnlyList<JugadorId> Reclamadores(EstadoPartida e)
    {
        if (e.FlorResuelta) return Array.Empty<JugadorId>();

        var reclamadores = new List<JugadorId>();
        for (int j = 0; j < e.CantidadJugadores; j++)
        {
            var rival = new JugadorId((j + 1) % e.CantidadJugadores);
            if (Flor.Hay(e.ManosIniciales[rival.Valor], e.Muestra))
                reclamadores.Add(new JugadorId(j));
        }
        return reclamadores;
    }

    private static EstadoPartida AplicarDenunciarFlor(EstadoPartida e, DenunciarFlor d)
    {
        if (e.Cierre is null || !e.DenunciasPendientes.Any(j => j.Equals(d.Jugador)))
            throw new InvalidOperationException("No hay una flor escondida para denunciar ahora.");

        // La flor escondida (3) pasa al que denuncia. Estamos en el cierre: se acredita ya.
        var contador = e.Contador.Sumar(e.EquipoDe(d.Jugador), 3);
        var pendientes = e.DenunciasPendientes.Where(j => !j.Equals(d.Jugador)).ToList();

        if (contador.Termino)
            return e with { Contador = contador, Cierre = null, DenunciasPendientes = Array.Empty<JugadorId>() };

        return FinalizarCierre(e with { Contador = contador, DenunciasPendientes = pendientes });
    }

    private static EstadoPartida AplicarPasar(EstadoPartida e, Pasar p)
    {
        if (e.Cierre is null || !e.DenunciasPendientes.Any(j => j.Equals(p.Jugador)))
            throw new InvalidOperationException("No hay nada que pasar ahora.");

        var pendientes = e.DenunciasPendientes.Where(j => !j.Equals(p.Jugador)).ToList();
        return FinalizarCierre(e with { DenunciasPendientes = pendientes });
    }

    // Cuando ya nadie puede denunciar, cierra la mano con el resultado de truco guardado.
    private static EstadoPartida FinalizarCierre(EstadoPartida e)
    {
        if (e.DenunciasPendientes.Count > 0)
            return e;

        var cierre = e.Cierre!;
        return CerrarMano(e with { Cierre = null }, cierre.GanadorTruco, cierre.PuntosTruco);
    }

    private static EstadoPartida AplicarCantarFlor(EstadoPartida e, CantarFlor cf)
    {
        if (!PuedeCantarFlor(e, cf.Jugador))
            throw new InvalidOperationException($"El jugador {cf.Jugador.Valor} no puede cantar flor ahora.");

        // Base (14a): la flor más alta cobra 3; empate, el equipo mano. Cantar flor anula
        // el envido (F1). Los bids (Con Flor Envido / Contra Flor al Resto) y la denuncia
        // son 14b/14c.
        return e with
        {
            CobroFlor = new Cobro(FlorGanador(e), 3),
            FlorResuelta = true,
            EnvidoPendiente = null,
            EnvidoJugado = true,
        };
    }

    // Bid de flor: si el rival no tiene flor no hay enfrentamiento (cobra la flor, 3);
    // si tiene, queda pendiente hasta quiero/no quiero.
    private static EstadoPartida AplicarBidFlor(EstadoPartida e, JugadorId jugador, bool esContra)
    {
        if (!PuedeCantarFlor(e, jugador))
            throw new InvalidOperationException($"El jugador {jugador.Valor} no puede cantar flor ahora.");

        var otro = new JugadorId((jugador.Valor + 1) % e.CantidadJugadores);
        bool rivalConFlor = Flor.Hay(e.ManosIniciales[otro.Valor], e.Muestra);

        if (!rivalConFlor)
            return e with
            {
                CobroFlor = new Cobro(e.EquipoDe(jugador), 3),
                FlorResuelta = true,
                EnvidoPendiente = null,
                EnvidoJugado = true,
            };

        return e with
        {
            FlorPendiente = new EstadoFlorBid(esContra, OtroEquipo(e.EquipoDe(jugador))),
            EnvidoPendiente = null,
            EnvidoJugado = true,
        };
    }

    // Resuelve un bid de flor. Querido: la flor más alta cobra 5 (Con Flor Envido) o la
    // falta + las flores (Contra Flor al Resto). No querido: el que cantó cobra 3 (A3).
    private static EstadoPartida ResolverBidFlor(EstadoPartida e, JugadorId jugador, bool quiere)
    {
        var pend = e.FlorPendiente!;
        if (e.EquipoDe(jugador) != pend.Responde)
            throw new InvalidOperationException($"El jugador {jugador.Valor} no es quien tiene que responder la flor.");

        EquipoId ganador;
        int puntos;
        if (quiere)
        {
            ganador = FlorGanador(e);
            puntos = pend.EsContraFlorAlResto
                ? FaltaEnvido(e.Contador) + 3 * FloresEnJuego(e)
                : 5;
        }
        else
        {
            ganador = OtroEquipo(pend.Responde); // el que cantó
            puntos = 3;
        }

        return e with
        {
            CobroFlor = new Cobro(ganador, puntos),
            FlorPendiente = null,
            FlorResuelta = true,
        };
    }

    // Quién se lleva la flor entre los dos jugadores (1v1): la más alta; empate, equipo mano.
    private static EquipoId FlorGanador(EstadoPartida e)
    {
        int f0 = FlorParaComparar(e.ManosIniciales[0], e.Muestra);
        int f1 = FlorParaComparar(e.ManosIniciales[1], e.Muestra);

        if (f0 > f1) return new EquipoId(0);
        if (f1 > f0) return new EquipoId(1);
        return e.EquipoDe(e.JugadorMano); // empate → equipo mano
    }

    private static int FlorParaComparar(IReadOnlyList<Carta> mano, Muestra muestra) =>
        Flor.Hay(mano, muestra) ? Flor.De(mano, muestra) : -1;

    private static int FloresEnJuego(EstadoPartida e) =>
        e.ManosIniciales.Count(m => Flor.Hay(m, e.Muestra));

    private static EstadoPartida AplicarCantarEnvido(EstadoPartida e, CantarEnvido ce)
    {
        // Reviro de un envido en curso.
        if (e.HayEnvidoPendiente)
        {
            var pend = e.EnvidoPendiente!;
            if (e.EquipoDe(ce.Jugador) != pend.Responde)
                throw new InvalidOperationException($"El jugador {ce.Jugador.Valor} no puede revirar el envido ahora.");
            if (!RevirosDeEnvido(pend.Ultimo).Contains(ce.Canto))
                throw new InvalidOperationException($"No se puede revirar a {ce.Canto} desde {pend.Ultimo}.");

            int valor = ValorEnvido(e, pend.ValorSiQuiero, ce.Canto);
            return e with
            {
                EnvidoPendiente = new EstadoEnvido(ce.Canto, valor, pend.ValorSiQuiero, OtroEquipo(e.EquipoDe(ce.Jugador))),
            };
        }

        // Apertura del envido.
        if (!PuedeIniciarEnvido(e, ce.Jugador))
            throw new InvalidOperationException($"El jugador {ce.Jugador.Valor} no puede tocar envido ahora.");

        int apertura = ValorEnvido(e, 0, ce.Canto);
        return e with
        {
            EnvidoPendiente = new EstadoEnvido(ce.Canto, apertura, 1, OtroEquipo(e.EquipoDe(ce.Jugador))),
        };
    }

    private static EstadoPartida AplicarCantarTruco(EstadoPartida e, CantarTruco c)
    {
        if (e.HayFlorPendiente || e.HayEnvidoPendiente)
            throw new InvalidOperationException("Primero se resuelve la flor o el envido.");
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
        if (e.HayFlorPendiente)
            return ResolverBidFlor(e, q.Jugador, quiere: true);
        if (e.HayEnvidoPendiente)
            return ResolverEnvido(e, q.Jugador, quiere: true);

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
        if (e.HayFlorPendiente)
            return ResolverBidFlor(e, n.Jugador, quiere: false);
        if (e.HayEnvidoPendiente)
            return ResolverEnvido(e, n.Jugador, quiere: false);

        ValidarRespuesta(e, n.Jugador);

        // El que cantó (el rival del que responde) se lleva el valor del último canto querido.
        var ganador = OtroEquipo(e.EquipoDe(n.Jugador));
        return TerminarMano(e, ganador, ValorTruco(e.Truco));
    }

    // Resuelve el envido pendiente: si se quiere, gana el de más puntos (empate: el mano);
    // si no se quiere, el que cantó se lleva el valor del último canto querido. La mano sigue.
    private static EstadoPartida ResolverEnvido(EstadoPartida e, JugadorId jugador, bool quiere)
    {
        var pend = e.EnvidoPendiente!;
        if (e.EquipoDe(jugador) != pend.Responde)
            throw new InvalidOperationException($"El jugador {jugador.Valor} no es quien tiene que responder el envido.");

        var (ganador, puntos) = quiere
            ? (EnvidoGanador(e), pend.ValorSiQuiero)
            : (OtroEquipo(pend.Responde), pend.ValorSiNoQuiero);

        return e with
        {
            CobroEnvido = new Cobro(ganador, puntos),
            EnvidoPendiente = null,
            EnvidoJugado = true,
        };
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
        if (e.HayFlorPendiente || e.HayEnvidoPendiente || e.HayCantoPendiente)
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

        // Baza completa: resolverla por equipo.
        var resultado = Baza.Resolver(
            jugadas.Select(j => (j.Carta, e.EquipoDe(j.Jugador))).ToList(), e.Muestra);
        var ganadorBaza = resultado.EsParda
            ? GanadorBaza.Parda
            : GanadorBaza.De(e.EquipoDe(jugadas[resultado.Ganador].Jugador));

        var bazasGanadas = e.BazasGanadas.Append(ganadorBaza).ToList();
        var resultadoMano = Mano.Resolver(bazasGanadas, e.EquipoDe(e.JugadorMano));

        if (resultadoMano.EstaDefinida)
        {
            var eTrasBaza = e with { Manos = manos, BazasGanadas = bazasGanadas, JugadasBaza = new List<Jugada>() };
            return TerminarMano(eTrasBaza, resultadoMano.Ganador, ValorTruco(e.Truco));
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

    // Cierra la mano acreditando en orden flor → envido → truco (B6, F4), cortando apenas
    // un equipo llega al objetivo. Después reparte la mano siguiente.
    private static EstadoPartida CerrarMano(EstadoPartida e, EquipoId ganadorTruco, int puntosTruco)
    {
        var contador = e.Contador;

        if (e.CobroFlor is Cobro cf)
        {
            contador = contador.Sumar(cf.Equipo, cf.Puntos);
            if (contador.Termino) return e with { Contador = contador };
        }
        if (e.CobroEnvido is Cobro ce)
        {
            contador = contador.Sumar(ce.Equipo, ce.Puntos);
            if (contador.Termino) return e with { Contador = contador };
        }

        contador = contador.Sumar(ganadorTruco, puntosTruco);
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
            ManosIniciales = reparto.Manos,
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

    // Valor de un canto de envido sobre el acumulado (para apertura, acumulado = 0).
    private static int ValorEnvido(EstadoPartida e, int acumulado, EnvidoCanto canto) => canto switch
    {
        EnvidoCanto.Envido => acumulado + 2,
        EnvidoCanto.RealEnvido => acumulado + 3,
        EnvidoCanto.FaltaEnvido => FaltaEnvido(e.Contador),
        _ => throw new ArgumentOutOfRangeException(nameof(canto), canto, "Canto de envido desconocido."),
    };

    // Falta Envido (A4): lo que le falta al que va primero para cerrar su etapa. Si el
    // líder está en malas, hasta la mitad; si ya está en buenas, hasta el largo.
    private static int FaltaEnvido(Contador contador)
    {
        int lider = Math.Max(contador.Puntos(new EquipoId(0)), contador.Puntos(new EquipoId(1)));
        return lider < contador.Mitad ? contador.Mitad - lider : contador.Largo - lider;
    }

    // Quién gana el envido: el equipo con más puntos (cada equipo juega su mejor mano);
    // empate, el equipo mano (B8).
    private static EquipoId EnvidoGanador(EstadoPartida e)
    {
        int env0 = EnvidoDeEquipo(e, new EquipoId(0));
        int env1 = EnvidoDeEquipo(e, new EquipoId(1));

        if (env0 > env1) return new EquipoId(0);
        if (env1 > env0) return new EquipoId(1);
        return e.EquipoDe(e.JugadorMano); // empate: gana el equipo mano
    }

    // El envido de un equipo es el mejor de sus jugadores.
    private static int EnvidoDeEquipo(EstadoPartida e, EquipoId equipo)
    {
        int mejor = -1;
        for (int j = 0; j < e.CantidadJugadores; j++)
            if (e.EquipoDe(new JugadorId(j)) == equipo)
                mejor = Math.Max(mejor, EnvidoParaComparar(e.ManosIniciales[j], e.Muestra));
        return mejor;
    }

    // Interino (Paso 13): una mano con flor (2+ piezas) no tiene envido definido; se la
    // trata como que no compite (-1). El Paso 14 implementa la anulación por flor.
    private static int EnvidoParaComparar(IReadOnlyList<Carta> mano, Muestra muestra)
    {
        int piezas = mano.Count(c => Tantos.De(c, muestra) > 7);
        return piezas >= 2 ? -1 : Envido.De(mano, muestra);
    }

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
