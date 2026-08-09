using Domain;

namespace Tests;

// Paso 14a — Cantar flor en el reductor (1v1), base: obligatoria para cobrar, anula el
// envido (F1), y por defecto la flor más alta cobra 3; empate gana el equipo mano (B8).
// Los bids (Con Flor Envido / Contra Flor al Resto) y la denuncia van en 14b/14c.
public class FlorCantoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto));

    // Flores (muestra 6 de Basto, ninguna carta es pieza):
    private static readonly Carta[] Flor38Espada = { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }; // 20+7+6+5
    private static readonly Carta[] Flor37Oro = { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) };             // 20+7+6+4
    private static readonly Carta[] Flor38Copa = { C(7, Palo.Copa), C(6, Palo.Copa), C(5, Palo.Copa) };         // 20+7+6+5
    private static readonly Carta[] SinFlor = { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Espada) };

    [Fact]
    public void UnSoloConFlor_CobraTres()
    {
        var e = Estado(Flor38Espada, SinFlor, repartidor: J1); // Turno = J0 (mano), J0 con flor
        var e1 = Partido.Aplicar(e, new CantarFlor(J0));

        Assert.Equal(new Cobro(E0, 3), e1.CobroFlor);
    }

    [Fact]
    public void DosConFlor_GanaLaMasAlta()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1); // J0 38 vs J1 37, Turno J0
        var e1 = Partido.Aplicar(e, new CantarFlor(J0));

        Assert.Equal(new Cobro(E0, 3), e1.CobroFlor);
    }

    [Fact]
    public void DosConFlorEmpatadas_GanaElEquipoMano()
    {
        var e = Estado(Flor38Espada, Flor38Copa, repartidor: J0); // ambos 38, mano = J1
        var e1 = Partido.Aplicar(e, new CantarFlor(J1));

        Assert.Equal(new Cobro(E1, 3), e1.CobroFlor); // empate → gana el mano (J1)
    }

    [Fact]
    public void CantarFlor_AnulaElEnvido()
    {
        var e = Estado(Flor38Espada, SinFlor, repartidor: J1); // Turno J0
        var e1 = Partido.Aplicar(e, new CantarFlor(J0));

        Assert.DoesNotContain(Partido.AccionesLegales(e1, e1.Turno), a => a is CantarEnvido);
    }

    [Fact]
    public void FlorCancelaUnEnvidoPendiente()
    {
        // J1 (mano, sin flor) toca envido; J0 (con flor) responde cantando flor: el envido
        // se cancela sin puntos y la flor cobra 3.
        var e = Estado(Flor38Espada, SinFlor, repartidor: J0); // Turno J1 (mano), J1 sin flor, J0 con flor
        var e1 = Partido.Aplicar(e, new CantarEnvido(J1, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new CantarFlor(J0));

        Assert.False(e2.HayEnvidoPendiente);
        Assert.Null(e2.CobroEnvido);                 // el envido no dio puntos
        Assert.Equal(new Cobro(E0, 3), e2.CobroFlor); // la flor sí
    }

    [Fact]
    public void SinFlor_NoSePuedeCantar()
    {
        var e = Estado(SinFlor, Flor38Espada, repartidor: J1); // Turno J0, J0 sin flor
        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e, new CantarFlor(J0)));
        Assert.DoesNotContain(Partido.AccionesLegales(e, J0), a => a is CantarFlor);
    }

    [Fact]
    public void LaFlorSoloSeCantaEnLaPrimeraBaza()
    {
        var e = Estado(Flor38Espada, Flor37Oro, repartidor: J1); // Turno J0
        // J0 juega su carta sin cantar flor; J1 juega la suya: primera baza cerrada.
        var e1 = Partido.Aplicar(e, new TirarCarta(J0, C(7, Palo.Espada)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J1, C(7, Palo.Oro)));

        Assert.DoesNotContain(Partido.AccionesLegales(e2, e2.Turno), a => a is CantarFlor);
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
