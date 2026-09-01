using Cli;

namespace Tests;

// Bloque C3: pruebas de extremo a extremo de Program.Main para los casos de error que tienen
// que dar un mensaje claro y un código de salida distinto de cero, no un stack trace ni (peor)
// quedarse esperando entrada del usuario. El camino de "jugar" interactivo no se prueba acá
// (necesita Console.ReadLine de un humano); sólo se cubre hasta donde valida argumentos, que es
// antes de arrancar el loop de juego.
public class CliProgramTests
{
    [Fact]
    public void Main_ConAyuda_DevuelveCero_YMuestraElUso()
    {
        var salida = new StringWriter();
        var original = Console.Out;
        Console.SetOut(salida);
        try
        {
            int codigo = Program.Main(new[] { "--help" });
            Assert.Equal(0, codigo);
            Assert.Contains("Uso:", salida.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [Fact]
    public void Main_ConSemillaInvalida_DevuelveUno_YNoArrancaAJugar()
    {
        var error = new StringWriter();
        var original = Console.Error;
        Console.SetError(error);
        try
        {
            int codigo = Program.Main(new[] { "no-es-un-numero" });
            Assert.Equal(1, codigo);
            Assert.Contains("no-es-un-numero", error.ToString());
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Fact]
    public void Main_ReproducirSinRuta_DevuelveUno_ConMensajeClaro()
    {
        var error = new StringWriter();
        var original = Console.Error;
        Console.SetError(error);
        try
        {
            int codigo = Program.Main(new[] { "--reproducir" });
            Assert.Equal(1, codigo);
            Assert.False(string.IsNullOrWhiteSpace(error.ToString()));
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Fact]
    public void Main_ReproducirArchivoInexistente_DevuelveUno_SinStackTrace()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"truco-no-existe-{Guid.NewGuid():N}.txt");
        var error = new StringWriter();
        var original = Console.Error;
        Console.SetError(error);
        try
        {
            int codigo = Program.Main(new[] { "--reproducir", ruta });
            Assert.Equal(1, codigo);
            var mensaje = error.ToString();
            Assert.False(string.IsNullOrWhiteSpace(mensaje));
            Assert.DoesNotContain("at Cli.", mensaje); // no es un stack trace crudo
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Fact]
    public void Main_ReproducirArchivoConFormatoInvalido_DevuelveUno_ConMensajeClaro()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"truco-formato-invalido-{Guid.NewGuid():N}.txt");
        File.WriteAllText(ruta, "esto no es una Grabacion v1");
        var error = new StringWriter();
        var original = Console.Error;
        Console.SetError(error);
        try
        {
            int codigo = Program.Main(new[] { "--reproducir", ruta });
            Assert.Equal(1, codigo);
            Assert.Contains(ruta, error.ToString());
        }
        finally
        {
            Console.SetError(original);
            File.Delete(ruta);
        }
    }
}
