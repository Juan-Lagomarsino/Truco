using Domain;

namespace Tests;

// Paso 5 + 16a — Resolución de una baza por equipo.
// RULES_Afinadas.md §"Como se resuelve la mano": gana la carta más alta; su equipo se
// lleva la baza. Si el máximo de fuerza lo comparten equipos distintos, es parda; si lo
// comparten dos del mismo equipo, gana ese equipo (no es parda).
public class BazaTests
{
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    private static readonly Muestra MuestraNeutra = new(new Carta(6, Palo.Oro)); // no pieza

    private static Carta C(int n, Palo p) => new(n, p);

    [Fact]
    public void LaCartaMasFuerte_GanaLaBaza()
    {
        // 1 de Espada (mata) le gana al 7 de Oro (mata más baja).
        var jugadas = new[] { (C(1, Palo.Espada), E0), (C(7, Palo.Oro), E1) };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.False(r.EsParda);
        Assert.Equal(0, r.Ganador); // ganó la jugada 0 (equipo 0)
    }

    [Fact]
    public void ElOrdenDeJuego_NoCambiaQuienGana()
    {
        var jugadas = new[] { (C(7, Palo.Oro), E0), (C(1, Palo.Espada), E1) };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Equal(1, r.Ganador); // gana la 1 de Espada, ahora en la posición 1
    }

    [Fact]
    public void LaPieza_LeGanaAUnaMata()
    {
        var muestra = new Muestra(C(2, Palo.Oro)); // el 4 de Oro es pieza
        var jugadas = new[] { (C(4, Palo.Oro), E0), (C(1, Palo.Espada), E1) };

        var r = Baza.Resolver(jugadas, muestra);

        Assert.Equal(0, r.Ganador);
    }

    [Fact]
    public void EmpateEntreEquiposDistintos_EsParda()
    {
        // 3 de Oro y 3 de Copa: mismo nivel, equipos distintos → parda.
        var jugadas = new[] { (C(3, Palo.Oro), E0), (C(3, Palo.Copa), E1) };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.True(r.EsParda);
    }

    [Fact]
    public void UnaParda_NoTieneGanador()
    {
        var jugadas = new[] { (C(3, Palo.Oro), E0), (C(3, Palo.Copa), E1) };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Throws<InvalidOperationException>(() => _ = r.Ganador);
    }

    // 16a: el caso central del 2v2. Dos cartas máximas del mismo equipo → gana el equipo.
    [Fact]
    public void EmpateArribaDelMismoEquipo_GanaEseEquipo_NoEsParda()
    {
        // 3 de Oro y 3 de Copa (mismo nivel) son del equipo 0; el rival tiró más bajo.
        var jugadas = new[]
        {
            (C(3, Palo.Oro), E0),
            (C(5, Palo.Copa), E1),
            (C(3, Palo.Copa), E0),
            (C(4, Palo.Basto), E1),
        };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.False(r.EsParda);
        Assert.Equal(E0, jugadas[r.Ganador].Item2); // ganó el equipo 0
        // G1 (PREGUNTAS_ABIERTAS.md): entre los empatados arriba, "gana" el que tiró
        // primero (posición 0, no la 2) — así el reductor sabe a quién hacer abrir la
        // baza siguiente sin tener que desempatar aparte.
        Assert.Equal(0, r.Ganador);
    }

    [Fact]
    public void ConVariasCartas_GanaLaMasFuerte()
    {
        // La 1 de Basto (mata) es la más fuerte.
        var jugadas = new[]
        {
            (C(4, Palo.Copa), E0),
            (C(1, Palo.Basto), E1),
            (C(7, Palo.Espada), E0),
            (C(6, Palo.Oro), E1),
        };

        var r = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Equal(1, r.Ganador);
    }
}
