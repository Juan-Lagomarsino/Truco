using Domain;

namespace Tests;

// Bloque A del plan nocturno 2 (docs/notas/PLAN_NOCTURNO_2.md): invariantes que tienen que
// valer SIEMPRE, para cualquier semilla y cualquier secuencia de acciones legales, en los
// tres modos (1v1, 2v2, a 6). No dependen de ninguna regla ambigua: son propiedades del
// reductor en sí (Partido.AccionesLegales / Partido.Aplicar), no del contenido de una regla
// del truco.
public class InvariantesFuzzTests
{
    // La misma estrategia de "elegí la acción número pasos % cantidad" que ya usan
    // GrabacionFuzzTests / ModoDeA6FuzzTests: determinista, barre las ramas del reductor sin
    // necesitar una política de juego real, y nunca se traba (A2 lo confirma en el camino).
    private static Accion ElegirDeterminista(EstadoPartida e, int pasos)
    {
        for (int j = 0; j < e.CantidadJugadores; j++)
        {
            var legales = Partido.AccionesLegales(e, new JugadorId(j));
            if (legales.Count > 0) return legales[pasos % legales.Count];
        }
        throw new InvalidOperationException("Ningún jugador tiene acciones legales: no debería pasar en un estado no terminado.");
    }

    // A1 + A2: en cada estado de la partida (no sólo en el final), toda acción candidata que
    // no aparezca en AccionesLegales de ningún jugador tiene que hacer lanzar a Aplicar, y
    // AccionesLegales nunca es vacía en un estado no terminado (para al menos un jugador).
    [Theory]
    [InlineData(2, 3)]
    [InlineData(2, 77)]
    [InlineData(2, 2024)]
    [InlineData(4, 3)]
    [InlineData(4, 77)]
    [InlineData(4, 2024)]
    [InlineData(6, 3)]
    [InlineData(6, 77)]
    [InlineData(6, 2024)]
    public void Aplicar_ConAccionQueNoEsLegal_SiempreLanza_YAccionesLegalesNuncaVaciaEnEstadoVivo(
        int cantidadJugadores, int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: cantidadJugadores);
        int pasos = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos < 40000, "La partida no debería tardar tanto.");

            // A2: siempre hay alguien con al menos una acción legal en un estado no terminado.
            var legalesPorJugador = new List<Accion>[cantidadJugadores];
            bool algunoConAcciones = false;
            for (int j = 0; j < cantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(e, new JugadorId(j));
                legalesPorJugador[j] = legales.ToList();
                if (legales.Count > 0) algunoConAcciones = true;
            }
            Assert.True(algunoConAcciones, "AccionesLegales no debería ser vacía para todos los jugadores en un estado vivo.");

            var legalesDeEsteEstado = legalesPorJugador.SelectMany(l => l).ToHashSet();

            // A2 (complemento): cada acción legal, aplicada sobre este mismo estado, no lanza.
            foreach (var legal in legalesDeEsteEstado)
                Partido.Aplicar(e, legal); // si lanza, el test falla acá

            // A1: cualquier candidata que no esté en el conjunto legal tiene que lanzar.
            // Excepción conocida: ver HALLAZGOS_NOCHE_2.md H1 — un CantarEnvido de apertura
            // fuera de turno no está en AccionesLegales pero Aplicar lo acepta igual, porque
            // PuedeIniciarEnvido (usado por Aplicar) no chequea el turno y AccionesLegales sí.
            // No se toca la lógica del autor; se documenta el hallazgo y se deja afuera de
            // esta aserción para poder seguir verificando A1 sobre todo lo demás.
            foreach (var candidata in CandidatasDeAccion(cantidadJugadores))
            {
                if (legalesDeEsteEstado.Contains(candidata)) continue;
                if (EsHallazgoH1_CantarEnvidoDeAperturaFueraDeTurno(e, candidata)) continue;
                Assert.ThrowsAny<Exception>(() => Partido.Aplicar(e, candidata));
            }

            var elegida = ElegirDeterminista(e, pasos);
            e = Partido.Aplicar(e, elegida);
            pasos++;
        }

        // Estado terminal: A2 dice "salvo estado terminal", así que acá SÍ tiene que estar vacía.
        for (int j = 0; j < cantidadJugadores; j++)
            Assert.Empty(Partido.AccionesLegales(e, new JugadorId(j)));
    }

    // Réplica exacta (sólo lectura de campos públicos) de la condición privada
    // Partido.PuedeIniciarEnvido, para detectar la forma precisa del hallazgo H1
    // (ver HALLAZGOS_NOCHE_2.md): un CantarEnvido de apertura que Aplicar acepta sin
    // chequear turno, aunque AccionesLegales no lo haya ofrecido.
    private static bool EsHallazgoH1_CantarEnvidoDeAperturaFueraDeTurno(EstadoPartida e, Accion candidata)
    {
        if (candidata is not CantarEnvido ce) return false;
        if (e.HayEnvidoPendiente) return false; // eso sería reviro, no apertura

        bool puedeIniciarSegunAplicar =
            !e.EnvidoJugado
            && !e.FlorResuelta
            && e.BazasGanadas.Count == 0
            && !e.JugadasBaza.Any(j => j.Jugador.Equals(ce.Jugador));

        return puedeIniciarSegunAplicar;
    }

    // El universo de acciones candidatas para un jugador: todas las formas sintácticamente
    // posibles de cada tipo de Accion, tirar cualquiera de las 40 cartas incluida. No filtra
    // por si "tiene sentido": la gracia es que Aplicar tiene que rechazar hasta las que no
    // lo tienen (p. ej. tirar una carta que no está en la mano).
    private static IEnumerable<Accion> CandidatasDeAccion(int cantidadJugadores)
    {
        var cartas = Mazo.Completo().Cartas;
        for (int j = 0; j < cantidadJugadores; j++)
        {
            var jugador = new JugadorId(j);
            foreach (var carta in cartas)
                yield return new TirarCarta(jugador, carta);

            yield return new CantarTruco(jugador);
            foreach (EnvidoCanto canto in Enum.GetValues<EnvidoCanto>())
                yield return new CantarEnvido(jugador, canto);
            yield return new CantarFlor(jugador);
            yield return new CantarFlorEnvido(jugador);
            yield return new CantarContraFlorAlResto(jugador);
            yield return new Quiero(jugador);
            yield return new NoQuiero(jugador);
            yield return new IrseAlMazo(jugador);
            yield return new DenunciarFlor(jugador);
            yield return new Pasar(jugador);
        }
    }

    // Reproducción mínima de HALLAZGOS_NOCHE_2.md H1: queda deshabilitado a propósito para
    // no ensuciar el suite (política de bug del plan nocturno), pero visible para que no se
    // repita el hueco si alguien la habilita sin decidir de qué lado está el bug.
    [Fact(Skip = "hallazgo: ver HALLAZGOS_NOCHE_2.md H1 — Aplicar acepta un CantarEnvido de apertura fuera de turno que AccionesLegales no ofrece")]
    public void H1_CantarEnvidoDeAperturaFueraDeTurno_DeberiaLanzarPeroNoLanza()
    {
        var e = Partido.Nueva(largo: 30, semilla: 3, cantidadJugadores: 2);
        Assert.Equal(1, e.Turno.Valor); // jugador 1 es mano; jugador 0 no tiene el turno
        Assert.Empty(Partido.AccionesLegales(e, new JugadorId(0)));

        var accion = new CantarEnvido(new JugadorId(0), EnvidoCanto.Envido);

        // Esto es lo que DEBERÍA pasar (y hoy no pasa): Aplicar debería lanzar porque la
        // acción no está en AccionesLegales del jugador 0.
        Assert.ThrowsAny<Exception>(() => Partido.Aplicar(e, accion));
    }

    // A4: el puntaje de cada equipo nunca es negativo, nunca supera el largo del partido, y
    // es monótono no decreciente a lo largo de una partida completa fuzz, en los tres modos.
    [Theory]
    [InlineData(2, 3)]
    [InlineData(2, 77)]
    [InlineData(2, 2024)]
    [InlineData(4, 3)]
    [InlineData(4, 77)]
    [InlineData(4, 2024)]
    [InlineData(6, 3)]
    [InlineData(6, 77)]
    [InlineData(6, 2024)]
    public void Puntaje_NuncaNegativo_NuncaSuperaElLargo_YEsMonotono(int cantidadJugadores, int semilla)
    {
        const int largo = 30;
        var e = Partido.Nueva(largo: largo, semilla: semilla, cantidadJugadores: cantidadJugadores);
        int pasos = 0;
        int anterior0 = 0, anterior1 = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos < 40000, "La partida no debería tardar tanto.");
            e = Partido.Aplicar(e, ElegirDeterminista(e, pasos));
            pasos++;

            int p0 = e.Contador.Puntos(new EquipoId(0));
            int p1 = e.Contador.Puntos(new EquipoId(1));

            Assert.True(p0 >= 0, "Los puntos del equipo 0 no pueden ser negativos.");
            Assert.True(p1 >= 0, "Los puntos del equipo 1 no pueden ser negativos.");
            Assert.True(p0 <= largo, "Los puntos del equipo 0 no pueden superar el largo del partido.");
            Assert.True(p1 <= largo, "Los puntos del equipo 1 no pueden superar el largo del partido.");
            Assert.True(p0 >= anterior0, "Los puntos del equipo 0 no pueden decrecer.");
            Assert.True(p1 >= anterior1, "Los puntos del equipo 1 no pueden decrecer.");

            anterior0 = p0;
            anterior1 = p1;
        }

        Assert.True(anterior0 >= largo ^ anterior1 >= largo, "Tiene que haber exactamente un equipo ganador.");
    }

    // A3: misma semilla + misma lista de acciones ⇒ EstadoPartida idéntico, campo por campo.
    // Corre la misma partida dos veces desde cero (con la misma estrategia determinista, que
    // por construcción produce la misma secuencia de acciones) y compara todo el estado en
    // cada paso, no sólo el final. Extiende PartidoTests.MismaSemilla_ReparteExactamenteIgual
    // (que sólo mira el reparto) a la partida completa y a los tres modos.
    [Theory]
    [InlineData(2, 5)]
    [InlineData(2, 42)]
    [InlineData(2, 12345)]
    [InlineData(4, 5)]
    [InlineData(4, 42)]
    [InlineData(4, 12345)]
    [InlineData(6, 5)]
    [InlineData(6, 42)]
    [InlineData(6, 12345)]
    public void MismaSemillaYMismasAcciones_DanElMismoEstado_CampoPorCampo_EnCadaPaso(
        int cantidadJugadores, int semilla)
    {
        var (estadosA, acciones) = JugarYRegistrarEstados(cantidadJugadores, semilla);

        // Segunda corrida, reproduciendo la misma secuencia de acciones ya grabada (no
        // recalculada): así se compara "misma semilla + misma lista de acciones", no dos
        // corridas de la misma estrategia que podrían divergir si algo no fuera determinista.
        var estadoB = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: cantidadJugadores);
        var estadosB = new List<EstadoPartida> { estadoB };
        foreach (var accion in acciones)
        {
            estadoB = Partido.Aplicar(estadoB, accion);
            estadosB.Add(estadoB);
        }

        Assert.Equal(estadosA.Count, estadosB.Count);
        for (int i = 0; i < estadosA.Count; i++)
            AssertEstadosIguales(estadosA[i], estadosB[i]);
    }

    private static (List<EstadoPartida> Estados, List<Accion> Acciones) JugarYRegistrarEstados(
        int cantidadJugadores, int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: cantidadJugadores);
        var estados = new List<EstadoPartida> { e };
        var acciones = new List<Accion>();
        int pasos = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos < 40000, "La partida no debería tardar tanto.");
            var elegida = ElegirDeterminista(e, pasos);
            acciones.Add(elegida);
            e = Partido.Aplicar(e, elegida);
            estados.Add(e);
            pasos++;
        }

        return (estados, acciones);
    }

    // Igual que el comparador de GrabacionFuzzTests: Assert.Equal(a, b) a secas no sirve
    // porque EstadoPartida guarda IReadOnlyList<...> (List<T>/arrays no tienen Equals de
    // valor) y Contador es una clase sin Equals de valor. Se compara campo por campo.
    private static void AssertEstadosIguales(EstadoPartida a, EstadoPartida b)
    {
        Assert.Equal(a.Contador.Largo, b.Contador.Largo);
        Assert.Equal(a.Contador.Puntos(new EquipoId(0)), b.Contador.Puntos(new EquipoId(0)));
        Assert.Equal(a.Contador.Puntos(new EquipoId(1)), b.Contador.Puntos(new EquipoId(1)));

        Assert.Equal(a.Semilla, b.Semilla);
        Assert.Equal(a.NumeroDeMano, b.NumeroDeMano);
        Assert.Equal(a.CantidadJugadores, b.CantidadJugadores);
        Assert.Equal(a.Repartidor, b.Repartidor);
        Assert.Equal(a.Muestra, b.Muestra);

        Assert.Equal(a.Manos.Count, b.Manos.Count);
        for (int j = 0; j < a.Manos.Count; j++)
            Assert.Equal(a.Manos[j], b.Manos[j]);
        Assert.Equal(a.ManosIniciales.Count, b.ManosIniciales.Count);
        for (int j = 0; j < a.ManosIniciales.Count; j++)
            Assert.Equal(a.ManosIniciales[j], b.ManosIniciales[j]);
        Assert.Equal(a.Activos, b.Activos);
        Assert.Equal(a.BazasGanadas, b.BazasGanadas);
        Assert.Equal(a.JugadasBaza, b.JugadasBaza);
        Assert.Equal(a.DenunciasPendientes, b.DenunciasPendientes);

        Assert.Equal(a.Fase, b.Fase);
        Assert.Equal(a.IndicePico, b.IndicePico);
        Assert.Equal(a.Abridor, b.Abridor);
        Assert.Equal(a.Turno, b.Turno);
        Assert.Equal(a.Truco, b.Truco);
        Assert.Equal(a.TrucoPendiente, b.TrucoPendiente);
        Assert.Equal(a.EquipoResponde, b.EquipoResponde);
        Assert.Equal(a.EquipoQuePuedeRevirar, b.EquipoQuePuedeRevirar);
        Assert.Equal(a.EnvidoPendiente, b.EnvidoPendiente);
        Assert.Equal(a.EnvidoJugado, b.EnvidoJugado);
        Assert.Equal(a.FlorResuelta, b.FlorResuelta);
        Assert.Equal(a.CobroFlor, b.CobroFlor);
        Assert.Equal(a.CobroEnvido, b.CobroEnvido);
        Assert.Equal(a.FlorPendiente, b.FlorPendiente);
        Assert.Equal(a.Cierre, b.Cierre);
    }
}
