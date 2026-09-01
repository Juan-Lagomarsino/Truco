using Domain;

namespace Tests;

// Paso 8 — Cálculo del envido de una mano de 3 cartas.
// RULES_Afinadas.md §"El toque de envido" / "Como se cuenta":
//   - tres palos distintos sin pieza → la carta más alta;
//   - dos del mismo palo sin pieza → 20 + esas dos;
//   - con una pieza → la pieza + la mejor de las otras dos, sin importar el palo.
// Alcance: manos sin flor (0 o 1 pieza). Dos piezas ya es flor y no tiene envido.
public class EnvidoTests
{
    private static Carta C(int n, Palo p) => new(n, p);

    // Ejemplo del documento: muestra 3 de Oro, mano 2/7 de Oro y 5 de Copa → 37.
    // El 2 de Oro es pieza (30) y la mejor de las otras dos es el 7. Es el envido máximo.
    [Fact]
    public void ConUnaPieza_EsLaPiezaMasLaMejorDeLasOtrasDos_Ejemplo37()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(7, Palo.Oro), C(5, Palo.Copa) };

        Assert.Equal(37, Envido.De(mano, muestra));
    }

    // Ejemplo del documento: 6 y 5 de Basto más 11 de Copa, sin piezas → 20+6+5 = 31.
    [Fact]
    public void DosDelMismoPaloSinPieza_Es20MasLaSuma_Ejemplo31()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(6, Palo.Basto), C(5, Palo.Basto), C(11, Palo.Copa) };

        Assert.Equal(31, Envido.De(mano, muestra));
    }

    // Ejemplo del documento: 12 de Espada, 10 de Oro y 11 de Copa, tres palos distintos
    // y sin piezas → 0 (las tres valen 0). Muestra de Basto para que ninguna sea pieza.
    [Fact]
    public void TresPalosDistintosSinPieza_ValeLaMasAlta_Ejemplo0()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var mano = new[] { C(12, Palo.Espada), C(10, Palo.Oro), C(11, Palo.Copa) };

        Assert.Equal(0, Envido.De(mano, muestra));
    }

    [Fact]
    public void TresPalosDistintosSinPieza_ValeLaMasAlta_Caso7()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var mano = new[] { C(7, Palo.Espada), C(5, Palo.Oro), C(3, Palo.Copa) };

        Assert.Equal(7, Envido.De(mano, muestra));
    }

    // Dos del mismo palo donde una es 10/11/12 (vale 0): el 0 entra en la suma.
    [Fact]
    public void DosDelMismoPalo_ConUnaCartaQueVale0_LaSumaCuentaEse0()
    {
        var muestra = new Muestra(C(6, Palo.Basto));
        var mano = new[] { C(11, Palo.Oro), C(4, Palo.Oro), C(3, Palo.Copa) };

        Assert.Equal(24, Envido.De(mano, muestra)); // 20 + 0 + 4
    }

    // C1: el 12 espejo (cuando la muestra es pieza) cuenta como esa pieza también para el
    // envido, no sólo para la flor. Muestra 2 de Oro → el 12 de Oro vale como un 2 (30).
    [Fact]
    public void El12Espejo_CuentaComoPieza_ParaElEnvido()
    {
        var muestra = new Muestra(C(2, Palo.Oro));
        var mano = new[] { C(12, Palo.Oro), C(7, Palo.Espada), C(3, Palo.Copa) };

        Assert.Equal(37, Envido.De(mano, muestra)); // 30 (12 espejo) + 7 (la mejor de las otras)
    }

    // Con dos piezas la mano es flor: el envido no está definido.
    [Fact]
    public void ConDosPiezas_LanzaPorqueEsFlor()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(4, Palo.Oro), C(7, Palo.Copa) };

        Assert.Throws<InvalidOperationException>(() => Envido.De(mano, muestra));
    }

    [Fact]
    public void UnaManoDeEnvido_TieneExactamenteTresCartas()
    {
        var muestra = new Muestra(C(3, Palo.Oro));
        var mano = new[] { C(2, Palo.Oro), C(7, Palo.Oro) };

        Assert.Throws<ArgumentException>(() => Envido.De(mano, muestra));
    }

    // Invariante (skill): si la mano no tiene flor, su envido cae en [0, 37].
    // Las manos con flor no tienen envido, así que quedan fuera.
    [Fact]
    public void SinFlor_ElEnvidoEstaEntre0Y37()
    {
        var todas = Mazo.Completo().Cartas;
        var muestrasDePrueba = new[]
        {
            new Muestra(C(3, Palo.Oro)),   // no pieza
            new Muestra(C(2, Palo.Oro)),   // pieza (activa el 12 espejo)
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
                        if (!Flor.Hay(mano, muestra))
                            Assert.InRange(Envido.De(mano, muestra), 0, 37);
                    }
        }
    }
}
