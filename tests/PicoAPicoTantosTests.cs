using Domain;

namespace Tests;

// Paso 17b-3 — Tantos especiales del pico a pico (modo de a 6). RULES §"Partidas de a 6":
// dentro de un pico a pico el Falta Envido vale 6 fijo y la Contra Flor al Resto vale 12
// (los 6 de la falta más las dos flores en juego). Fuera del pico, la Falta se cuenta
// contra la mitad/largo como siempre.
public class PicoAPicoTantosTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly JugadorId J4 = new(4);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto)); // 6 no define piezas

    // Manos sin flor: J1 tiene envido alto (33), J4 bajo (5); ninguna forma flor.
    private static readonly Carta[] EnvidoAlto = { C(7, Palo.Espada), C(6, Palo.Espada), C(4, Palo.Copa) };
    private static readonly Carta[] EnvidoBajo = { C(5, Palo.Oro), C(3, Palo.Basto), C(2, Palo.Copa) };

    // Manos con flor: J1 flor 38 (Espada), J4 flor 37 (Oro).
    private static readonly Carta[] Flor38Espada = { C(7, Palo.Espada), C(6, Palo.Espada), C(5, Palo.Espada) };
    private static readonly Carta[] Flor37Oro = { C(7, Palo.Oro), C(6, Palo.Oro), C(4, Palo.Oro) };

    // En el pico a pico la Falta Envido querida vale 6 fijo (no la falta contra la mitad).
    [Fact]
    public void FaltaEnvido_EnElPico_ValeSeis()
    {
        var e = EstadoPico(EnvidoAlto, EnvidoBajo); // turno J1 (mano del pico 0)
        var e1 = Partido.Aplicar(e, new CantarEnvido(J1, EnvidoCanto.FaltaEnvido));
        var e2 = Partido.Aplicar(e1, new Quiero(J4));

        Assert.Equal(new Cobro(E1, 6), e2.CobroEnvido); // J1 (E1) gana el envido, 6 fijo
    }

    // En el pico a pico la Contra Flor al Resto querida vale 12 (6 de la falta + 3 por cada flor).
    [Fact]
    public void ContraFlorAlResto_EnElPico_ValeDoce()
    {
        var e = EstadoPico(Flor38Espada, Flor37Oro);
        var e1 = Partido.Aplicar(e, new CantarContraFlorAlResto(J1));
        var e2 = Partido.Aplicar(e1, new Quiero(J4));

        Assert.Equal(new Cobro(E1, 12), e2.CobroFlor); // J1 (E1) tiene la flor más alta
    }

    // Un estado de pico a pico (pico 0, repartidor J0): pareja J1 vs J4, mano J1.
    private static EstadoPartida EstadoPico(IReadOnlyList<Carta> manoJ1, IReadOnlyList<Carta> manoJ4)
    {
        var vacia = Array.Empty<Carta>();
        var manos = new IReadOnlyList<Carta>[] { vacia, manoJ1, vacia, vacia, manoJ4, vacia };
        return new EstadoPartida
        {
            Contador = new Contador(30),
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 6,
            Repartidor = J0,
            Muestra = MuestraNeutra,
            Manos = manos,
            ManosIniciales = manos,
            Activos = new[] { J1, J4 },
            Fase = FaseCiclo.PicoAPico,
            IndicePico = 0,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = J1,
            Turno = J1,
        };
    }
}
