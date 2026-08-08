using Domain;

namespace Tests;

// Paso 4 — Fuerza de una carta dada la muestra.
// RULES_Afinadas.md §"Jerarquia en formato programacion": la tabla de 19 niveles,
// de mejor a peor, con el 12 espejo. Fuerza más alta = carta más fuerte; misma
// fuerza = parda.
public class JerarquiaTests
{
    // Un representante de cada uno de los 19 niveles, en orden de fuerza, con la
    // muestra 3 de Oro (que NO es pieza, así los cuatro 12 caen todos al nivel 13).
    // La lista completa tiene que quedar estrictamente decreciente en fuerza.
    [Fact]
    public void LosDiecinueveNiveles_QuedanEstrictamenteDecrecientes()
    {
        var muestra = new Muestra(new Carta(3, Palo.Oro)); // y = Oro, no es pieza

        Carta[] deMejorAPeor =
        {
            new(2, Palo.Oro),      // 1  pieza
            new(4, Palo.Oro),      // 2  pieza
            new(5, Palo.Oro),      // 3  pieza
            new(11, Palo.Oro),     // 4  pieza
            new(10, Palo.Oro),     // 5  pieza
            new(1, Palo.Espada),   // 6  mata
            new(1, Palo.Basto),    // 7  mata
            new(7, Palo.Espada),   // 8  mata
            new(7, Palo.Oro),      // 9  mata
            new(3, Palo.Copa),     // 10 chica (3)
            new(2, Palo.Copa),     // 11 chica (2, no muestra)
            new(1, Palo.Copa),     // 12 chica (1, no espada/basto)
            new(12, Palo.Copa),    // 13 negra (12 común)
            new(11, Palo.Copa),    // 14 negra (11, no muestra)
            new(10, Palo.Copa),    // 15 negra (10, no muestra)
            new(7, Palo.Copa),     // 16 blanca (7, no espada/oro)
            new(6, Palo.Oro),      // 17 blanca (6)
            new(5, Palo.Copa),     // 18 blanca (5, no muestra)
            new(4, Palo.Copa),     // 19 blanca (4, no muestra)
        };

        var fuerzas = deMejorAPeor.Select(c => Jerarquia.Fuerza(c, muestra)).ToArray();

        for (int i = 0; i < fuerzas.Length - 1; i++)
            Assert.True(fuerzas[i] > fuerzas[i + 1],
                $"El nivel {i + 1} ({deMejorAPeor[i].Numero} de {deMejorAPeor[i].Palo}) debería ser " +
                $"más fuerte que el nivel {i + 2} ({deMejorAPeor[i + 1].Numero} de {deMejorAPeor[i + 1].Palo}).");
    }

    // El 12 del palo de la muestra espeja a la muestra sólo si la muestra es pieza.
    [Fact]
    public void El12DelPaloDeLaMuestra_EspejaLaMuestra_SoloSiLaMuestraEsPieza()
    {
        var conPieza = new Muestra(new Carta(2, Palo.Oro));    // muestra pieza
        var sinPieza = new Muestra(new Carta(1, Palo.Oro));    // muestra no pieza

        var doce = new Carta(12, Palo.Oro);

        // Espeja: el 12 de Oro tiene la misma fuerza que el 2 de Oro (la pieza más alta).
        Assert.Equal(Jerarquia.Fuerza(new Carta(2, Palo.Oro), conPieza), Jerarquia.Fuerza(doce, conPieza));
        // No espeja: como 12 común es más débil que una chica cualquiera (un 3).
        Assert.True(Jerarquia.Fuerza(new Carta(3, Palo.Copa), sinPieza) > Jerarquia.Fuerza(doce, sinPieza));
    }

    // Dos cartas del mismo nivel empatan: van parda.
    [Fact]
    public void DosCartasDelMismoNivel_TienenLaMismaFuerza_VanParda()
    {
        var muestra = new Muestra(new Carta(6, Palo.Copa)); // no pieza

        Assert.Equal(
            Jerarquia.Fuerza(new Carta(3, Palo.Oro), muestra),
            Jerarquia.Fuerza(new Carta(3, Palo.Basto), muestra));
    }

    // Invariante (skill): la tabla cubre exactamente las 40 cartas para cualquier
    // muestra — ninguna carta se queda sin fuerza.
    [Fact]
    public void ParaCualquierMuestra_LasCuarentaCartasTienenFuerza()
    {
        var todas = Mazo.Completo().Cartas;

        foreach (var cartaMuestra in todas)
        {
            var muestra = new Muestra(cartaMuestra);
            foreach (var carta in todas)
                Assert.True(Jerarquia.Fuerza(carta, muestra) >= 1);
        }
    }

    // Invariante (skill): nunca hay dos cartas JUGABLES con la misma fuerza de pieza.
    // Entre las 39 cartas repartibles (todas menos la muestra), las cinco más fuertes
    // (las piezas) tienen fuerzas distintas y por encima del resto.
    [Theory]
    [InlineData(3, Palo.Oro)]    // muestra no pieza
    [InlineData(5, Palo.Oro)]    // muestra pieza (el 12 espejo ocupa el lugar vacante)
    public void LasCincoPiezas_OcupanCincoNivelesUnicosPorEncimaDelResto(int numMuestra, Palo paloMuestra)
    {
        var muestra = new Muestra(new Carta(numMuestra, paloMuestra));

        var repartibles = Mazo.Completo().Cartas
            .Where(c => c != muestra.Carta)                 // la muestra no se reparte
            .Select(c => Jerarquia.Fuerza(c, muestra))
            .OrderByDescending(f => f)
            .ToArray();

        var top5 = repartibles.Take(5).ToArray();

        Assert.Equal(5, top5.Distinct().Count());           // cinco fuerzas distintas
        Assert.True(top5[4] > repartibles[5]);              // la quinta pieza supera a la sexta carta
    }
}
