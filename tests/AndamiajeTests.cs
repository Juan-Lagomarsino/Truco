namespace Tests;

// Sanity check del andamiaje: confirma que la suite corre y que /tests puede
// ver el ensamblado de /core. No prueba ninguna regla del juego todavía.
public class AndamiajeTests
{
    [Fact]
    public void La_suite_corre()
    {
        Assert.True(true);
    }

    [Fact]
    public void Tests_ve_el_ensamblado_de_core()
    {
        // Referencia a un tipo real de /core para que el link falle en compilación
        // si la referencia de proyecto se rompe.
        var carta = new Domain.Carta(1, Domain.Palo.Espada);
        Assert.Equal(1, carta.Numero);
    }
}
