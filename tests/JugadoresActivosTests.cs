using Domain;

namespace Tests;

// Paso 17b-1 — Jugadores activos: en una mano donde sólo juega un subconjunto (la pareja
// de un pico a pico dentro de la mesa de 6), la baza se resuelve entre esos jugadores y el
// turno cicla sólo entre ellos. Es la base del pico a pico.
public class JugadoresActivosTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J3 = new(3);
    private static readonly EquipoId E0 = new(0);

    private static Carta C(int n, Palo p) => new(n, p);

    [Fact]
    public void ConDosActivos_LaBazaSeResuelveEntreEsosDos_YElTurnoCiclaEntreEllos()
    {
        var e = EstadoConPico();

        // J0 (activo) tira; el turno pasa a J3 (el otro activo), la baza todavía no cierra.
        var e1 = Partido.Aplicar(e, new TirarCarta(J0, C(1, Palo.Espada)));
        Assert.Equal(J3, e1.Turno);
        Assert.Empty(e1.BazasGanadas);

        // J3 tira: la baza cierra con sólo dos cartas (no espera a los seis).
        var e2 = Partido.Aplicar(e1, new TirarCarta(J3, C(4, Palo.Copa)));
        Assert.Single(e2.BazasGanadas);
        Assert.Equal(E0, e2.BazasGanadas[0].Equipo); // J0 ganó con la 1 de Espada
        Assert.Equal(J0, e2.Turno);                   // abre la siguiente el ganador
    }

    [Fact]
    public void LosJugadoresNoActivos_NoTienenAcciones()
    {
        var e = EstadoConPico();
        Assert.Empty(Partido.AccionesLegales(e, new JugadorId(1))); // J1 no juega este pico
        Assert.Empty(Partido.AccionesLegales(e, new JugadorId(4)));
    }

    private static EstadoPartida EstadoConPico()
    {
        var dummy = new[] { C(6, Palo.Oro), C(5, Palo.Copa), C(3, Palo.Espada) };
        var manos = new IReadOnlyList<Carta>[]
        {
            new[] { C(1, Palo.Espada), C(6, Palo.Oro), C(5, Palo.Oro) }, // J0 (activo)
            dummy,                                                       // J1
            dummy,                                                       // J2
            new[] { C(4, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) }, // J3 (activo)
            dummy,                                                       // J4
            dummy,                                                       // J5
        };
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 6,
            Repartidor = new JugadorId(5),
            Muestra = new Muestra(C(6, Palo.Basto)),
            Manos = manos,
            ManosIniciales = manos,
            Activos = new[] { J0, J3 }, // el pico J0 vs J3
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = J0,
            Turno = J0,
        };
    }
}
