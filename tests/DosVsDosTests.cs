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

    private static EstadoPartida Estado(
        IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1,
        IReadOnlyList<Carta> mano2, IReadOnlyList<Carta> mano3, JugadorId repartidor)
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
            Muestra = MuestraNeutra,
            Manos = manos,
            ManosIniciales = manos,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
