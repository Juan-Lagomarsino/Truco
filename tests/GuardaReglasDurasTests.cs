using System.Text.RegularExpressions;

namespace Tests;

// Guarda automática de las reglas duras de /core (ver CLAUDE.md §Reglas duras de /core).
// Escanea el código fuente real de /core buscando los patrones prohibidos. No reemplaza
// el criterio humano (es una guarda textual, no un analizador semántico), pero atrapa el
// caso común: alguien agrega System.Random sin semilla, DateTime.Now, IO, async/Task,
// estado estático mutable o using UnityEngine sin darse cuenta de que rompe el dominio.
public class GuardaReglasDurasTests
{
    private static readonly string CoreDir = ResolverCoreDir();

    private static string ResolverCoreDir([System.Runtime.CompilerServices.CallerFilePath] string aqui = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(aqui)!, "..", "core"));

    private static IReadOnlyList<string> ArchivosDeCore()
        => Directory.EnumerateFiles(CoreDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();

    public static IEnumerable<object[]> ArchivosDeCoreParaTeoria()
        => ArchivosDeCore().Select(f => new object[] { f });

    // Saca comentarios de línea y de bloque antes de escanear: la documentación explica
    // estas mismas reglas en prosa (ver IBarajador.cs) y no queremos que el texto que
    // describe la prohibición dispare la propia guarda.
    private static string SinComentarios(string codigo)
    {
        codigo = Regex.Replace(codigo, @"/\*.*?\*/", "", RegexOptions.Singleline);
        codigo = Regex.Replace(codigo, @"//.*$", "", RegexOptions.Multiline);
        return codigo;
    }

    private static readonly Regex RandomSinSemilla = new(@"new\s+Random\s*\(\s*\)", RegexOptions.Compiled);
    private static readonly Regex EstaticoMutable = new(
        @"(?<![\w.])static\s+(?!readonly\b|class\b|const\b)[\w<>\[\],\.\?]+\s+\w+\s*(;|=(?!>)|\{)",
        RegexOptions.Compiled);

    [Fact]
    public void Hay_archivos_de_core_para_revisar()
    {
        Assert.NotEmpty(ArchivosDeCore());
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_Random_no_seedeado(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        Assert.False(RandomSinSemilla.IsMatch(codigo), $"{archivo}: usa `new Random()` sin semilla inyectada");
        Assert.False(codigo.Contains("Random.Shared"), $"{archivo}: usa Random.Shared, que no es determinista");
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_DateTime_Now(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        Assert.False(codigo.Contains("DateTime.Now"), $"{archivo}: usa DateTime.Now");
        Assert.False(codigo.Contains("DateTime.UtcNow"), $"{archivo}: usa DateTime.UtcNow");
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_IO_ni_consola_ni_red(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        string[] prohibidos =
        {
            "Console.", "System.IO", "File.", "StreamReader", "StreamWriter", "Directory.",
            "System.Net", "HttpClient", "Socket", "TcpClient", "WebClient",
        };
        foreach (var token in prohibidos)
            Assert.False(codigo.Contains(token), $"{archivo}: usa `{token}` (IO/red prohibido en /core)");
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_async_ni_Task(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        Assert.False(Regex.IsMatch(codigo, @"\basync\b"), $"{archivo}: usa `async`");
        Assert.False(Regex.IsMatch(codigo, @"\bTask\b"), $"{archivo}: usa `Task`");
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_UnityEngine(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        Assert.False(codigo.Contains("UnityEngine"), $"{archivo}: referencia UnityEngine");
    }

    [Theory]
    [MemberData(nameof(ArchivosDeCoreParaTeoria))]
    public void Sin_estado_estatico_mutable(string archivo)
    {
        var codigo = SinComentarios(File.ReadAllText(archivo));
        var violaciones = EstaticoMutable.Matches(codigo).Select(m => m.Value.Trim()).ToList();
        Assert.True(violaciones.Count == 0,
            $"{archivo}: posible estado estático mutable: {string.Join(", ", violaciones)}");
    }

    // --- Tests de la guarda en sí, con snippets sintéticos ---
    // Estos prueban que los detectores distinguen el caso prohibido del caso legítimo
    // ya presente en /core (p. ej. `new Random(semilla)` o `static readonly` son válidos).

    [Theory]
    [InlineData("var r = new Random();", true)]
    [InlineData("var r = new Random(semilla);", false)]
    [InlineData("var r = new Random(42);", false)]
    public void Detector_de_Random(string snippet, bool esViolacion)
        => Assert.Equal(esViolacion, RandomSinSemilla.IsMatch(SinComentarios(snippet)));

    [Theory]
    [InlineData("private static int Contador;", true)]
    [InlineData("public static int Total = 0;", true)]
    [InlineData("private static int Total { get; set; }", true)]
    [InlineData("private static readonly int[] Numeros = { 1, 2 };", false)]
    [InlineData("public static class Tantos", false)]
    [InlineData("public static int De(Carta carta, Muestra muestra)", false)]
    [InlineData("private static bool EsPieza(int v) => v > 7;", false)]
    public void Detector_de_estatico_mutable(string snippet, bool esViolacion)
        => Assert.Equal(esViolacion, EstaticoMutable.IsMatch(SinComentarios(snippet)));

    [Theory]
    [InlineData("// esto no es DateTime.Now, es un comentario", false)]
    [InlineData("var ahora = DateTime.Now;", true)]
    [InlineData("var ahora = DateTime.UtcNow;", true)]
    public void Detector_de_DateTime_ignora_comentarios(string snippet, bool esViolacion)
    {
        var codigo = SinComentarios(snippet);
        var tieneViolacion = codigo.Contains("DateTime.Now") || codigo.Contains("DateTime.UtcNow");
        Assert.Equal(esViolacion, tieneViolacion);
    }
}
