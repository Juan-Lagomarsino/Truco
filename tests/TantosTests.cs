using Domain;

namespace Tests;

// Paso 7 — Valor de una carta para el tanto (envido y flor).
// RULES_Afinadas.md §"Cuanto vale cada carta":
//   1..7 no pieza valen su número; 10,11,12 no pieza valen 0;
//   piezas: 2→30, 4→29, 5→28, 11→27, 10→27; el 12 espejo vale como la pieza que copia.
public class TantosTests
{
    // Muestra de Copa: ninguna carta de Oro es pieza, así se ve el valor "normal".
    private static readonly Muestra MuestraCopa = new(new Carta(3, Palo.Copa));

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 6)]
    [InlineData(7, 7)]
    public void DelUnoAlSiete_NoPiezas_ValenSuNumero(int numero, int valorEsperado)
    {
        Assert.Equal(valorEsperado, Tantos.De(new Carta(numero, Palo.Oro), MuestraCopa));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void DiezOnceYDoce_NoPiezas_ValenCero(int numero)
    {
        Assert.Equal(0, Tantos.De(new Carta(numero, Palo.Oro), MuestraCopa));
    }

    [Theory]
    [InlineData(2, 30)]
    [InlineData(4, 29)]
    [InlineData(5, 28)]
    [InlineData(11, 27)]
    [InlineData(10, 27)]
    public void LasPiezas_ValenSuValorEspecial(int numero, int valorEsperado)
    {
        var muestra = new Muestra(new Carta(3, Palo.Oro)); // piezas de Oro
        Assert.Equal(valorEsperado, Tantos.De(new Carta(numero, Palo.Oro), muestra));
    }

    // Línea 151 del documento: 11 y 10 de la muestra valen los dos 27, aunque el 11
    // le gana al 10 en fuerza. Fuerza y tantos son funciones distintas.
    [Fact]
    public void ElOnceYElDiezDeLaMuestra_ValenAmbosVeintisiete_AunqueDifieranEnFuerza()
    {
        var muestra = new Muestra(new Carta(3, Palo.Oro));
        var once = new Carta(11, Palo.Oro);
        var diez = new Carta(10, Palo.Oro);

        Assert.Equal(27, Tantos.De(once, muestra));
        Assert.Equal(27, Tantos.De(diez, muestra));
        Assert.True(Jerarquia.Fuerza(once, muestra) > Jerarquia.Fuerza(diez, muestra));
    }

    [Fact]
    public void El12Espejo_ValeComoLaPiezaQueCopia_SoloSiLaMuestraEsPieza()
    {
        var conPieza = new Muestra(new Carta(2, Palo.Oro)); // muestra pieza → espejo vale 30
        var sinPieza = new Muestra(new Carta(1, Palo.Oro)); // muestra no pieza → 12 común, 0
        var doceOro = new Carta(12, Palo.Oro);

        Assert.Equal(30, Tantos.De(doceOro, conPieza));
        Assert.Equal(0, Tantos.De(doceOro, sinPieza));
    }

    // Invariante (skill): el valor de una carta siempre cae en [0, 30].
    [Fact]
    public void ParaCualquierMuestra_ElValorEstaEntreCeroYTreinta()
    {
        var todas = Mazo.Completo().Cartas;

        foreach (var cartaMuestra in todas)
        {
            var muestra = new Muestra(cartaMuestra);
            foreach (var carta in todas)
            {
                int v = Tantos.De(carta, muestra);
                Assert.InRange(v, 0, 30);
            }
        }
    }
}
