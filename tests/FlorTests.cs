using Domain;

namespace Tests;

// Paso 9 — Flor: detección y recuento.
// RULES_Afinadas.md §"El canto de la flor".
//   Formas: tres del mismo palo; una pieza + dos del mismo palo entre ellas;
//           dos piezas + cualquier carta; tres piezas.
//   Recuento (B1, precedencia por cantidad de piezas):
//     3 piezas → mayor entera + unidades de las otras dos;
//     2 piezas → mayor entera + unidades de la segunda + tercera carta;
//     1 pieza  → pieza + suma de las otras dos;
//     3 mismo palo → 20 + suma de las tres.
//   C1: el 12 espejo cuenta como pieza para formar y contar flor.
public class FlorTests
{
    private static Carta C(int n, Palo p) => new(n, p);

    // --- Detección ---

    [Fact]
    public void TresDelMismoPalo_EsFlor()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(12, Palo.Espada) };

        Assert.True(Flor.Hay(mano, muestra));
    }

    [Fact]
    public void UnaPiezaMasDosDelMismoPalo_EsFlor()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(5, Palo.Oro), C(7, Palo.Basto), C(6, Palo.Basto) };

        Assert.True(Flor.Hay(mano, muestra));
    }

    [Fact]
    public void DosPiezas_EsFlor()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(4, Palo.Oro), C(7, Palo.Copa) };

        Assert.True(Flor.Hay(mano, muestra));
    }

    [Fact]
    public void TresPiezas_EsFlor()
    {
        var muestra = new Muestra(C(3, Palo.Copa));
        var mano = new[] { C(2, Palo.Copa), C(4, Palo.Copa), C(5, Palo.Copa) };

        Assert.True(Flor.Hay(mano, muestra));
    }

    [Fact]
    public void TresPalosDistintosSinPieza_NoEsFlor()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var mano = new[] { C(7, Palo.Espada), C(5, Palo.Oro), C(3, Palo.Copa) };

        Assert.False(Flor.Hay(mano, muestra));
    }

    [Fact]
    public void UnaPiezaConLasOtrasDeDistintoPalo_NoEsFlor()
    {
        // Una pieza pero las otras dos no comparten palo: es envido, no flor.
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(7, Palo.Copa), C(5, Palo.Basto) };

        Assert.False(Flor.Hay(mano, muestra));
    }

    // C1: el 12 espejo cuenta como pieza para FORMAR flor.
    [Fact]
    public void El12Espejo_CuentaComoPieza_ParaFormarFlor()
    {
        var muestra = new Muestra(C(2, Palo.Oro)); // muestra pieza → 12 de Oro es espejo (un 2)
        var mano = new[] { C(12, Palo.Oro), C(4, Palo.Oro), C(7, Palo.Copa) };

        Assert.True(Flor.Hay(mano, muestra)); // dos piezas: el 12 espejo y el 4 de Oro
    }

    // --- Recuento: los ejemplos del documento ---

    [Fact]
    public void TresPiezas_Ejemplo47()
    {
        var muestra = new Muestra(C(3, Palo.Copa));
        var mano = new[] { C(2, Palo.Copa), C(4, Palo.Copa), C(5, Palo.Copa) };

        Assert.Equal(47, Flor.De(mano, muestra)); // 30 + 9 + 8
    }

    [Fact]
    public void UnaPiezaMasDosDelMismoPalo_Ejemplo41()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(5, Palo.Oro), C(7, Palo.Basto), C(6, Palo.Basto) };

        Assert.Equal(41, Flor.De(mano, muestra)); // 28 + 7 + 6
    }

    [Fact]
    public void TresDelMismoPalo_Ejemplo33()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(7, Palo.Espada), C(6, Palo.Espada), C(12, Palo.Espada) };

        Assert.Equal(33, Flor.De(mano, muestra)); // 20 + 7 + 6 + 0
    }

    [Fact]
    public void FlorMinima_Es20()
    {
        // Tres del mismo palo que valen 0 (10, 11, 12 de un palo que no es la muestra).
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(10, Palo.Espada), C(11, Palo.Espada), C(12, Palo.Espada) };

        Assert.Equal(20, Flor.De(mano, muestra));
    }

    [Fact]
    public void DosPiezas_MayorEnteraMasUnidadesMasTercera()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(4, Palo.Oro), C(7, Palo.Copa) };

        Assert.Equal(46, Flor.De(mano, muestra)); // 30 + 9 + 7
    }

    // B1: con una pieza y las tres del mismo palo, gana el recuento con pieza.
    [Fact]
    public void ConUnaPiezaYTresDelMismoPalo_GanaElRecuentoConPieza_NoTresDelMismoPalo()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(6, Palo.Oro), C(7, Palo.Oro) };

        Assert.Equal(43, Flor.De(mano, muestra)); // 30 + 6 + 7, no 20+2+6+7 = 35
    }

    // C1: el 12 espejo cuenta como pieza también al CONTAR.
    [Fact]
    public void El12Espejo_CuentaComoPieza_AlContar()
    {
        var muestra = new Muestra(C(2, Palo.Oro)); // 12 de Oro espejo = un 2 (30)
        var mano = new[] { C(12, Palo.Oro), C(4, Palo.Oro), C(7, Palo.Copa) };

        Assert.Equal(46, Flor.De(mano, muestra)); // 30 + 9 + 7
    }

    [Fact]
    public void ContarUnaManoSinFlor_Lanza()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var mano = new[] { C(7, Palo.Espada), C(5, Palo.Oro), C(3, Palo.Copa) };

        Assert.Throws<InvalidOperationException>(() => Flor.De(mano, muestra));
    }

    // Invariante (skill): una flor siempre cae en [20, 47].
    [Fact]
    public void TodaFlor_EstaEntre20Y47()
    {
        var todas = Mazo.Completo().Cartas;
        var muestrasDePrueba = new[]
        {
            new Muestra(C(3, Palo.Oro)),  // no pieza
            new Muestra(C(2, Palo.Oro)),  // pieza (activa el 12 espejo)
            new Muestra(C(11, Palo.Copa)),
        };

        foreach (var muestra in muestrasDePrueba)
        {
            var repartibles = todas.Where(c => c != muestra.Carta).ToArray();
            for (int i = 0; i < repartibles.Length; i++)
                for (int j = i + 1; j < repartibles.Length; j++)
                    for (int k = j + 1; k < repartibles.Length; k++)
                    {
                        var mano = new[] { repartibles[i], repartibles[j], repartibles[k] };
                        if (Flor.Hay(mano, muestra))
                            Assert.InRange(Flor.De(mano, muestra), 20, 47);
                    }
        }
    }
}
