using Domain;

namespace Tests;

// Paso 1 — Identidad de una carta: número + palo, nada más.
// RULES_Afinadas.md §"Jerarquia en formato programacion": Carta = (Numero, Palo),
// con Numero ∈ N = [1,2,3,4,5,6,7,10,11,12] y Palo ∈ P = 4 palos.
// La fuerza y los tantos NO viven acá: dependen de la muestra y son otras funciones.
public class CartaTests
{
    [Theory]
    [InlineData(1, Palo.Espada)]
    [InlineData(7, Palo.Oro)]
    [InlineData(12, Palo.Copa)]
    [InlineData(10, Palo.Basto)]
    public void UnaCarta_PreservaSuNumeroYSuPalo(int numero, Palo palo)
    {
        var carta = new Carta(numero, palo);

        Assert.Equal((numero, palo), (carta.Numero, carta.Palo));
    }

    [Fact]
    public void DosCartas_ConMismoNumeroYPalo_SonIguales()
    {
        Assert.Equal(new Carta(3, Palo.Oro), new Carta(3, Palo.Oro));
    }

    [Theory]
    [InlineData(3, Palo.Oro, 3, Palo.Copa)] // mismo número, distinto palo
    [InlineData(3, Palo.Oro, 2, Palo.Oro)]  // mismo palo, distinto número
    public void DosCartas_ConDistintoNumeroOPalo_SonDistintas(int n1, Palo p1, int n2, Palo p2)
    {
        Assert.NotEqual(new Carta(n1, p1), new Carta(n2, p2));
    }

    // El mazo español no tiene 8 ni 9, ni 0, ni 13.
    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(13)]
    public void UnNumeroFueraDelMazoEspaniol_EsRechazado(int numero)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Carta(numero, Palo.Oro));
    }

    [Fact]
    public void UnPaloFueraDelEnum_EsRechazado()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Carta(1, (Palo)99));
    }
}
