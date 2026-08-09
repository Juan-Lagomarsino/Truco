using Domain;

namespace Tests;

// Paso 17b-2 — Schedule del modo de a 6: la máquina de estados redondilla ↔ pico a pico.
// Al cerrar una mano, con 6 jugadores el reductor ramifica: dentro de un pico a pico avanza
// al pico siguiente sin repartir; al terminar un estado elige el próximo según el ciclo
// (redondilla → pico a pico, salvo corte a la mitad) y reparte.
//
// El mano de cada pico es el jugador a la derecha del repartidor: los tres picos tienen de
// mano los tres jugadores consecutivos al repartidor. Con repartidor J0, manos 1, 2, 3.
public class ModoDeA6ScheduleTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly JugadorId J2 = new(2);
    private static readonly JugadorId J3 = new(3);
    private static readonly JugadorId J5 = new(5);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    // Dentro de un pico a pico (no el último): irse al mazo cierra el pico y avanza al pico
    // siguiente SIN repartir. Cambian Activos/Turno/Abridor y el mano (anclado al repartidor);
    // se conservan muestra, número de mano, repartidor y las manos iniciales.
    [Fact]
    public void PicoNoFinal_AvanzaAlPicoSiguiente_SinRepartir()
    {
        // Repartidor J0 → pico 0 tiene de mano a J1 (pareja J1 vs J4).
        var e = Estado6(FaseCiclo.PicoAPico, indicePico: 0, activos: new[] { J1, new JugadorId(4) },
            mano: J1, repartidor: J0, contador: new Contador(30));

        var e2 = Partido.Aplicar(e, new IrseAlMazo(J1)); // J1 (E1) se va → E0 se lleva 1

        Assert.Equal(FaseCiclo.PicoAPico, e2.Fase);
        Assert.Equal(1, e2.IndicePico);
        Assert.Equal(new[] { J2, J5 }, e2.Activos);       // pico 1: pareja J2 vs J5
        Assert.Equal(J2, e2.Turno);                        // mano del pico 1 = repartidor+2
        Assert.Equal(J2, e2.Abridor);
        Assert.Equal(J2, e2.JugadorMano);
        Assert.Equal(0, e2.NumeroDeMano);                  // no se repartió
        Assert.Equal(J0, e2.Repartidor);                   // el repartidor no rota entre picos
        Assert.Equal(e.Muestra, e2.Muestra);
        Assert.Same(e.ManosIniciales, e2.ManosIniciales);
        Assert.Equal(1, e2.Contador.Puntos(E0));
        Assert.Equal(0, e2.Contador.Puntos(E1));
    }

    // El último pico (índice 2) cierra el estado: se reparte una redondilla nueva (todos
    // activos), con número de mano incrementado y repartidor rotado una silla.
    [Fact]
    public void UltimoPico_CierraElEstado_RepartiendoRedondilla()
    {
        // Repartidor J0 → pico 2 tiene de mano a J3 (pareja J3 vs J0).
        var e = Estado6(FaseCiclo.PicoAPico, indicePico: 2, activos: new[] { J0, J3 },
            mano: J3, repartidor: J0, contador: new Contador(30));

        var e2 = Partido.Aplicar(e, new IrseAlMazo(J3)); // J3 (E1) se va → E0 se lleva 1

        Assert.Equal(FaseCiclo.Redondilla, e2.Fase);
        Assert.Empty(e2.Activos);                          // redondilla: juegan los seis
        Assert.Equal(1, e2.NumeroDeMano);                  // se repartió
        Assert.Equal(J1, e2.Repartidor);                   // rotó una silla
        Assert.Equal(J2, e2.JugadorMano);                  // repartidor+1
        Assert.Equal(1, e2.Contador.Puntos(E0));
    }

    // Corte a la mitad: al terminar una redondilla, si un equipo llegó a la mitad, el próximo
    // estado es otra redondilla (no un pico a pico).
    [Fact]
    public void Redondilla_ConUnEquipoEnBuenas_SigueConRedondilla()
    {
        var contador = new Contador(30).Sumar(E0, 16); // E0 en buenas (mitad = 15)
        var e = Estado6(FaseCiclo.Redondilla, indicePico: 0, activos: Array.Empty<JugadorId>(),
            mano: J1, repartidor: J0, contador: contador);

        var e2 = Partido.Aplicar(e, new IrseAlMazo(J1)); // cierra la redondilla

        Assert.Equal(FaseCiclo.Redondilla, e2.Fase);
        Assert.Empty(e2.Activos);
        Assert.Equal(1, e2.NumeroDeMano);
        Assert.Equal(J1, e2.Repartidor);
    }

    // Al terminar una redondilla sin nadie en buenas, el próximo estado es un pico a pico:
    // se reparte y arranca el pico 0.
    [Fact]
    public void Redondilla_SinNadieEnBuenas_ArrancaPicoAPico()
    {
        var e = Estado6(FaseCiclo.Redondilla, indicePico: 0, activos: Array.Empty<JugadorId>(),
            mano: J1, repartidor: J0, contador: new Contador(30));

        var e2 = Partido.Aplicar(e, new IrseAlMazo(J1)); // cierra la redondilla

        Assert.Equal(FaseCiclo.PicoAPico, e2.Fase);
        Assert.Equal(0, e2.IndicePico);
        Assert.Equal(1, e2.NumeroDeMano);        // se repartió
        Assert.Equal(J1, e2.Repartidor);         // rotó una silla → repartidor del pico a pico
        // Repartidor J1 → pico 0 = pareja J2 vs J5, mano J2.
        Assert.Equal(new[] { J2, J5 }, e2.Activos);
        Assert.Equal(J2, e2.Turno);
        Assert.Equal(J2, e2.JugadorMano);
    }

    // El corte a la mitad se evalúa sólo al terminar el estado entero. Dentro de un pico a
    // pico, aunque un equipo esté en buenas, se juegan igual los tres picos.
    [Fact]
    public void MitadDeUnPicoAPico_ConUnEquipoEnBuenas_IgualAvanzaAlPicoSiguiente()
    {
        var contador = new Contador(30).Sumar(E0, 16); // E0 en buenas
        var e = Estado6(FaseCiclo.PicoAPico, indicePico: 0, activos: new[] { J1, new JugadorId(4) },
            mano: J1, repartidor: J0, contador: contador);

        var e2 = Partido.Aplicar(e, new IrseAlMazo(J1));

        Assert.Equal(FaseCiclo.PicoAPico, e2.Fase);
        Assert.Equal(1, e2.IndicePico);
        Assert.Equal(new[] { J2, J5 }, e2.Activos);
    }

    // Manos sin flor ni piezas (muestra 6 de Basto): irse al mazo no abre ventana de denuncia.
    private static EstadoPartida Estado6(
        FaseCiclo fase, int indicePico, IReadOnlyList<JugadorId> activos,
        JugadorId mano, JugadorId repartidor, Contador contador)
    {
        var dummy = new[] { C(4, Palo.Copa), C(6, Palo.Oro), C(3, Palo.Espada) };
        var manos = new IReadOnlyList<Carta>[] { dummy, dummy, dummy, dummy, dummy, dummy };
        return new EstadoPartida
        {
            Contador = contador,
            Semilla = 0,
            NumeroDeMano = 0,
            CantidadJugadores = 6,
            Repartidor = repartidor,
            Muestra = new Muestra(C(6, Palo.Basto)),
            Manos = manos,
            ManosIniciales = manos,
            Activos = activos,
            Fase = fase,
            IndicePico = indicePico,
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
