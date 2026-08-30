namespace Domain;

/// <summary>
/// Reconstruye una partida a partir de una <see cref="Grabacion"/>, aplicando sus acciones
/// en orden desde una partida nueva con los mismos parámetros. Puro y determinista: mismo
/// resultado siempre para la misma grabación. Deliberadamente delgado — no hace nada que
/// <see cref="Partido"/> no supiera hacer ya, sólo evita que cada caller reescriba el mismo
/// fold.
/// </summary>
public static class Grabador
{
    /// <summary>El estado final, después de aplicar todas las acciones de la grabación.</summary>
    public static EstadoPartida Reproducir(Grabacion g)
    {
        var estado = Partido.Nueva(g.Largo, g.Semilla, g.RepartidorInicial, g.CantidadJugadores);
        foreach (var accion in g.Acciones)
            estado = Partido.Aplicar(estado, accion);
        return estado;
    }

    /// <summary>
    /// El estado después de cada paso, incluido el inicial (antes de aplicar ninguna
    /// acción). Longitud = Acciones.Count + 1. Sirve para comparar paso a paso contra una
    /// partida jugada en vivo.
    /// </summary>
    public static IEnumerable<EstadoPartida> ReproducirPasoAPaso(Grabacion g)
    {
        var estado = Partido.Nueva(g.Largo, g.Semilla, g.RepartidorInicial, g.CantidadJugadores);
        yield return estado;
        foreach (var accion in g.Acciones)
        {
            estado = Partido.Aplicar(estado, accion);
            yield return estado;
        }
    }
}
