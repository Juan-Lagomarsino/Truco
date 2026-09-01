namespace Cli;

/// <summary>
/// Parseo y validación de los argumentos de línea de comandos, separado de <see cref="Program"/>
/// para que sea puro (nada de Console ni de archivos) y se pueda probar directo desde /tests.
/// No cambia el formato de grabación ni la lógica de juego: sólo decide qué hacer con lo que
/// el usuario tipeó antes de tocar /core.
/// </summary>
public static class Argumentos
{
    public static bool EsAyuda(string[] args) =>
        args.Any(a => a is "--help" or "-h");

    public static bool EsReproducir(string[] args) =>
        args.Length > 0 && args[0] == "--reproducir";

    /// <summary>Devuelve la ruta a reproducir, o un error si falta.</summary>
    public static (bool Ok, string? Ruta, string? Error) ParsearReproducir(string[] args)
    {
        if (args.Length < 2)
            return (false, null, "Falta la ruta del archivo a reproducir. Uso: --reproducir <archivo>");
        return (true, args[1], null);
    }

    /// <summary>
    /// La semilla para jugar: null si no se pidió ninguna (se elige una al azar), un valor
    /// si se pidió una válida, o un error si se pasó algo que no es un número entero. Un
    /// primer argumento que empieza con "--" no es una semilla (es una opción sin manejar acá).
    /// </summary>
    public static (bool Ok, int? Semilla, string? Error) ParsearSemilla(string[] args)
    {
        if (args.Length == 0 || args[0].StartsWith("--"))
            return (true, null, null);

        if (!int.TryParse(args[0], out int semilla))
            return (false, null, $"Semilla inválida: '{args[0]}' no es un número entero.");

        return (true, semilla, null);
    }
}
