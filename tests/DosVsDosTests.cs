using Domain;

namespace Tests;

// Paso 16b — El reductor jugando un 2v2 de cartas. Equipos intercalados: jugadores 0 y 2
// son el equipo 0; 1 y 3 el equipo 1. RULES_Afinadas.md §"Como se resuelve la mano".
public class DosVsDosTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly JugadorId J2 = new(2);
    private static readonly JugadorId J3 = new(3);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Copa)); // no pieza

    [Fact]
    public void NuevaPartida_ConCuatroJugadores_ReparteTresACadaUno()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7, cantidadJugadores: 4);

        Assert.Equal(4, e.Manos.Count);
        Assert.All(e.Manos, m => Assert.Equal(3, m.Count));
        Assert.Equal(J1, e.JugadorMano); // repartidor J0 por defecto → mano J1
    }

    [Fact]
    public void UnaBazaGanadaPorDosDelMismoEquipo_LaGanaEseEquipo()
    {
        // Abridor J1 (mano). Orden antihorario: J1, J2, J3, J0.
        // J0 y J2 (equipo 0) tiran 3 (mismo nivel), los del equipo 1 tiran más bajo.
        var e = Estado(
            mano0: new[] { C(3, Palo.Copa), C(6, Palo.Basto), C(7, Palo.Oro) },
            mano1: new[] { C(4, Palo.Basto), C(5, Palo.Espada), C(6, Palo.Oro) },
            mano2: new[] { C(3, Palo.Oro), C(6, Palo.Espada), C(7, Palo.Basto) },
            mano3: new[] { C(5, Palo.Basto), C(4, Palo.Espada), C(6, Palo.Espada) },
            repartidor: J0);

        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(4, Palo.Basto)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J2, C(3, Palo.Oro)));
        var e3 = Partido.Aplicar(e2, new TirarCarta(J3, C(5, Palo.Basto)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(3, Palo.Copa)));

        Assert.Single(e4.BazasGanadas);
        Assert.False(e4.BazasGanadas[0].EsParda);
        Assert.Equal(E0, e4.BazasGanadas[0].Equipo); // dos 3 del equipo 0 arriba → gana el 0
    }

    [Fact]
    public void ElEquipoGanaLaMano_CuandoGanaDosBazas()
    {
        // El equipo 0 (J0, J2) tiene las matas y gana las dos bazas.
        var e = Estado(
            mano0: new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(3, Palo.Oro) }, // dos matas
            mano1: new[] { C(4, Palo.Oro), C(5, Palo.Espada), C(6, Palo.Oro) },
            mano2: new[] { C(6, Palo.Espada), C(4, Palo.Basto), C(3, Palo.Basto) },
            mano3: new[] { C(5, Palo.Basto), C(4, Palo.Espada), C(6, Palo.Basto) },
            repartidor: J0); // mano J1 abre

        // Baza 1: J1, J2, J3 tiran; J0 gana con la 1 de Espada.
        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(4, Palo.Oro)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J2, C(6, Palo.Espada)));
        var e3 = Partido.Aplicar(e2, new TirarCarta(J3, C(5, Palo.Basto)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(1, Palo.Espada)));
        // Baza 2: abre J0 (ganó) y gana con la 1 de Basto.
        var e5 = Partido.Aplicar(e4, new TirarCarta(J0, C(1, Palo.Basto)));
        var e6 = Partido.Aplicar(e5, new TirarCarta(J1, C(5, Palo.Espada)));
        var e7 = Partido.Aplicar(e6, new TirarCarta(J2, C(4, Palo.Basto)));
        var e8 = Partido.Aplicar(e7, new TirarCarta(J3, C(4, Palo.Espada)));

        Assert.Equal(1, e8.Contador.Puntos(E0)); // el equipo 0 ganó la mano (truco liso)
        Assert.Equal(1, e8.NumeroDeMano);         // se repartió la siguiente
    }

    // 16c: el envido lo juega el mejor de cada equipo.
    [Fact]
    public void ElEnvido_LoJuegaElMejorDeCadaEquipo()
    {
        // Muestra 6 de Basto (sin piezas en estas manos). El equipo 0 tiene el 31 de J0.
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = Estado(
            mano0: new[] { C(6, Palo.Oro), C(5, Palo.Oro), C(2, Palo.Copa) },   // envido 31
            mano1: new[] { C(7, Palo.Espada), C(4, Palo.Oro), C(3, Palo.Copa) }, // envido 7
            mano2: new[] { C(4, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) }, // envido 5
            mano3: new[] { C(6, Palo.Copa), C(4, Palo.Espada), C(3, Palo.Oro) }, // envido 6
            repartidor: J0, muestra: muestra);

        var e1 = Partido.Aplicar(e, new CantarEnvido(J1, EnvidoCanto.Envido)); // canta el mano (equipo 1)
        var e2 = Partido.Aplicar(e1, new Quiero(J0)); // responde el equipo 0

        Assert.Equal(new Cobro(E0, 2), e2.CobroEnvido); // el 31 del equipo 0 le gana al 7 del 1
    }

    // 16c: collera — dos flores del mismo equipo cobran 3 cada una (6).
    [Fact]
    public void Collera_DosFloresDelMismoEquipo_Cobran6()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = Estado(
            mano0: new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }, // flor E0
            mano1: new[] { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Basto) },        // sin flor
            mano2: new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) },           // flor E0
            mano3: new[] { C(4, Palo.Espada), C(5, Palo.Copa), C(3, Palo.Oro) },       // sin flor
            repartidor: J3, muestra: muestra); // mano = J0

        var e1 = Partido.Aplicar(e, new CantarFlor(J0));

        Assert.Equal(new Cobro(E0, 6), e1.CobroFlor); // dos flores del equipo 0
    }

    // 16c: enfrentamiento — gana el equipo de la flor más alta y cobra por sus flores.
    [Fact]
    public void EnfrentamientoDeFlores_GanaElEquipoDeLaMasAlta_ConCollera()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = Estado(
            mano0: new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }, // flor 38 (E0)
            mano1: new[] { C(7, Palo.Copa), C(6, Palo.Copa), C(4, Palo.Copa) },        // flor 37 (E1)
            mano2: new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) },           // flor 37 (E0)
            mano3: new[] { C(5, Palo.Oro), C(4, Palo.Espada), C(3, Palo.Basto) },      // sin flor
            repartidor: J3, muestra: muestra); // mano = J0

        var e1 = Partido.Aplicar(e, new CantarFlor(J0));

        // El equipo 0 tiene la flor más alta (38) y dos flores → 6; el equipo 1 no cobra.
        Assert.Equal(new Cobro(E0, 6), e1.CobroFlor);
    }

    // G1: si dos del mismo equipo empatan arriba, la baza la abre el que tiró primero
    // (el más cercano al que abrió), no cualquiera de los dos.
    [Fact]
    public void EmpateArribaDelMismoEquipo_AbreLaSiguienteElQueTiroPrimero()
    {
        // Mismo escenario que UnaBazaGanadaPorDosDelMismoEquipo_LaGanaEseEquipo: J2 y J0
        // (equipo 0) empatan con un 3 arriba de todo. En el orden de juego J1,J2,J3,J0,
        // J2 tira su 3 antes que J0: tiene que abrir la baza siguiente J2, no J0.
        var e = Estado(
            mano0: new[] { C(3, Palo.Copa), C(6, Palo.Basto), C(7, Palo.Oro) },
            mano1: new[] { C(4, Palo.Basto), C(5, Palo.Espada), C(6, Palo.Oro) },
            mano2: new[] { C(3, Palo.Oro), C(6, Palo.Espada), C(7, Palo.Basto) },
            mano3: new[] { C(5, Palo.Basto), C(4, Palo.Espada), C(6, Palo.Espada) },
            repartidor: J0);

        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(4, Palo.Basto)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J2, C(3, Palo.Oro))); // primero de los empatados
        var e3 = Partido.Aplicar(e2, new TirarCarta(J3, C(5, Palo.Basto)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(3, Palo.Copa))); // segundo de los empatados

        Assert.Equal(J2, e4.Abridor);
        Assert.Equal(J2, e4.Turno);
    }

    // B9: cualquier jugador del equipo rival puede responder un canto — no hace falta que
    // sea "el pie" (el último del equipo en recibir cartas). Acá responde J1, no J3.
    [Fact]
    public void CualquieraDelEquipoRivalPuedeResponderElEnvido_NoSoloElPie()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = Estado(
            mano0: new[] { C(6, Palo.Oro), C(5, Palo.Oro), C(2, Palo.Copa) },   // envido 31
            mano1: new[] { C(7, Palo.Espada), C(4, Palo.Oro), C(3, Palo.Copa) },
            mano2: new[] { C(4, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) },
            mano3: new[] { C(6, Palo.Copa), C(4, Palo.Espada), C(3, Palo.Oro) },
            repartidor: J3, muestra: muestra); // mano = J0, Turno J0

        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));

        // Los dos jugadores del equipo rival (J1 y J3) pueden responder, no sólo el pie (J3).
        Assert.Contains(Partido.AccionesLegales(e1, J1), a => a is Quiero);
        Assert.Contains(Partido.AccionesLegales(e1, J3), a => a is Quiero);

        // Responde J1 (no el pie): compromete a todo el equipo.
        var e2 = Partido.Aplicar(e1, new Quiero(J1));
        Assert.NotNull(e2.CobroEnvido);
        Assert.Empty(Partido.AccionesLegales(e2, J3)); // ya se resolvió, J3 no tiene nada que hacer
    }

    // B9, para el truco: mismo criterio, cualquiera del equipo responde.
    [Fact]
    public void CualquieraDelEquipoRivalPuedeResponderElTruco_NoSoloElPie()
    {
        // Muestra de Basto (no Copa): así ninguna carta de estas manos es pieza ni forma
        // flor, y no se abre una ventana de denuncia de flor al terminar la mano.
        var e = Estado(
            mano0: new[] { C(6, Palo.Oro), C(5, Palo.Oro), C(2, Palo.Copa) },
            mano1: new[] { C(7, Palo.Espada), C(4, Palo.Oro), C(3, Palo.Copa) },
            mano2: new[] { C(4, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) },
            mano3: new[] { C(6, Palo.Copa), C(4, Palo.Espada), C(3, Palo.Oro) },
            repartidor: J3, muestra: new Muestra(C(6, Palo.Basto))); // mano = J0, Turno J0

        var e1 = Partido.Aplicar(e, new CantarTruco(J0)); // responde el equipo 1 (J1, J3)

        Assert.Contains(Partido.AccionesLegales(e1, J1), a => a is Quiero);
        Assert.Contains(Partido.AccionesLegales(e1, J3), a => a is Quiero);

        var e2 = Partido.Aplicar(e1, new Quiero(J1)); // responde J1, no el pie J3
        Assert.Equal(NivelTruco.Truco, e2.Truco);
        Assert.Empty(Partido.AccionesLegales(e2, J3));
    }

    // B9, para un bid de flor: cualquiera del equipo rival responde, incluso un jugador que
    // no tiene flor él mismo — el bid es del equipo, no del que puso la flor en la mesa.
    [Fact]
    public void CualquieraDelEquipoRivalPuedeResponderUnBidDeFlor_AunSinTenerFlorElMismo()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = Estado(
            mano0: new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }, // flor 38 (E0)
            mano1: new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) },           // flor 37 (E1) — J1 tiene flor
            mano2: new[] { C(4, Palo.Copa), C(5, Palo.Copa), C(3, Palo.Basto) },       // sin flor
            mano3: new[] { C(4, Palo.Espada), C(5, Palo.Basto), C(3, Palo.Oro) },      // J3 sin flor
            repartidor: J3, muestra: muestra); // mano = J0

        var e1 = Partido.Aplicar(e, new CantarFlorEnvido(J0));
        Assert.NotNull(e1.FlorPendiente); // E1 sí tiene flor (J1): hay bid pendiente, no cobro directo

        // J3, que no tiene flor él mismo, igual puede responder por su equipo.
        Assert.DoesNotContain(Partido.AccionesLegales(e1, J1), a => a is CantarFlor); // ya no hay ventana de cantar
        Assert.Contains(Partido.AccionesLegales(e1, J3), a => a is Quiero);
        var e2 = Partido.Aplicar(e1, new Quiero(J3));

        Assert.Equal(new Cobro(E0, 5), e2.CobroFlor); // la flor más alta (E0, 38) cobra 5
    }

    // B7: irse al mazo es del equipo. Si J1 se va, entrega lo que valía la mano y la mano
    // termina para los cuatro, aunque su compañero J3 no haya hecho nada.
    [Fact]
    public void IrseAlMazo_EsDelEquipo_ElRivalCobraYLaManoTermina()
    {
        // Muestra de Basto: sin piezas ni flor escondida, para que TerminarMano cierre
        // directo (sin ventana de denuncia) y se pueda comparar el puntaje sin más ruido.
        var e = Estado(
            mano0: new[] { C(6, Palo.Oro), C(5, Palo.Oro), C(2, Palo.Copa) },
            mano1: new[] { C(7, Palo.Espada), C(4, Palo.Oro), C(3, Palo.Copa) },
            mano2: new[] { C(4, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) },
            mano3: new[] { C(6, Palo.Copa), C(4, Palo.Espada), C(3, Palo.Oro) },
            repartidor: J3, muestra: new Muestra(C(6, Palo.Basto))); // mano = J0, Turno J0

        var e1 = Partido.Aplicar(e, new IrseAlMazo(J0)); // se va J0 (equipo 0)

        Assert.Equal(1, e1.Contador.Puntos(E1)); // el equipo rival cobra
        Assert.Equal(0, e1.Contador.Puntos(E0));
        Assert.Equal(1, e1.NumeroDeMano); // la mano terminó y se repartió la siguiente
    }

    // 16d: una partida 2v2 completa con todos los cantos termina, sin deadlock y con un
    // solo equipo ganador, y los puntos nunca decrecen.
    [Theory]
    [InlineData(3)]
    [InlineData(77)]
    [InlineData(2024)]
    public void UnaPartida2v2Completa_TerminaConUnGanador(int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: 4);
        int pasos = 0, puntos0 = 0, puntos1 = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 40000, "La partida no debería tardar tanto.");

            Accion? elegida = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(e, new JugadorId(j));
                if (legales.Count > 0) { elegida = legales[pasos % legales.Count]; break; }
            }
            Assert.NotNull(elegida); // nunca hay deadlock

            e = Partido.Aplicar(e, elegida!);

            int n0 = e.Contador.Puntos(E0), n1 = e.Contador.Puntos(E1);
            Assert.True(n0 >= puntos0 && n1 >= puntos1, "Los puntos no pueden decrecer.");
            puntos0 = n0; puntos1 = n1;
        }

        Assert.True(e.Contador.Puntos(E0) >= 30 ^ e.Contador.Puntos(E1) >= 30);
    }

    private static EstadoPartida Estado(
        IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1,
        IReadOnlyList<Carta> mano2, IReadOnlyList<Carta> mano3, JugadorId repartidor, Muestra? muestra = null)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 4);
        var manos = new[] { mano0, mano1, mano2, mano3 };
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 4,
            Repartidor = repartidor,
            Muestra = muestra ?? MuestraNeutra,
            Manos = manos,
            ManosIniciales = manos,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
