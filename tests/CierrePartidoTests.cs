using Domain;

namespace Tests;

// B6 (PREGUNTAS_ABIERTAS.md): orden de acreditación al cerrar la mano es flor → envido →
// truco, y el partido termina apenas un equipo llega al objetivo. Si los tantos de flor
// solos ya cruzan la meta, lo que venga después (acá, el truco) no se llega a acreditar.
// Flor y envido nunca coexisten en la misma mano (F1: cantar flor anula el envido), así
// que el caso observable es "flor sola vs. lo que sigue" — acá, el truco.
public class CierrePartidoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto));

    // Tres del mismo palo (flor real, 20+7+6+5=38); de paso el 7 de Espada es mata, así que
    // J0 también gana las dos bazas con estas mismas cartas.
    private static readonly Carta[] FlorEspada = { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) };
    private static readonly Carta[] SinFlorDebil = { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Basto) };

    // Partido a 10 (par, para poder tener un largo chico): el equipo 0 ya tiene 7. Su flor
    // (3 puntos) lo lleva justo a 10 = el largo. El truco liso que ganaría después (1 punto,
    // porque J0 también gana la mano) NUNCA se acredita porque el partido ya terminó con la flor.
    [Fact]
    public void SiLaFlorSolaLlegaAlLargo_ElPartidoTermina_YElTrucoNoSeAcredita()
    {
        var contador = new Contador(10).Sumar(E0, 7);
        var e = Estado(FlorEspada, SinFlorDebil, repartidor: J1, contador: contador); // mano = J0

        var e1 = Partido.Aplicar(e, new CantarFlor(J0)); // J0 tiene flor, J1 no: cobra 3 directo
        Assert.Equal(new Cobro(E0, 3), e1.CobroFlor);
        Assert.Equal(7, e1.Contador.Puntos(E0)); // resuelto, pero todavía no acreditado (F4)

        // Se juega la mano igual: J0 gana las dos bazas (mata 7 de Espada, después 6 de Espada).
        var e2 = Partido.Aplicar(e1, new TirarCarta(J0, C(7, Palo.Espada)));
        var e3 = Partido.Aplicar(e2, new TirarCarta(J1, C(4, Palo.Copa)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(6, Palo.Espada)));
        var e5 = Partido.Aplicar(e4, new TirarCarta(J1, C(5, Palo.Oro)));

        // Al cerrar: la flor sola (7+3=10) ya termina el partido. El punto de la mano
        // (que J0 también ganó) nunca se suma: el contador queda exactamente en 10, no 11.
        Assert.True(e5.Terminado);
        Assert.Equal(10, e5.Contador.Puntos(E0));
        Assert.Equal(E0, e5.Contador.Ganador);
        Assert.Equal(0, e5.Contador.Puntos(E1));
    }

    private static EstadoPartida Estado(
        IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1, JugadorId repartidor, Contador contador)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 2);
        return new EstadoPartida
        {
            Contador = contador,
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 2,
            Repartidor = repartidor,
            Muestra = MuestraNeutra,
            Manos = new[] { mano0, mano1 },
            ManosIniciales = new[] { mano0, mano1 },
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
