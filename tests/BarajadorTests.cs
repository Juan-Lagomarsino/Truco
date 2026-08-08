using Domain;

namespace Tests;

// Paso 3 — Barajado determinista. core-dominio: nada de System.Random sin semilla
// inyectada. La semilla hace el barajado reproducible (para grabar partidas,
// reproducir bugs y sincronizar cliente/servidor).
public class BarajadorTests
{
    private static IReadOnlyList<Carta> Ordenadas(IEnumerable<Carta> cartas) =>
        cartas.OrderBy(c => c.Palo).ThenBy(c => c.Numero).ToList();

    [Fact]
    public void MismaSemilla_ProduceElMismoBarajado()
    {
        var cartas = Mazo.Completo().Cartas;

        var a = new BarajadorConSemilla(42).Barajar(cartas);
        var b = new BarajadorConSemilla(42).Barajar(cartas);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Barajar_EsUnaPermutacion_NiPierdeNiDuplica()
    {
        var cartas = Mazo.Completo().Cartas;

        var barajadas = new BarajadorConSemilla(7).Barajar(cartas);

        Assert.Equal(Ordenadas(cartas), Ordenadas(barajadas));
    }
}
