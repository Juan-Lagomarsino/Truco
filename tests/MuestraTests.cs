using Domain;

namespace Tests;

// Paso 2 — La Muestra. RULES_Afinadas.md §"La Muestra" y §"Jerarquia".
// La muestra es una carta que define el palo de las piezas (2,4,5,11,10 de su palo)
// y, según su propio número, puede ser ella misma una pieza. Que la muestra sea o no
// pieza es lo que después decide si el 12 de su palo actúa de espejo.
public class MuestraTests
{
    [Theory]
    [InlineData(Palo.Oro)]
    [InlineData(Palo.Copa)]
    [InlineData(Palo.Espada)]
    [InlineData(Palo.Basto)]
    public void ElPaloDePiezas_EsElPaloDeLaMuestra(Palo palo)
    {
        var muestra = new Muestra(new Carta(4, palo));

        Assert.Equal(palo, muestra.PaloDePiezas);
    }

    // Piezas = [2,4,5,11,10]. Si la muestra es una de esas, la muestra es pieza.
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(11)]
    [InlineData(10)]
    public void LaMuestraEsPieza_SiSuNumeroEsUnaPieza(int numero)
    {
        var muestra = new Muestra(new Carta(numero, Palo.Oro));

        Assert.True(muestra.EsPieza);
    }

    // El 12 NO es número de pieza (el 12 espejo es otro mecanismo, no la muestra siendo pieza).
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(12)]
    public void LaMuestraNoEsPieza_SiSuNumeroNoEsUnaPieza(int numero)
    {
        var muestra = new Muestra(new Carta(numero, Palo.Oro));

        Assert.False(muestra.EsPieza);
    }
}
