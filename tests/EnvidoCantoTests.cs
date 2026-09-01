using Domain;

namespace Tests;

// Paso 13 — Envido en el reductor (1v1). Decisiones: ventana en la primera baza con
// gating por jugador (A1), sin límite de reviros (B2), Falta contra el fin de la etapa
// del que va primero (A4), envido antes que truco. RULES_Afinadas.md §"El toque de envido".
// La anulación por flor y el esconder/denunciar son del Paso 14.
public class EnvidoCantoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    // J0 con envido 31 (6 y 5 de Oro), J1 con 7 (tres palos distintos). Muestra neutra.
    private static readonly Muestra MuestraNeutra = new(C(6, Palo.Basto));
    private static readonly Carta[] Mano31 = { C(6, Palo.Oro), C(5, Palo.Oro), C(2, Palo.Copa) };
    private static readonly Carta[] Mano7 = { C(7, Palo.Copa), C(5, Palo.Espada), C(3, Palo.Oro) };

    [Fact]
    public void AlCantarEnvido_ElRivalPuedeQuererNoQuererORevirar()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1); // mano = J0, Turno = J0
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));

        var acciones = Partido.AccionesLegales(e1, J1);
        Assert.Contains(acciones, a => a is Quiero);
        Assert.Contains(acciones, a => a is NoQuiero);
        Assert.Contains(acciones, a => a is CantarEnvido c && c.Canto == EnvidoCanto.RealEnvido);
        Assert.Empty(Partido.AccionesLegales(e1, J0)); // el que cantó espera
    }

    [Fact]
    public void EnvidoQuerido_GanaElQueTieneMasPuntos()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1); // Turno = J0
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(new Cobro(E0, 2), e2.CobroEnvido); // J0 (31) le gana a J1 (7)
    }

    [Fact]
    public void EnvidoQuerido_ConEmpate_GanaElMano()
    {
        var e = Estado(Mano31, (Carta[])Mano31.Clone(), repartidor: J1); // ambos 31, mano = J0
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(new Cobro(E0, 2), e2.CobroEnvido); // empate → gana el mano (J0)
    }

    [Fact]
    public void EnvidoNoQuerido_ElQueTocoSeLlevaUno_YSigueLaMano()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new NoQuiero(J1));

        Assert.Equal(new Cobro(E0, 1), e2.CobroEnvido); // el que tocó (J0)
        Assert.Equal(0, e2.NumeroDeMano);                // la mano NO terminó
        Assert.True(e2.EnvidoJugado);
    }

    [Fact]
    public void EnvidoEnvidoNoQuerido_SeLlevanDos()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new CantarEnvido(J1, EnvidoCanto.Envido)); // reviro
        var e3 = Partido.Aplicar(e2, new NoQuiero(J0));

        // No quiere el que abrió; el que tocó el segundo envido (J1) se lleva 2.
        Assert.Equal(new Cobro(E1, 2), e3.CobroEnvido);
    }

    [Fact]
    public void EnvidoRealEnvidoQuerido_ValeCinco()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new CantarEnvido(J1, EnvidoCanto.RealEnvido));
        var e3 = Partido.Aplicar(e2, new Quiero(J0));

        Assert.Equal(new Cobro(E0, 5), e3.CobroEnvido); // 2 + 3, gana J0
    }

    [Fact]
    public void SinLimiteDeReviros_TresEnvidosValenSeis()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new CantarEnvido(J1, EnvidoCanto.Envido));
        var e3 = Partido.Aplicar(e2, new CantarEnvido(J0, EnvidoCanto.Envido));
        var e4 = Partido.Aplicar(e3, new Quiero(J1));

        Assert.Equal(new Cobro(E0, 6), e4.CobroEnvido);
    }

    [Fact]
    public void FaltaEnvidoQuerido_ContraElFinDeLasMalas_SiElPrimeroEstaEnMalas()
    {
        // Partido a 30 (mitad 15), ambos en 0 → el que va primero está en malas: falta 15.
        var e = Estado(Mano31, Mano7, repartidor: J1, contador: new Contador(30));
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.FaltaEnvido));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(new Cobro(E0, 15), e2.CobroEnvido);
    }

    // A4: si el que va primero (el líder) ya está en buenas, la Falta vale lo que le
    // falta para GANAR EL PARTIDO, no para llegar a la mitad.
    [Fact]
    public void FaltaEnvidoQuerido_ContraElFinDelPartido_SiElPrimeroYaEstaEnBuenas()
    {
        // Partido a 30 (mitad 15). El equipo 0 ya está en buenas (20); el líder es él,
        // así que la falta vale lo que le falta al partido: 30 - 20 = 10.
        var contador = new Contador(30).Sumar(E0, 20);
        var e = Estado(Mano31, Mano7, repartidor: J1, contador: contador); // J0 (E0) es mano
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.FaltaEnvido));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));

        Assert.Equal(new Cobro(E0, 10), e2.CobroEnvido);
    }

    // B5: con los dos equipos empatados en puntaje, la Falta vale lo mismo sin importar
    // qué equipo la cante (el "líder" es el mismo puntaje para los dos).
    [Fact]
    public void FaltaEnvido_ConLosEquiposEmpatados_ValeLoMismoParaCualquierEquipo()
    {
        // Los dos en 10 (mitad 15, todavía en malas): falta = 15 - 10 = 5 para cualquiera.
        var contadorEmpatado = new Contador(30).Sumar(E0, 10).Sumar(E1, 10);

        var eAbreJ0 = Estado(Mano31, Mano7, repartidor: J1, contador: contadorEmpatado); // J0 mano
        var e1 = Partido.Aplicar(eAbreJ0, new CantarEnvido(J0, EnvidoCanto.FaltaEnvido));
        var e2 = Partido.Aplicar(e1, new Quiero(J1));
        Assert.Equal(new Cobro(E0, 5), e2.CobroEnvido);

        var eAbreJ1 = Estado(Mano7, Mano31, repartidor: J0, contador: contadorEmpatado); // J1 mano, J1 tiene el 7 ahora
        var e3 = Partido.Aplicar(eAbreJ1, new CantarEnvido(J1, EnvidoCanto.FaltaEnvido));
        var e4 = Partido.Aplicar(e3, new Quiero(J0));
        Assert.Equal(new Cobro(E1, 5), e4.CobroEnvido); // misma falta (5), sea cual sea el que cantó
    }

    [Fact]
    public void ElEnvidoSoloSePuedeTocarEnLaPrimeraBaza()
    {
        // Jugamos la primera baza entera sin tocar envido; después ya no aparece.
        var e = Estado(Mano31, Mano7, repartidor: J1); // Turno J0
        var e1 = Partido.Aplicar(e, new TirarCarta(J0, C(6, Palo.Oro)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J1, C(7, Palo.Copa)));

        // Segunda baza en curso: nadie puede tocar envido.
        Assert.DoesNotContain(Partido.AccionesLegales(e2, e2.Turno), a => a is CantarEnvido);
    }

    [Fact]
    public void MientrasHayEnvidoPendiente_NoSePuedeTirarCarta()
    {
        var e = Estado(Mano31, Mano7, repartidor: J1);
        var e1 = Partido.Aplicar(e, new CantarEnvido(J0, EnvidoCanto.Envido));

        Assert.Throws<InvalidOperationException>(
            () => Partido.Aplicar(e1, new TirarCarta(J1, C(7, Palo.Copa))));
    }

    [Fact]
    public void ElEnvidoVaAntesQueElTruco()
    {
        // J0 canta truco; J1, en vez de responder, toca envido. Primero se resuelve el
        // envido y después queda pendiente el truco.
        var e = Estado(Mano31, Mano7, repartidor: J1); // Turno J0
        var e1 = Partido.Aplicar(e, new CantarTruco(J0));
        var e2 = Partido.Aplicar(e1, new CantarEnvido(J1, EnvidoCanto.Envido));
        var e3 = Partido.Aplicar(e2, new Quiero(J0)); // resuelve el envido (gana J0, 31)

        Assert.Equal(new Cobro(E0, 2), e3.CobroEnvido); // envido resuelto (pendiente de acreditar)
        Assert.True(e3.HayCantoPendiente);              // el truco sigue pendiente
        Assert.Contains(Partido.AccionesLegales(e3, J1), a => a is Quiero); // ahora J1 responde el truco
    }

    // F4: los tantos se resuelven en el momento pero se acreditan al cerrar la mano.
    [Fact]
    public void LosTantos_SeResuelvenEnElMomento_PeroSeAcreditanAlCerrarLaMano()
    {
        var e = Estado(
            new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(4, Palo.Oro) },  // J0: matas, envido 4
            new[] { C(6, Palo.Copa), C(5, Palo.Copa), C(3, Palo.Oro) },     // J1: envido 31, cartas débiles
            repartidor: J0); // mano = J1

        var e1 = Partido.Aplicar(e, new CantarEnvido(J1, EnvidoCanto.Envido));
        var e2 = Partido.Aplicar(e1, new Quiero(J0)); // envido resuelto: gana J1 (31)

        Assert.Equal(new Cobro(E1, 2), e2.CobroEnvido);
        Assert.Equal(0, e2.Contador.Puntos(E1)); // todavía no acreditado

        // Se juega la mano: J0 gana las dos bazas con las matas.
        var e3 = Partido.Aplicar(e2, new TirarCarta(J1, C(6, Palo.Copa)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J0, C(1, Palo.Espada)));
        var e5 = Partido.Aplicar(e4, new TirarCarta(J0, C(1, Palo.Basto)));
        var e6 = Partido.Aplicar(e5, new TirarCarta(J1, C(5, Palo.Copa)));

        // Al cerrar la mano se acreditan envido (E1, 2) y truco liso (E0, 1).
        Assert.Equal(2, e6.Contador.Puntos(E1));
        Assert.Equal(1, e6.Contador.Puntos(E0));
        Assert.Equal(1, e6.NumeroDeMano);
    }

    private static EstadoPartida Estado(
        IReadOnlyList<Carta> mano0, IReadOnlyList<Carta> mano1, JugadorId repartidor, Contador? contador = null)
    {
        var mano = new JugadorId((repartidor.Valor + 1) % 2);
        return new EstadoPartida
        {
            Contador = contador ?? new Contador(30),
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
