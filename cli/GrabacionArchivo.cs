using Domain;

namespace Cli;

/// <summary>
/// El único IO de archivo de la grabación de partidas: /core sólo sabe convertir una
/// Grabacion a texto y viceversa (<see cref="GrabacionTexto"/>), nunca toca el disco. Ver
/// docs/notas/DISENO_Grabacion.md §2.3 y DECISIONES_NOCTURNAS.md D4.
/// </summary>
public static class GrabacionArchivo
{
    public static void Escribir(Grabacion g, string ruta) => File.WriteAllText(ruta, GrabacionTexto.Escribir(g));

    public static Grabacion Leer(string ruta) => GrabacionTexto.Leer(File.ReadAllText(ruta));
}
