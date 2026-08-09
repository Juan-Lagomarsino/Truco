using Domain;

namespace Tests;

// Paso 14c — Denuncia de flor escondida (1v1). F3/A2: si un jugador tenía flor y no la
// cantó, al cerrar la mano (con todas las cartas a la vista) el rival puede reclamarla y
// se lleva 3; si el rival pasa, no cobra nadie.
public class FlorDenunciaTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto));

    private static readonly Carta[] FlorEspada = { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) };
    private static readonly Carta[] SinFlorDebil = { C(4, Palo.Copa), C(5, Palo.Oro), C(3, Palo.Basto) };
    private static readonly Carta[] SinFlorB = { C(4, Palo.Oro), C(5, Palo.Copa), C(3, Palo.Basto) };

    // J0 esconde la flor (juega sin cantarla) y gana las dos bazas con cartas fuertes.
    private static EstadoPartida ManoConFlorEscondidaJugada()
    {
        var e = Estado(FlorEspada, SinFlorDebil, repartidor: J1); // mano = J0, Turno J0
        var e1 = Partido.Aplicar(e, new TirarCarta(J0, C(7, Palo.Espada))); // no canta flor
        var e2 = Partido.Aplicar(e1, new TirarCarta(J1, C(4, Palo.Copa)));
        var e3 = Partido.Aplicar(e2, new TirarCarta(J0, C(6, Palo.Espada)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J1, C(5, Palo.Oro)));
        return e4; // mano cerrada; J0 escondió flor → ventana de denuncia para J1
    }

    [Fact]
    public void AlCerrarLaMano_ConFlorEscondida_ElRivalPuedeDenunciarOPasar()
    {
        var cierre = ManoConFlorEscondidaJugada();

        var acciones = Partido.AccionesLegales(cierre, J1);
        Assert.Contains(acciones, a => a is DenunciarFlor);
        Assert.Contains(acciones, a => a is Pasar);
        Assert.Empty(Partido.AccionesLegales(cierre, J0)); // el que escondió no hace nada
    }

    [Fact]
    public void SiElRivalDenuncia_SeLlevaLosTresDeLaFlor()
    {
        var cierre = ManoConFlorEscondidaJugada();
        var fin = Partido.Aplicar(cierre, new DenunciarFlor(J1));

        Assert.Equal(3, fin.Contador.Puntos(E1)); // la flor escondida pasa al que denuncia
        Assert.Equal(1, fin.Contador.Puntos(E0)); // el truco liso, para J0 que ganó la mano
        Assert.Equal(1, fin.NumeroDeMano);          // se repartió la siguiente
    }

    [Fact]
    public void SiElRivalPasa_NadieCobraLaFlorEscondida()
    {
        var cierre = ManoConFlorEscondidaJugada();
        var fin = Partido.Aplicar(cierre, new Pasar(J1));

        Assert.Equal(0, fin.Contador.Puntos(E1)); // J0 se salió con la suya
        Assert.Equal(1, fin.Contador.Puntos(E0)); // sólo el truco
        Assert.Equal(1, fin.NumeroDeMano);
    }

    [Fact]
    public void SinFlorEscondida_NoHayVentanaDeDenuncia()
    {
        // Ninguno tiene flor: la mano cierra directo y reparte la siguiente.
        var e = Estado(SinFlorDebil, SinFlorB, repartidor: J1); // mano J0
        var e1 = Partido.Aplicar(e, new TirarCarta(J0, C(4, Palo.Copa)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J1, C(4, Palo.Oro)));
        var e3 = Partido.Aplicar(e2, new TirarCarta(e2.Turno, e2.Manos[e2.Turno.Valor][0]));
        var e4 = Partido.Aplicar(e3, new TirarCarta(e3.Turno, e3.Manos[e3.Turno.Valor][0]));

        Assert.DoesNotContain(Partido.AccionesLegales(e4, e4.Turno), a => a is DenunciarFlor);
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
