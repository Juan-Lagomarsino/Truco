using Domain;

namespace Tests;

// Paso 17 (parte 1) — El reductor jugando un 3v3 (redondilla) de a 6. Equipos
// intercalados: jugadores 0, 2, 4 son el equipo 0; 1, 3, 5 el equipo 1.
public class TresVsTresTests
{
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    [Fact]
    public void NuevaPartida_ConSeisJugadores_ReparteTresACadaUno()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7, cantidadJugadores: 6);

        Assert.Equal(6, e.Manos.Count);
        Assert.All(e.Manos, m => Assert.Equal(3, m.Count));
        Assert.Equal(new JugadorId(1), e.JugadorMano); // repartidor J0 por defecto → mano J1
    }

    // Trillera: tres flores del mismo equipo cobran 3 cada una (9).
    [Fact]
    public void Trillera_TresFloresDelMismoEquipo_Cobran9()
    {
        var muestra = new Muestra(C(6, Palo.Basto)); // sin piezas en estas manos
        var manos = new IReadOnlyList<Carta>[]
        {
            new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }, // J0 (E0) flor
            new[] { C(1, Palo.Espada), C(3, Palo.Oro), C(7, Palo.Basto) },     // J1 (E1)
            new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(5, Palo.Oro) },          // J2 (E0) flor
            new[] { C(1, Palo.Basto), C(3, Palo.Espada), C(3, Palo.Copa) },    // J3 (E1)
            new[] { C(7, Palo.Copa), C(6, Palo.Copa), C(5, Palo.Copa) },       // J4 (E0) flor
            new[] { C(2, Palo.Espada), C(4, Palo.Espada), C(1, Palo.Copa) },   // J5 (E1)
        };
        var e = Estado6(manos, muestra, repartidor: new JugadorId(5)); // mano = J0

        var e1 = Partido.Aplicar(e, new CantarFlor(new JugadorId(0)));

        Assert.Equal(new Cobro(E0, 9), e1.CobroFlor); // tres flores del equipo 0
    }

    [Theory]
    [InlineData(5)]
    [InlineData(321)]
    public void UnaPartida3v3Completa_TerminaConUnGanador(int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: 6);
        int pasos = 0, puntos0 = 0, puntos1 = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 60000, "La partida no debería tardar tanto.");

            Accion? elegida = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(e, new JugadorId(j));
                if (legales.Count > 0) { elegida = legales[pasos % legales.Count]; break; }
            }
            Assert.NotNull(elegida);

            e = Partido.Aplicar(e, elegida!);

            int n0 = e.Contador.Puntos(E0), n1 = e.Contador.Puntos(E1);
            Assert.True(n0 >= puntos0 && n1 >= puntos1);
            puntos0 = n0; puntos1 = n1;
        }

        Assert.True(e.Contador.Puntos(E0) >= 30 ^ e.Contador.Puntos(E1) >= 30);
    }

    private static EstadoPartida Estado6(IReadOnlyList<IReadOnlyList<Carta>> manos, Muestra muestra, JugadorId repartidor)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 6);
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 6,
            Repartidor = repartidor,
            Muestra = muestra,
            Manos = manos,
            ManosIniciales = manos,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
