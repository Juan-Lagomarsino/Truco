using Domain;

namespace Bot;

/// <summary>
/// Política de bot simple y honesta: decide una <see cref="Accion"/> legal mirando sólo lo
/// que ese jugador puede ver — su propia mano (actual e inicial) y el estado público de la
/// mesa. Nunca lee las cartas de otro jugador, ni siquiera las de su compañero de equipo,
/// para decidir: si el que tiene que responder un canto de equipo no es el que tiene la
/// flor o el mejor envido, decide igual con lo suyo, no con lo ajeno.
///
/// No es una IA sofisticada ni implementa ninguna regla del truco nueva: son umbrales fijos
/// sobre funciones de dominio que ya existen (<see cref="Envido.De"/>, <see cref="Flor.De"/>,
/// <see cref="Jerarquia.Fuerza"/>). Son heurísticas de juego, no reglas — se pueden cambiar
/// sin tocar /core.
/// </summary>
public static class PoliticaSimple
{
    private const int EnvidoAceptable = 27;    // umbral folklórico de "quiero" con el envido
    private const int FlorAceptable = 30;      // la flor va de 20 a 47; éste es un piso holgado
    private const int FuerzaDeCartaBuena = 11; // Fuerza = 20 - Nivel; 11 es el peor de los matas

    /// <summary>Elige una acción legal para <paramref name="jugador"/> en <paramref name="estado"/>.</summary>
    public static Accion Elegir(EstadoPartida estado, JugadorId jugador)
    {
        var legales = Partido.AccionesLegales(estado, jugador);
        if (legales.Count == 0)
            throw new InvalidOperationException($"El jugador {jugador.Valor} no tiene acciones legales.");

        if (estado.Cierre is not null)
            return Unica<Pasar>(legales); // nunca denuncia: no espía la mano del rival para justificarlo

        if (estado.HayFlorPendiente)
            return ResponderSegunTanto(legales, FlorDePropia(estado, jugador), FlorAceptable);

        // Cantar flor, si se puede, tiene prioridad sobre cualquier otra cosa: anula el
        // envido pendiente o por tocar (F1), y una política simple nunca la esconde.
        // Es legal tanto en turno libre como al responder un truco o un envido pendiente
        // (RULES_Afinadas.md: "el envido y la flor van primero").
        if (HayAlguna<CantarFlor>(legales))
            return Unica<CantarFlor>(legales);

        if (estado.HayEnvidoPendiente)
            return ResponderSegunTanto(legales, EnvidoDePropia(estado, jugador), EnvidoAceptable);

        if (estado.HayCantoPendiente)
            return ResponderTruco(estado, jugador, legales);

        return TurnoLibre(estado, jugador, legales);
    }

    private static Accion TurnoLibre(EstadoPartida estado, JugadorId jugador, IReadOnlyList<Accion> legales)
    {
        var apertura = EnvidoDePropia(estado, jugador) >= EnvidoAceptable ? MejorAperturaDeEnvido(legales) : null;
        if (apertura is not null) return apertura;

        if (TieneCartaBuena(estado, jugador) && HayAlguna<CantarTruco>(legales))
            return Unica<CantarTruco>(legales);

        return CartaMasDebil(estado, legales);
    }

    // Al responder un truco pendiente también se puede tocar envido antes de contestar (el
    // envido va primero); si no, decide con lo que tiene en la mano actual. La flor ya se
    // resolvió más arriba, en Elegir.
    private static Accion ResponderTruco(EstadoPartida estado, JugadorId jugador, IReadOnlyList<Accion> legales)
    {
        var apertura = EnvidoDePropia(estado, jugador) >= EnvidoAceptable ? MejorAperturaDeEnvido(legales) : null;
        if (apertura is not null) return apertura;

        return TieneCartaBuena(estado, jugador) ? Unica<Quiero>(legales) : Unica<NoQuiero>(legales);
    }

    // Envido y flor pendientes se contestan igual: nunca revira (siempre Quiero/NoQuiero
    // liso), sólo compara el propio tanto contra un umbral fijo.
    private static Accion ResponderSegunTanto(IReadOnlyList<Accion> legales, int propio, int aceptable)
        => propio >= aceptable ? Unica<Quiero>(legales) : Unica<NoQuiero>(legales);

    private static Accion? MejorAperturaDeEnvido(IReadOnlyList<Accion> legales)
    {
        // Conservador: pide lo mínimo disponible (Envido antes que Real, antes que Falta).
        foreach (var canto in new[] { EnvidoCanto.Envido, EnvidoCanto.RealEnvido, EnvidoCanto.FaltaEnvido })
        {
            var accion = legales.OfType<CantarEnvido>().FirstOrDefault(c => c.Canto == canto);
            if (accion is not null) return accion;
        }
        return null;
    }

    private static Accion CartaMasDebil(EstadoPartida estado, IReadOnlyList<Accion> legales)
        => legales.OfType<TirarCarta>()
            .OrderBy(t => Jerarquia.Fuerza(t.Carta, estado.Muestra))
            .First(); // guarda las cartas fuertes para más adelante

    private static bool TieneCartaBuena(EstadoPartida estado, JugadorId jugador)
        => estado.Manos[jugador.Valor].Any(c => Jerarquia.Fuerza(c, estado.Muestra) >= FuerzaDeCartaBuena);

    private static int EnvidoDePropia(EstadoPartida estado, JugadorId jugador)
    {
        var mano = estado.ManosIniciales[jugador.Valor];
        // Una mano con flor (2+ piezas) no tiene envido definido (Envido.De lanza para
        // ésas); mismo criterio que Partido.EnvidoParaComparar: no compite con ese número.
        return Flor.Hay(mano, estado.Muestra) ? int.MinValue : Envido.De(mano, estado.Muestra);
    }

    private static int FlorDePropia(EstadoPartida estado, JugadorId jugador)
    {
        var mano = estado.ManosIniciales[jugador.Valor];
        return Flor.Hay(mano, estado.Muestra) ? Flor.De(mano, estado.Muestra) : 0;
    }

    private static bool HayAlguna<T>(IReadOnlyList<Accion> legales) where T : Accion
        => legales.OfType<T>().Any();

    private static T Unica<T>(IReadOnlyList<Accion> legales) where T : Accion
        => legales.OfType<T>().First();
}
