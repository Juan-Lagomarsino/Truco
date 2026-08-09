using Domain;

namespace Tests;

// Paso 15 — Irse al mazo (1v1). RULES_Afinadas.md §"Irse al mazo": el que se va entrega
// los puntos en juego (1 si no se gritó nada, o el valor del último truco querido) y la
// mano termina. No se puede ir dejando un canto sin resolver. El mano puede irse antes de
// tirar (A5). Es acción del equipo (B7); en 1v1 el equipo es el jugador.
public class IrseAlMazoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    private static readonly Carta[] Mano0 = { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Espada) };
    private static readonly Carta[] Mano1 = { C(6, Palo.Copa), C(7, Palo.Oro), C(4, Palo.Basto) };

    [Fact]
    public void IrseAlMazo_SinCantos_ElRivalSeLlevaUno_YSigueLaSiguienteMano()
    {
        var e = Estado(repartidor: J0); // Turno = J1 (mano)
        var e1 = Partido.Aplicar(e, new IrseAlMazo(J1));

        Assert.Equal(1, e1.Contador.Puntos(E0)); // el rival del que se fue
        Assert.Equal(0, e1.Contador.Puntos(E1));
        Assert.Equal(1, e1.NumeroDeMano);
    }

    [Fact]
    public void IrseAlMazo_AntesDeTirar_EstaPermitido()
    {
        var e = Estado(repartidor: J0); // Turno = J1, nadie tiró aún
        Assert.Contains(Partido.AccionesLegales(e, J1), a => a is IrseAlMazo);
    }

    [Fact]
    public void IrseAlMazo_ConTrucoQuerido_ElRivalSeLlevaLoQueValia()
    {
        var e = Estado(repartidor: J0); // Turno J1
        var e1 = Partido.Aplicar(e, new CantarTruco(J1));
        var e2 = Partido.Aplicar(e1, new Quiero(J0)); // mano vale 2, Turno vuelve a J1
        var e3 = Partido.Aplicar(e2, new IrseAlMazo(J1));

        Assert.Equal(2, e3.Contador.Puntos(E0));
    }

    [Fact]
    public void NoSePuedeIrAlMazo_ConUnCantoPendiente()
    {
        var e = Estado(repartidor: J0);
        var e1 = Partido.Aplicar(e, new CantarTruco(J1)); // truco pendiente
        // El que debe responder no puede irse dejando el canto sin resolver.
        Assert.DoesNotContain(Partido.AccionesLegales(e1, J0), a => a is IrseAlMazo);
        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e1, new IrseAlMazo(J0)));
    }

    private static EstadoPartida Estado(JugadorId repartidor)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 2);
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 2,
            Repartidor = repartidor,
            Muestra = new Muestra(C(6, Palo.Basto)),
            Manos = new[] { Mano0, Mano1 },
            ManosIniciales = new[] { Mano0, Mano1 },
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
