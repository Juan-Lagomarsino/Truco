using Cli;

namespace Tests;

// Bloque C3 del plan nocturno 2: mejoras de UX no invasivas en /cli — --help, validación de
// argumentos, mensajes de error claros para semilla o archivo de reproducción inválidos. No
// cambia el formato de grabación ni la lógica de juego (eso sigue siendo 100% de /core).
// Argumentos es puro (sin Console ni archivos), así que se prueba directo, sin redirigir IO.
public class CliArgumentosTests
{
    [Fact]
    public void EsAyuda_ReconoceElFlagEnCualquierPosicion()
    {
        Assert.True(Argumentos.EsAyuda(new[] { "--help" }));
        Assert.True(Argumentos.EsAyuda(new[] { "-h" }));
        Assert.True(Argumentos.EsAyuda(new[] { "42", "--help" }));
    }

    [Fact]
    public void EsAyuda_FalseSinElFlag()
    {
        Assert.False(Argumentos.EsAyuda(new[] { "42" }));
        Assert.False(Argumentos.EsAyuda(Array.Empty<string>()));
    }

    [Fact]
    public void ParsearSemilla_SinArgumentos_NoEligeSemilla_YNoEsError()
    {
        var (ok, semilla, error) = Argumentos.ParsearSemilla(Array.Empty<string>());

        Assert.True(ok);
        Assert.Null(semilla);
        Assert.Null(error);
    }

    [Fact]
    public void ParsearSemilla_ConUnEnteroValido_LoDevuelve()
    {
        var (ok, semilla, error) = Argumentos.ParsearSemilla(new[] { "2024" });

        Assert.True(ok);
        Assert.Equal(2024, semilla);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("3.14")]
    [InlineData("")]
    public void ParsearSemilla_ConAlgoQueNoEsEntero_EsErrorConMensajeClaro(string invalido)
    {
        var (ok, semilla, error) = Argumentos.ParsearSemilla(new[] { invalido });

        Assert.False(ok);
        Assert.Null(semilla);
        Assert.NotNull(error);
        Assert.Contains(invalido, error);
    }

    [Fact]
    public void ParsearSemilla_UnaOpcionQueEmpiezaConGuionGuion_NoSeTomaComoSemilla()
    {
        // "--reproducir" u otra opción futura no debería intentar parsearse como número.
        var (ok, semilla, error) = Argumentos.ParsearSemilla(new[] { "--reproducir", "archivo.txt" });

        Assert.True(ok);
        Assert.Null(semilla);
        Assert.Null(error);
    }

    [Fact]
    public void EsReproducir_ReconoceElPrimerArgumento()
    {
        Assert.True(Argumentos.EsReproducir(new[] { "--reproducir", "archivo.txt" }));
        Assert.False(Argumentos.EsReproducir(new[] { "42" }));
        Assert.False(Argumentos.EsReproducir(Array.Empty<string>()));
    }

    [Fact]
    public void ParsearReproducir_ConRuta_LaDevuelve()
    {
        var (ok, ruta, error) = Argumentos.ParsearReproducir(new[] { "--reproducir", "grabacion-1.txt" });

        Assert.True(ok);
        Assert.Equal("grabacion-1.txt", ruta);
        Assert.Null(error);
    }

    [Fact]
    public void ParsearReproducir_SinRuta_EsErrorConMensajeClaro()
    {
        var (ok, ruta, error) = Argumentos.ParsearReproducir(new[] { "--reproducir" });

        Assert.False(ok);
        Assert.Null(ruta);
        Assert.NotNull(error);
    }
}
