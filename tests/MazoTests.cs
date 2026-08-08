using Domain;

namespace Tests;

// Paso 3 — El mazo español de 40 y el flujo de la mesa: barajar, cortar,
// repartir, y recién ahí dar vuelta la muestra.
// RULES_Afinadas.md §"Lógica" (mazo de 40, sin 8, sin 9) y §"La Muestra"
// (se saca después de repartir; las manos se reparten sobre 39).
public class MazoTests
{
    private static IReadOnlyList<Carta> Ordenadas(IEnumerable<Carta> cartas) =>
        cartas.OrderBy(c => c.Palo).ThenBy(c => c.Numero).ToList();

    [Fact]
    public void MazoCompleto_TieneCuarentaCartas()
    {
        Assert.Equal(40, Mazo.Completo().Cantidad);
    }

    [Fact]
    public void MazoCompleto_NoTieneOchoNiNueve()
    {
        var numeros = Mazo.Completo().Cartas.Select(c => c.Numero);

        Assert.DoesNotContain(numeros, n => n == 8 || n == 9);
    }

    [Fact]
    public void MazoCompleto_NoTieneCartasDuplicadas()
    {
        var cartas = Mazo.Completo().Cartas;

        Assert.Equal(cartas.Count, cartas.Distinct().Count());
    }

    [Theory]
    [InlineData(Palo.Oro)]
    [InlineData(Palo.Copa)]
    [InlineData(Palo.Espada)]
    [InlineData(Palo.Basto)]
    public void MazoCompleto_TieneDiezCartasDeCadaPalo(Palo palo)
    {
        var delPalo = Mazo.Completo().Cartas.Count(c => c.Palo == palo);

        Assert.Equal(10, delPalo);
    }

    [Fact]
    public void Cortar_PreservaLasCuarentaCartas()
    {
        var mazo = Mazo.Completo();

        var cortado = mazo.Cortar(15);

        Assert.Equal(Ordenadas(mazo.Cartas), Ordenadas(cortado.Cartas));
    }

    [Fact]
    public void Cortar_MandaLasDeArribaAbajo()
    {
        // El que corta levanta las primeras `posicion` cartas y las manda abajo.
        var mazo = Mazo.Completo();
        var original = mazo.Cartas;

        var cortado = mazo.Cortar(15).Cartas;

        Assert.Equal(original[15], cortado[0]);                 // la de abajo del corte queda arriba
        Assert.Equal(original[14], cortado[^1]);                // la de arriba del corte queda al fondo
    }

    [Theory]
    [InlineData(0)]
    [InlineData(40)]
    public void Cortar_FueraDeRango_EsRechazado(int posicion)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mazo.Completo().Cortar(posicion));
    }

    [Theory]
    [InlineData(2)]  // 1v1
    [InlineData(4)]  // 2v2
    [InlineData(6)]  // 3v3
    public void Repartir_DaTresCartasACadaJugador(int jugadores)
    {
        var reparto = Mazo.Completo().Repartir(jugadores);

        Assert.Equal(jugadores, reparto.Manos.Count);
        Assert.All(reparto.Manos, mano => Assert.Equal(3, mano.Count));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    public void LaMuestra_SeSacaDespuesDeRepartir(int jugadores)
    {
        var mazo = Mazo.Completo();
        var repartidas = jugadores * 3;

        var reparto = mazo.Repartir(jugadores);

        // La muestra es la carta que sigue a todas las repartidas.
        Assert.Equal(mazo.Cartas[repartidas], reparto.Muestra.Carta);
        // Y no la tiene ningún jugador.
        Assert.DoesNotContain(reparto.Manos.SelectMany(m => m), c => c == reparto.Muestra.Carta);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    public void ElReparto_ConservaLasCuarentaCartas(int jugadores)
    {
        var mazo = Mazo.Completo();

        var reparto = mazo.Repartir(jugadores);
        var todas = reparto.Manos.SelectMany(m => m)
            .Append(reparto.Muestra.Carta)
            .Concat(reparto.Resto.Cartas);

        Assert.Equal(Ordenadas(mazo.Cartas), Ordenadas(todas));
    }

    [Fact]
    public void Repartir_EsRoundRobin_UnaCartaPorJugadorPorVuelta()
    {
        // Documenta la convención de reparto: carta i va al jugador (i mod N).
        var mazo = Mazo.Completo();
        var c = mazo.Cartas;

        var reparto = mazo.Repartir(2);

        // Jugador 0 recibe las cartas 0, 2, 4; jugador 1 las 1, 3, 5.
        Assert.Equal(new[] { c[0], c[2], c[4] }, reparto.Manos[0]);
        Assert.Equal(new[] { c[1], c[3], c[5] }, reparto.Manos[1]);
    }

    [Fact]
    public void FlujoCompleto_BarajarCortarRepartirMuestra_ConservaElMazo()
    {
        // El flujo de la mesa: J1 baraja, J2 corta, J1 reparte, se da vuelta la muestra.
        var mazo = Mazo.Completo()
            .Barajar(new BarajadorConSemilla(2024))
            .Cortar(17);

        var reparto = mazo.Repartir(2);
        var todas = reparto.Manos.SelectMany(m => m)
            .Append(reparto.Muestra.Carta)
            .Concat(reparto.Resto.Cartas);

        Assert.Equal(40, todas.Count());
        Assert.Equal(Ordenadas(Mazo.Completo().Cartas), Ordenadas(todas));
    }
}
