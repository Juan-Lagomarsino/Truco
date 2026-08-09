using Domain;

namespace Tests;

// Paso 14b — Bids de flor (1v1): Con Flor Envido y Contra Flor al Resto (F2, A3).
// Con Flor Envido: querido 5 a la flor más alta, no querido 3 al que cantó.
// Contra Flor al Resto: querido = falta del que va ganando + los puntos de las flores en
// juego; no querido 3. Si el rival no tiene flor, no hay enfrentamiento: cobra 3. Son
// independientes (C3): no se encadenan.
public class FlorBidTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto));

    private static readonly Carta[] Flor38Espada = { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) };
    private static readonly Carta[] Flor37Oro = { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) };
    private static readonly Carta[] SinFlor = { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Espada) };

    [Fact]
    public void ConFlorEnvido_Querido_LaFlorMasAltaCobraCinco()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1); // Turno J0, J0 38 > J1 37
        var e1 = Partido.Aplicar(e, new CantarFlorEnvido(J0));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(5, e2.Contador.Puntos(E0));
    }

    [Fact]
    public void ConFlorEnvido_NoQuerido_ElCantorCobraTres()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarFlorEnvido(J0));
        var e2 = Partido.Aplicar(e1, new NoQuiero(J1));

        Assert.Equal(3, e2.Contador.Puntos(E0));
    }

    [Fact]
    public void ConFlorEnvido_RivalSinFlor_CobraTres()
    {
        var e = Estado(Flor38Espada, SinFlor, repartidor: J1); // J1 sin flor
        var e1 = Partido.Aplicar(e, new CantarFlorEnvido(J0));

        Assert.Equal(3, e1.Contador.Puntos(E0)); // no hay enfrentamiento: cobra la flor
        Assert.True(e1.FlorResuelta);
    }

    [Fact]
    public void ContraFlorAlResto_Querido_ValeLaFaltaMasLasFlores()
    {
        // Partido a 30 (mitad 15), ambos en 0 → falta 15; dos flores → 15 + 3*2 = 21.
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarContraFlorAlResto(J0));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(21, e2.Contador.Puntos(E0));
    }

    [Fact]
    public void ContraFlorAlResto_NoQuerido_ElCantorCobraTres()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarContraFlorAlResto(J0));
        var e2 = Partido.Aplicar(e1, new NoQuiero(J1));

        Assert.Equal(3, e2.Contador.Puntos(E0));
    }

    [Fact]
    public void ConUnBidPendiente_ResponddeSoloElRival()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarFlorEnvido(J0));

        Assert.Contains(Partido.AccionesLegales(e1, J1), a => a is Quiero);
        Assert.Empty(Partido.AccionesLegales(e1, J0));
    }

    private static EstadoPartida Estado(
        IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1, JugadorId repartidor)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 2);
        return new EstadoPartida
        {
            Contador = new Contador(30),
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
