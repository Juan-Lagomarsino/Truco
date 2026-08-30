using Domain;

namespace Tests;

// RULES_Afinadas.md §"Señas" — "Tabla de señas por carta". Cubre el mapeo de UNA carta a
// su seña (o a ninguna, si es mala). No cubre qué señas hacer con la mano completa de tres
// cartas: eso queda pendiente, ver docs/notas/PREGUNTAS_PENDIENTES.md.
public class SeñasTests
{
    private static readonly Muestra MuestraDeOro = new(new Carta(3, Palo.Oro)); // y = Oro, no es pieza
    private static readonly Muestra MuestraPiezaDeOro = new(new Carta(2, Palo.Oro)); // y = Oro, sí es pieza

    // --- Piezas: cada una de las 2,4,5,11,10 del palo de la muestra tiene su propia seña ---

    [Theory]
    [InlineData(2, Seña.LevantarCejas)]
    [InlineData(4, Seña.TirarBeso)]
    [InlineData(5, Seña.ArrugarNariz)]
    [InlineData(11, Seña.GuiñoDerecho)]
    [InlineData(10, Seña.GuiñoIzquierdo)]
    public void Piezas_TienenSeñaPropia(int numero, Seña esperada)
    {
        var carta = new Carta(numero, Palo.Oro);
        Assert.Equal(esperada, Señas.DeCarta(carta, MuestraDeOro));
    }

    // El 12 del palo de la muestra espeja a la muestra (sólo si la muestra es pieza) y
    // hace la seña de la pieza que copia, no la de un 12 común.
    [Fact]
    public void DoceEspejo_HaceLaSeñaDeLaPiezaQueCopia()
    {
        var doceDeOro = new Carta(12, Palo.Oro);
        Assert.Equal(Seña.LevantarCejas, Señas.DeCarta(doceDeOro, MuestraPiezaDeOro));
    }

    // Si la muestra NO es pieza, el 12 de su palo es una negra común: sin seña propia.
    [Fact]
    public void Doce_SinEspejo_EsNegraSinSeñaPropia()
    {
        var doceDeOro = new Carta(12, Palo.Oro);
        Assert.Null(Señas.DeCarta(doceDeOro, MuestraDeOro));
    }

    // --- Matas: identidad fija, no dependen de la muestra ---

    [Theory]
    [InlineData(1, Palo.Espada, Seña.MuecaDerecha)]
    [InlineData(1, Palo.Basto, Seña.MuecaDerecha)]
    [InlineData(7, Palo.Espada, Seña.MuecaIzquierda)]
    [InlineData(7, Palo.Oro, Seña.MuecaIzquierda)]
    public void Matas_TienenSeñaDePares_SinDependerDeLaMuestra(int numero, Palo palo, Seña esperada)
    {
        var carta = new Carta(numero, palo);
        // Probar con dos muestras distintas (una del mismo palo que la carta, otra no)
        // confirma que la seña de las matas no cambia con la muestra.
        Assert.Equal(esperada, Señas.DeCarta(carta, MuestraDeOro));
        Assert.Equal(esperada, Señas.DeCarta(carta, new Muestra(new Carta(6, Palo.Copa))));
    }

    // --- Chicas y falsos ---

    [Theory]
    [InlineData(Palo.Oro)]
    [InlineData(Palo.Copa)]
    [InlineData(Palo.Espada)]
    [InlineData(Palo.Basto)]
    public void El3_SiempreMuerdeElLabio_SinImportarElPalo(Palo palo)
    {
        var carta = new Carta(3, palo);
        Assert.Equal(Seña.MorderLabioInferior, Señas.DeCarta(carta, MuestraDeOro));
    }

    [Fact]
    public void El2QueNoEsPieza_AbreLaBocaLevemente()
    {
        var dosDeCopa = new Carta(2, Palo.Copa); // muestra es Oro, así que este 2 no es pieza
        Assert.Equal(Seña.BocaLevementeAbierta, Señas.DeCarta(dosDeCopa, MuestraDeOro));
    }

    // El 2 del palo de la muestra es pieza (cejas), no "2 común" (boca abierta).
    [Fact]
    public void El2DelPaloDeLaMuestra_EsPieza_NoChica()
    {
        var dosDeOro = new Carta(2, Palo.Oro);
        Assert.Equal(Seña.LevantarCejas, Señas.DeCarta(dosDeOro, MuestraDeOro));
    }

    [Theory]
    [InlineData(Palo.Oro)]
    [InlineData(Palo.Copa)]
    public void UnosFalsos_SacanLaPuntaDeLaLengua(Palo palo)
    {
        var carta = new Carta(1, palo);
        Assert.Equal(Seña.PuntaDeLaLengua, Señas.DeCarta(carta, MuestraDeOro));
    }

    // --- Blancas y negras: sin seña propia ---

    [Theory]
    [InlineData(7, Palo.Basto)]   // blanca (7 que no es espada ni oro)
    [InlineData(7, Palo.Copa)]    // blanca
    [InlineData(6, Palo.Oro)]     // blanca (6 de cualquier palo)
    [InlineData(11, Palo.Copa)]   // negra (11 que no es de la muestra)
    [InlineData(10, Palo.Copa)]   // negra (10 que no es de la muestra)
    [InlineData(4, Palo.Copa)]    // blanca (4 que no es de la muestra)
    [InlineData(5, Palo.Copa)]    // blanca (5 que no es de la muestra)
    public void BlancasYNegras_NoTienenSeñaPropia(int numero, Palo palo)
    {
        var carta = new Carta(numero, palo);
        Assert.Null(Señas.DeCarta(carta, MuestraDeOro));
    }
}
