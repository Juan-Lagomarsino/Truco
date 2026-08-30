using Bot;
using Domain;

namespace Tests;

// Paso 18a — /bot: casos puntuales de PoliticaSimple, además del fuzz de bot vs bot en
// PoliticaSimpleFuzzTests. Cada test fija una mano concreta para que la decisión sea
// determinística y verificable, no sólo "no explota".
public class PoliticaSimpleTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    [Fact]
    public void ConFlorEnElPrimerTurno_LaCanta_EnVezDeTirarCarta()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) }, // flor
            mano1: new[] { C(4, Palo.Oro), C(5, Palo.Oro), C(3, Palo.Copa) },
            repartidor: J1); // mano = J0, turno = J0

        var accion = PoliticaSimple.Elegir(e, J0);

        Assert.IsType<CantarFlor>(accion);
    }

    [Fact]
    public void ConEnvidoAlto_LoAbre_EnVezDeTirarCarta()
    {
        // Envido de J0: 20 + 7 + 6 = 33 (dos de Oro sin pieza), por encima del umbral.
        // Muestra de Copa para que ninguna carta de J0 (Oro/Espada) sea pieza ni forme flor.
        var muestra = new Muestra(C(6, Palo.Copa));
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Espada) },
            mano1: new[] { C(2, Palo.Espada), C(4, Palo.Basto), C(5, Palo.Basto) },
            repartidor: J1); // turno = J0

        var accion = PoliticaSimple.Elegir(e, J0);

        Assert.IsType<CantarEnvido>(accion);
        Assert.Equal(EnvidoCanto.Envido, ((CantarEnvido)accion).Canto); // conservador: pide el mínimo
    }

    [Fact]
    public void SinFlorNiEnvidoNiCartaBuena_TiraLaCartaMasDebil()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        // J0: envido bajo (0, tres palos distintos, la más alta vale 0), sin pieza ni mata.
        // La más débil de las tres por Jerarquia es el 4 de Copa (blanca, nivel 19).
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(12, Palo.Oro), C(11, Palo.Espada), C(4, Palo.Copa) },
            mano1: new[] { C(3, Palo.Basto), C(2, Palo.Basto), C(1, Palo.Copa) },
            repartidor: J1); // turno = J0

        var accion = PoliticaSimple.Elegir(e, J0);

        var tirada = Assert.IsType<TirarCarta>(accion);
        Assert.Equal(C(4, Palo.Copa), tirada.Carta);
    }

    [Fact]
    public void RespondeEnvido_ConTantoAltoQuiere_ConTantoBajoNoQuiere()
    {
        // Muestra de Copa para que ninguna carta de J0 (Oro/Espada) sea pieza ni forme flor.
        var muestra = new Muestra(C(6, Palo.Copa));
        // J0 responde: envido 33 (alto) en un caso, 20+2+1=23 (bajo) en el otro.
        var eAlto = EstadoConEnvidoPendiente(muestra,
            mano0: new[] { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Espada) },
            mano1: new[] { C(2, Palo.Espada), C(4, Palo.Basto), C(5, Palo.Basto) });
        var eBajo = EstadoConEnvidoPendiente(muestra,
            mano0: new[] { C(2, Palo.Oro), C(1, Palo.Oro), C(4, Palo.Espada) },
            mano1: new[] { C(2, Palo.Espada), C(4, Palo.Basto), C(5, Palo.Basto) });

        Assert.IsType<Quiero>(PoliticaSimple.Elegir(eAlto, J0));
        Assert.IsType<NoQuiero>(PoliticaSimple.Elegir(eBajo, J0));
    }

    [Fact]
    public void NuncaLeeLasCartasDelRival_ElMismoEstadoPropioDaLaMismaDecision()
    {
        // Prueba indirecta de "sólo lo que ese jugador ve": si sólo cambia la mano del
        // rival (que J0 no debería mirar), la decisión de J0 no cambia.
        var muestra = new Muestra(C(6, Palo.Basto));
        var manoJ0 = new[] { C(12, Palo.Oro), C(11, Palo.Espada), C(4, Palo.Copa) };
        var eConRivalA = EstadoConManos(muestra, manoJ0, new[] { C(3, Palo.Basto), C(2, Palo.Basto), C(1, Palo.Copa) }, J1);
        var eConRivalB = EstadoConManos(muestra, manoJ0, new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(7, Palo.Espada) }, J1);

        Assert.Equal(PoliticaSimple.Elegir(eConRivalA, J0), PoliticaSimple.Elegir(eConRivalB, J0));
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

    // J1 (mano) ya tocó el envido; J0 (equipo 0) tiene que responder.
    private static EstadoPartida EstadoConEnvidoPendiente(
        Muestra muestra, IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1)
    {
        var e = EstadoConManos(muestra, mano0, mano1, repartidor: J0); // turno = J1
        return Partido.Aplicar(e, new CantarEnvido(J1, EnvidoCanto.Envido));
    }
}
