using Domain;

namespace Tests;

// Paso 6 — Resolución de la mano (mejor de tres bazas).
// RULES_Afinadas.md §"Como se resuelve la mano": las ocho filas de la tabla, la
// regla corta de las pardas y "todas pardas → gana el que es mano".
public class ManoTests
{
    private static readonly EquipoId A = new(0);
    private static readonly EquipoId B = new(1);

    private static GanadorBaza Gana(EquipoId e) => GanadorBaza.De(e);
    private static readonly GanadorBaza Parda = GanadorBaza.Parda;

    // --- Las ocho filas de la tabla del documento ---

    [Fact] // A | A | (no se juega) | A
    public void GanaLasDosPrimeras_GanaLaMano_YNoSeJuegaLaTercera()
    {
        var r = Mano.Resolver(new[] { Gana(A), Gana(A) }, mano: A);

        Assert.True(r.EstaDefinida);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // A | B | A | A
    public void GanaPrimeraYTercera_GanaLaMano()
    {
        var r = Mano.Resolver(new[] { Gana(A), Gana(B), Gana(A) }, mano: A);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // A | B | B | B
    public void GanaSegundaYTercera_GanaLaMano()
    {
        var r = Mano.Resolver(new[] { Gana(A), Gana(B), Gana(B) }, mano: A);
        Assert.Equal(B, r.Ganador);
    }

    [Fact] // A | B | Parda | A  (gana el que ganó la primera no parda)
    public void UnoUnoYTerceraParda_GanaElQueGanoLaPrimera()
    {
        var r = Mano.Resolver(new[] { Gana(A), Gana(B), Parda }, mano: B);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // A | Parda | (no se juega) | A
    public void GanaPrimeraYSegundaParda_GanaLaMano()
    {
        var r = Mano.Resolver(new[] { Gana(A), Parda }, mano: B);

        Assert.True(r.EstaDefinida);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // Parda | A | (no se juega) | A
    public void PrimeraPardaYGanaSegunda_GanaLaMano()
    {
        var r = Mano.Resolver(new[] { Parda, Gana(A) }, mano: B);

        Assert.True(r.EstaDefinida);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // Parda | Parda | A | A
    public void DosPardasYGanaLaTercera_GanaLaTercera()
    {
        var r = Mano.Resolver(new[] { Parda, Parda, Gana(A) }, mano: B);
        Assert.Equal(A, r.Ganador);
    }

    [Fact] // Parda | Parda | Parda | el que es mano
    public void TodasPardas_GanaElQueEsMano()
    {
        var r = Mano.Resolver(new[] { Parda, Parda, Parda }, mano: B);
        Assert.Equal(B, r.Ganador);
    }

    // --- Estados indefinidos: todavía falta jugar ---

    [Fact]
    public void ConUnaSolaBaza_LaManoNoEstaDefinida()
    {
        var r = Mano.Resolver(new[] { Gana(A) }, mano: A);
        Assert.False(r.EstaDefinida);
    }

    [Fact]
    public void UnoUnoSinParda_LaManoNoEstaDefinida_FaltaLaTercera()
    {
        var r = Mano.Resolver(new[] { Gana(A), Gana(B) }, mano: A);
        Assert.False(r.EstaDefinida);
    }

    [Fact]
    public void DosPardas_LaManoNoEstaDefinida_FaltaLaTercera()
    {
        var r = Mano.Resolver(new[] { Parda, Parda }, mano: A);
        Assert.False(r.EstaDefinida);
    }

    [Fact]
    public void UnaManoIndefinida_NoTieneGanador()
    {
        var r = Mano.Resolver(new[] { Gana(A) }, mano: A);
        Assert.Throws<InvalidOperationException>(() => _ = r.Ganador);
    }
}
