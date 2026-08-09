using Domain;

namespace Tests;

// Paso 11 — El reductor: EstadoPartida + AccionesLegales + Aplicar, en 1v1 y sólo
// cartas (sin cantos todavía). Gana la mano el que gana dos bazas y suma 1 (truco
// liso). RULES_Afinadas.md §"Como se resuelve la mano" y §"Explicacion".
// Decisiones D1 (repartidor parámetro), D2 (tras parda abre el mano), D3 (semilla
// en el estado).
public class PartidoTests
{
    private static readonly JugadorId J0 = new(0);
    private static readonly JugadorId J1 = new(1);

    private static Carta C(int n, Palo p) => new(n, p);

    // --- Reparto y arranque ---

    [Fact]
    public void NuevaPartida_ReparteTresCartasACadaJugador_YHayMuestra()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);

        Assert.Equal(2, e.Manos.Count);
        Assert.All(e.Manos, m => Assert.Equal(3, m.Count));
    }

    [Fact]
    public void NuevaPartida_RepartidorPorDefectoEsCero_YElManoEsUno()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);

        Assert.Equal(J0, e.Repartidor);
        Assert.Equal(J1, e.JugadorMano);
        Assert.Equal(J1, e.Turno); // abre el mano
    }

    [Fact]
    public void NuevaPartida_ConRepartidorUno_ElManoEsCero()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7, repartidorInicial: J1);

        Assert.Equal(J0, e.JugadorMano);
        Assert.Equal(J0, e.Turno);
    }

    // --- AccionesLegales ---

    [Fact]
    public void AccionesLegales_ParaElJugadorEnTurno_HayUnaPorCadaCarta()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var enTurno = e.Turno;

        var acciones = Partido.AccionesLegales(e, enTurno);

        Assert.Equal(3, acciones.Count);
        Assert.All(acciones, a => Assert.IsType<TirarCarta>(a));
    }

    [Fact]
    public void AccionesLegales_ParaElQueNoEsSuTurno_EstaVacia()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var otro = e.Turno.Equals(J0) ? J1 : J0;

        Assert.Empty(Partido.AccionesLegales(e, otro));
    }

    // --- Tirar cartas ---

    [Fact]
    public void Aplicar_TirarCarta_SacaLaCartaDeLaManoYPasaElTurno()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var jugador = e.Turno;
        var carta = e.Manos[jugador.Valor][0];

        var d = Partido.Aplicar(e, new TirarCarta(jugador, carta));

        Assert.DoesNotContain(carta, d.Manos[jugador.Valor]);
        Assert.NotEqual(jugador, d.Turno); // en 1v1 pasa al otro
    }

    [Fact]
    public void Aplicar_TirarFueraDeTurno_Lanza()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var otro = e.Turno.Equals(J0) ? J1 : J0;
        var carta = e.Manos[otro.Valor][0];

        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e, new TirarCarta(otro, carta)));
    }

    [Fact]
    public void Aplicar_TirarUnaCartaQueNoEstaEnLaMano_Lanza()
    {
        var e = Partido.Nueva(largo: 30, semilla: 7);
        var jugador = e.Turno;
        var ajena = e.Manos[(jugador.Valor + 1) % 2][0]; // carta del otro

        Assert.Throws<InvalidOperationException>(() => Partido.Aplicar(e, new TirarCarta(jugador, ajena)));
    }

    // --- Resolución de bazas dentro de la mano (estado literal) ---

    [Fact]
    public void TrasGanarUnaBaza_AbreLaSiguiente_ElGanador()
    {
        // Mano = J1 (repartidor J0). J1 abre con una carta débil, J0 gana la baza.
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(1, Palo.Espada), C(4, Palo.Oro), C(5, Palo.Oro) }, // J0 tiene la 1 de Espada (mata)
            mano1: new[] { C(4, Palo.Copa), C(6, Palo.Oro), C(7, Palo.Copa) },  // J1 débiles
            repartidor: J0);

        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(4, Palo.Copa)));  // abre el mano J1
        var e2 = Partido.Aplicar(e1, new TirarCarta(J0, C(1, Palo.Espada))); // J0 gana

        Assert.Equal(J0, e2.Abridor); // la baza siguiente la abre el ganador
        Assert.Equal(J0, e2.Turno);
    }

    [Fact]
    public void TrasUnaParda_AbreLaSiguiente_ElMano()
    {
        // Dos 3 de distinto palo empardan. Debe abrir la siguiente el mano (J1).
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(3, Palo.Oro), C(4, Palo.Oro), C(5, Palo.Oro) },
            mano1: new[] { C(3, Palo.Copa), C(6, Palo.Oro), C(7, Palo.Copa) },
            repartidor: J0); // mano = J1

        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(3, Palo.Copa)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J0, C(3, Palo.Oro)));

        Assert.Single(e2.BazasGanadas);
        Assert.True(e2.BazasGanadas[0].EsParda);
        Assert.Equal(J1, e2.Abridor); // D2: abre el mano
        Assert.Equal(J1, e2.Turno);
    }

    [Fact]
    public void AlGanarLaMano_ElEquipoSumaUnPunto_YSeReparteLaManoSiguiente()
    {
        // J0 tiene las dos cartas más fuertes: gana bazas 1 y 2, gana la mano.
        var muestra = new Muestra(C(6, Palo.Basto));
        var e = EstadoConManos(
            muestra,
            mano0: new[] { C(1, Palo.Espada), C(1, Palo.Basto), C(4, Palo.Oro) }, // dos matas
            mano1: new[] { C(4, Palo.Copa), C(5, Palo.Copa), C(6, Palo.Copa) },
            repartidor: J0); // mano = J1 abre

        // Baza 1: J1 tira débil, J0 gana con 1 de Espada.
        var e1 = Partido.Aplicar(e, new TirarCarta(J1, C(4, Palo.Copa)));
        var e2 = Partido.Aplicar(e1, new TirarCarta(J0, C(1, Palo.Espada)));
        // Baza 2: abre J0, gana con 1 de Basto.
        var e3 = Partido.Aplicar(e2, new TirarCarta(J0, C(1, Palo.Basto)));
        var e4 = Partido.Aplicar(e3, new TirarCarta(J1, C(5, Palo.Copa)));

        // J0 ganó la mano (equipo 0) y suma 1. Se repartió la mano siguiente.
        Assert.Equal(1, e4.Contador.Puntos(new EquipoId(0)));
        Assert.Equal(1, e4.NumeroDeMano);
        Assert.All(e4.Manos, m => Assert.Equal(3, m.Count)); // nuevas cartas
        Assert.Equal(J1, e4.Repartidor); // el reparto rotó
    }

    // --- Determinismo (D3) ---

    [Fact]
    public void MismaSemilla_ReparteExactamenteIgual()
    {
        var a = Partido.Nueva(largo: 30, semilla: 123);
        var b = Partido.Nueva(largo: 30, semilla: 123);

        Assert.Equal(a.Muestra.Carta, b.Muestra.Carta);
        Assert.Equal(a.Manos[0], b.Manos[0]);
        Assert.Equal(a.Manos[1], b.Manos[1]);
    }

    // --- Invariantes de partida completa ---

    [Theory]
    [InlineData(11)]
    [InlineData(1234)]
    [InlineData(99999)]
    public void UnaPartidaCompleta_TerminaConUnGanador_YLosPuntosNuncaDecrecen(int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla);

        int puntos0 = 0, puntos1 = 0;
        int pasos = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 5000, "La partida no debería tardar tanto en terminar.");

            var legales = Partido.AccionesLegales(e, e.Turno);
            Assert.NotEmpty(legales); // invariante: nunca vacía para el jugador en turno

            e = Partido.Aplicar(e, legales[0]);

            int n0 = e.Contador.Puntos(new EquipoId(0));
            int n1 = e.Contador.Puntos(new EquipoId(1));
            Assert.True(n0 >= puntos0 && n1 >= puntos1, "Los puntos no pueden decrecer.");
            puntos0 = n0; puntos1 = n1;
        }

        // Exactamente un equipo llegó al largo.
        bool gano0 = e.Contador.Puntos(new EquipoId(0)) >= 30;
        bool gano1 = e.Contador.Puntos(new EquipoId(1)) >= 30;
        Assert.True(gano0 ^ gano1);
    }

    // --- helper ---

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
            BazasGanadas = new List<GanadorBaza>(),
            JugadasBaza = new List<Jugada>(),
            Abridor = mano,
            Turno = mano,
        };
    }
}
