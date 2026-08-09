using Domain;

namespace Tests;

// Paso 12 — Truco / Retruco / Vale Cuatro en el reductor (1v1).
// RULES_Afinadas.md §"El grite de Truco/Retruco/ValeCuatro": se canta en tu turno, el
// rival dice quiero o no quiero; si no quiere, el que cantó se lleva el valor del
// último canto querido y termina la mano. Sólo revira el equipo que quiso el canto
// anterior; nunca se revira el propio canto.
public class TrucoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    [Fact]
    public void AlCantarTruco_ElRivalDebeResponderQuieroONoQuiero()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7); // Turno = J1 (mano)
        var e1 = Partido.Aplicar(e, new CantarTruco(J1));

        // El que responde es el rival (J0); el que cantó no puede hacer nada.
        var accionesRival = Partido.AccionesLegales(e1, J0);
        Assert.Contains(accionesRival, a => a is Quiero);
        Assert.Contains(accionesRival, a => a is NoQuiero);
        Assert.Empty(Partido.AccionesLegales(e1, J1));
    }

    [Fact]
    public void MientrasHayCantoPendiente_NoSePuedeTirarCarta()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var e1 = Partido.Aplicar(e, new CantarTruco(J1));
        var carta = e1.Manos[J0.Valor][0];

        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e1, new TirarCarta(J0, carta)));
    }

    [Fact]
    public void TrucoNoQuerido_ElQueCantoSeLlevaUnPunto_YTerminaLaMano()
    {
        var e = EstadoConManos(
            new Muestra(C(6, Palo.Basto)),
            mano0: new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(4, Palo.Oro) },
            mano1: new[] { C(4, Palo.Copa), C(5, Palo.Copa), C(6, Palo.Copa) },
            repartidor: J0); // mano = J1, Turno = J1

        var e1 = Partido.Aplicar(e, new CantarTruco(J1)); // canta el equipo 1
        var e2 = Partido.Aplicar(e1, new NoQuiero(J0));

        Assert.Equal(1, e2.Contador.Puntos(E1)); // el que canto (equipo 1) se lleva 1
        Assert.Equal(1, e2.NumeroDeMano);         // se repartio la mano siguiente
    }

    [Fact]
    public void TrucoQuerido_LaManoValeDos()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var e1 = Partido.Aplicar(e, new CantarTruco(e.Turno));
        var respondedor = e.Turno.Equals(J0) ? J1 : J0;
        var e2 = Partido.Aplicar(e1, new Quiero(respondedor));

        Assert.Equal(NivelTruco.Truco, e2.Truco);
        Assert.Null(e2.TrucoPendiente);
    }

    [Fact]
    public void SoloRevira_ElEquipoQueQuisoElCantoAnterior()
    {
        // J1 canta truco, J0 quiere: ahora sólo el equipo de J0 puede revirar (retruco).
        var e = EstadoConManos(
            new Muestra(C(6, Palo.Basto)),
            mano0: new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(4, Palo.Oro) },
            mano1: new[] { C(4, Palo.Copa), C(5, Palo.Copa), C(6, Palo.Copa) },
            repartidor: J0); // Turno = J1

        var e1 = Partido.Aplicar(e, new CantarTruco(J1));
        var e2 = Partido.Aplicar(e1, new Quiero(J0)); // Turno vuelve a J1

        // J1 (que cantó el truco) no puede revirar su propio canto.
        Assert.DoesNotContain(Partido.AccionesLegales(e2, J1), a => a is CantarTruco);

        // J1 tira, y en el turno de J0 (que quiso) sí aparece el retruco.
        var e3 = Partido.Aplicar(e2, new TirarCarta(J1, C(4, Palo.Copa)));
        Assert.Contains(Partido.AccionesLegales(e3, J0), a => a is CantarTruco);
    }

    [Fact]
    public void SecuenciaTrucoRetrucoValeCuatro_LaManoValeCuatro()
    {
        // J1 tiene la 1 de Espada (gana la baza 1), lo que devuelve el turno a J1 para
        // el vale cuatro. Cada revira lo hace el equipo que quiso el canto anterior.
        var e = EstadoConManos(
            new Muestra(C(6, Palo.Basto)),
            mano0: new[] { C(4, Palo.Oro), C(5, Palo.Oro), C(6, Palo.Oro) },
            mano1: new[] { C(1, Palo.Espada), C(4, Palo.Copa), C(6, Palo.Copa) },
            repartidor: J0); // Turno = J1

        var e1 = Partido.Aplicar(e, new CantarTruco(J1));                  // J1: truco
        var e2 = Partido.Aplicar(e1, new Quiero(J0));                      // J0 quiere (revira eq. 0)
        var e3 = Partido.Aplicar(e2, new TirarCarta(J1, C(1, Palo.Espada))); // J1 gana la baza 1
        var e4 = Partido.Aplicar(e3, new CantarTruco(J0));                 // J0: retruco
        var e5 = Partido.Aplicar(e4, new Quiero(J1));                      // J1 quiere (revira eq. 1)
        var e6 = Partido.Aplicar(e5, new TirarCarta(J0, C(4, Palo.Oro)));  // cierra baza 1, gana J1, turno J1
        var e7 = Partido.Aplicar(e6, new CantarTruco(J1));                 // J1: vale cuatro
        var e8 = Partido.Aplicar(e7, new Quiero(J0));

        Assert.Equal(NivelTruco.ValeCuatro, e8.Truco);
    }

    [Fact]
    public void CantarTrucoFueraDeTurno_Lanza()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7); // Turno = J1
        var fueraDeTurno = e.Turno.Equals(J0) ? J1 : J0;

        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e, new CantarTruco(fueraDeTurno)));
    }

    [Fact]
    public void TrucoQuerido_YGanadoPorCartas_Suma2()
    {
        var e = EstadoConManos(
            new Muestra(C(6, Palo.Basto)),
            mano0: new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(4, Palo.Oro) }, // J0 gana las dos
            mano1: new[] { C(4, Palo.Copa), C(5, Palo.Copa), C(6, Palo.Copa) },
            repartidor: J0); // Turno = J1

        var e1 = Partido.Aplicar(e, new CantarTruco(J1));  // truco
        var e2 = Partido.Aplicar(e1, new Quiero(J0));      // querido: vale 2
        // Baza 1: J1 abre, J0 gana con la 1 de Espada.
        var e3 = Partido.Aplicar(e2, new TirarCarta(J1, C(4, Palo.Copa)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(1, Palo.Espada)));
        // Baza 2: J0 abre y gana con la 1 de Basto.
        var e5 = Partido.Aplicar(e4, new TirarCarta(J0, C(1, Palo.Basto)));
        var e6 = Partido.Aplicar(e5, new TirarCarta(J1, C(5, Palo.Copa)));

        Assert.Equal(2, e6.Contador.Puntos(E0));
    }

    // Simulación con cantos: en cada paso, quien tenga acciones (el en turno o el que
    // responde un canto) elige una rotando el índice. Debe terminar, nunca dejar a nadie
    // sin poder jugar, y dar exactamente un ganador.
    [Theory]
    [InlineData(3)]
    [InlineData(77)]
    [InlineData(555)]
    public void UnaPartidaConCantos_Termina_SinDeadlockYConUnGanador(int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla);
        int pasos = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 20000, "La partida no debería tardar tanto.");

            Accion? elegida = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(e, new JugadorId(j));
                if (legales.Count > 0) { elegida = legales[pasos % legales.Count]; break; }
            }

            Assert.NotNull(elegida); // nunca hay deadlock: siempre alguien puede jugar
            e = Partido.Aplicar(e, elegida!);
        }

        bool gano0 = e.Contador.Puntos(E0) >= 30;
        bool gano1 = e.Contador.Puntos(E1) >= 30;
        Assert.True(gano0 ^ gano1);
    }

    private static EstadoPartida EstadoConManos(
        Muestra muestra, IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1, JugadorId repartidor)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 2);
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 2,
            Repartidor = repartidor,
            Muestra = muestra,
            Manos = new IReadOnlyList<Carta>[] { mano0, mano1 },
            ManosIniciales = new IReadOnlyList<Carta>[] { mano0, mano1 },
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
