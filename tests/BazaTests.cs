using Domain;

namespace Tests;

// Paso 5 — Resolución de una baza (una ronda de cartas sobre la mesa).
// RULES_Afinadas.md §"Como se resuelve la mano": "El que tiró la carta más alta
// gana la ronda". Empate de fuerza = parda.
//
// Alcance: resolución por carta (gana la más fuerte, empate arriba = parda). Es
// correcto para 1v1. El matiz de equipos (dos cartas máximas del mismo equipo
// ganan en vez de empardar) se agrega en el Paso 16.
public class BazaTests
{
    private static readonly Muestra MuestraNeutra = new(new Carta(6, Palo.Oro)); // no pieza, no toca 3/mata

    [Fact]
    public void LaCartaMasFuerte_GanaLaBaza()
    {
        // 1 de Espada (mata, nivel 6) le gana al 7 de Oro (mata, nivel 9).
        var jugadas = new[] { new Carta(1, Palo.Espada), new Carta(7, Palo.Oro) };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.False(resultado.EsParda);
        Assert.Equal(0, resultado.Ganador);
    }

    [Fact]
    public void ElOrdenDeJuego_NoCambiaQuienGana()
    {
        var jugadas = new[] { new Carta(7, Palo.Oro), new Carta(1, Palo.Espada) };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Equal(1, resultado.Ganador); // gana la 1 de Espada, ahora en la posición 1
    }

    [Fact]
    public void LaPieza_LeGanaAUnaMata()
    {
        // Con muestra 2 de Oro, el 4 de Oro es pieza (nivel 2) y le gana a la 1 de Espada.
        var muestra = new Muestra(new Carta(2, Palo.Oro));
        var jugadas = new[] { new Carta(4, Palo.Oro), new Carta(1, Palo.Espada) };

        var resultado = Baza.Resolver(jugadas, muestra);

        Assert.Equal(0, resultado.Ganador);
    }

    [Fact]
    public void EmpateDeFuerza_EsParda()
    {
        // 3 de Oro y 3 de Basto están las dos en el nivel 10.
        var jugadas = new[] { new Carta(3, Palo.Oro), new Carta(3, Palo.Basto) };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.True(resultado.EsParda);
    }

    [Fact]
    public void UnaParda_NoTieneGanador()
    {
        var jugadas = new[] { new Carta(3, Palo.Oro), new Carta(3, Palo.Basto) };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Throws<InvalidOperationException>(() => _ = resultado.Ganador);
    }

    [Fact]
    public void ConVariasCartas_GanaLaMasFuerte()
    {
        // Cuatro cartas; la 1 de Basto (mata, nivel 7) es la más fuerte.
        var jugadas = new[]
        {
            new Carta(4, Palo.Copa),
            new Carta(1, Palo.Basto),
            new Carta(7, Palo.Espada),
            new Carta(6, Palo.Oro),
        };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.Equal(1, resultado.Ganador);
    }

    [Fact]
    public void ConVariasCartas_SiElMaximoEstaEmpatado_EsParda()
    {
        // Dos 3 empatan arriba; la tercera es más débil.
        var jugadas = new[]
        {
            new Carta(3, Palo.Copa),
            new Carta(3, Palo.Basto),
            new Carta(5, Palo.Copa),
        };

        var resultado = Baza.Resolver(jugadas, MuestraNeutra);

        Assert.True(resultado.EsParda);
    }
}
