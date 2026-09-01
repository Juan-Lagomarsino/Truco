using Bot;
using Domain;

namespace Cli;

/// <summary>
/// Consola jugable: 1 contra 1, vos (jugador 0) contra <see cref="PoliticaSimple"/>
/// (jugador 1). Toda la entrada/salida del juego vive acá — /core y /bot son puros, sin
/// Console ni ningún otro IO.
///
/// Uso: <c>dotnet run --project cli [semilla]</c> para jugar (graba la partida al
/// terminar); <c>dotnet run --project cli -- --reproducir archivo.txt</c> para reproducir
/// una grabación ya guardada sin jugarla de nuevo; <c>dotnet run --project cli -- --help</c>
/// para ver esta ayuda.
/// </summary>
public static class Program
{
    private const int Largo = 30;
    private static readonly JugadorId Humano = new(0);
    private static readonly JugadorId JugadorBot = new(1);
    private static readonly EquipoId EquipoHumano = new(0);
    private static readonly EquipoId EquipoBot = new(1);

    /// <summary>0 si terminó bien, 1 si hubo un error de uso (argumentos inválidos, archivo
    /// inexistente o con formato inválido). No lanza: los errores esperables de la línea de
    /// comandos se muestran como mensaje claro, no como stack trace.</summary>
    public static int Main(string[] args)
    {
        if (Argumentos.EsAyuda(args))
        {
            MostrarAyuda();
            return 0;
        }

        if (Argumentos.EsReproducir(args))
        {
            var (ok, ruta, error) = Argumentos.ParsearReproducir(args);
            if (!ok)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
            return Reproducir(ruta!);
        }

        return Jugar(args);
    }

    private static void MostrarAyuda()
    {
        Console.WriteLine("Truco Uruguayo — consola jugable (1 contra 1, vos contra el bot).");
        Console.WriteLine();
        Console.WriteLine("Uso:");
        Console.WriteLine("  dotnet run --project cli [semilla]        Jugar una partida nueva.");
        Console.WriteLine("                                            Sin semilla, se elige una al azar.");
        Console.WriteLine("  dotnet run --project cli -- --reproducir <archivo>");
        Console.WriteLine("                                            Reproducir una partida ya grabada.");
        Console.WriteLine("  dotnet run --project cli -- --help        Mostrar esta ayuda.");
    }

    private static int Jugar(string[] args)
    {
        var (ok, semillaParseada, error) = Argumentos.ParsearSemilla(args);
        if (!ok)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        int semilla = semillaParseada ?? Environment.TickCount;

        Console.WriteLine($"Truco Uruguayo — partida a {Largo}, semilla {semilla}.");
        Console.WriteLine();

        var estado = Partido.Nueva(largo: Largo, semilla: semilla, cantidadJugadores: 2);
        var acciones = new List<Accion>();

        while (!estado.Terminado)
        {
            MostrarEstado(estado);

            var conTurno = ConAccionesLegales(estado)
                ?? throw new InvalidOperationException("Ningún jugador tiene acciones legales: no debería pasar.");

            var accion = conTurno.Equals(Humano) ? PedirAccionAlHumano(estado) : ElegirYAnunciar(estado);
            acciones.Add(accion);
            estado = Partido.Aplicar(estado, accion);
        }

        MostrarResultadoFinal(estado);

        var grabacion = new Grabacion
        {
            Largo = Largo,
            Semilla = semilla,
            RepartidorInicial = null,
            CantidadJugadores = 2,
            Acciones = acciones,
        };
        var ruta = $"grabacion-{semilla}.txt";
        GrabacionArchivo.Escribir(grabacion, ruta);
        Console.WriteLine($"Partida grabada en {ruta} (reproducila con --reproducir {ruta}).");
        return 0;
    }

    private static int Reproducir(string ruta)
    {
        Grabacion grabacion;
        try
        {
            grabacion = GrabacionArchivo.Leer(ruta);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"No pude leer el archivo '{ruta}': {ex.Message}");
            return 1;
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine($"El archivo '{ruta}' no tiene el formato de una grabación válida: {ex.Message}");
            return 1;
        }

        Console.WriteLine($"Reproduciendo {ruta} — semilla {grabacion.Semilla}, {grabacion.Acciones.Count} acciones.");
        Console.WriteLine();

        foreach (var estado in Grabador.ReproducirPasoAPaso(grabacion))
            MostrarEstado(estado);

        var final = Grabador.Reproducir(grabacion);
        MostrarResultadoFinal(final);
        return 0;
    }

    private static JugadorId? ConAccionesLegales(EstadoPartida estado)
    {
        for (int j = 0; j < estado.CantidadJugadores; j++)
        {
            var jugador = new JugadorId(j);
            if (Partido.AccionesLegales(estado, jugador).Count > 0) return jugador;
        }
        return null;
    }

    private static Accion ElegirYAnunciar(EstadoPartida estado)
    {
        var accion = PoliticaSimple.Elegir(estado, JugadorBot);
        Console.WriteLine($"Bot: {Describir(accion)}");
        Console.WriteLine();
        return accion;
    }

    private static Accion PedirAccionAlHumano(EstadoPartida estado)
    {
        var legales = Partido.AccionesLegales(estado, Humano);
        for (int i = 0; i < legales.Count; i++)
            Console.WriteLine($"  {i + 1}) {Describir(legales[i])}");

        while (true)
        {
            Console.Write("Elegí una opción: ");
            var linea = Console.ReadLine();
            if (int.TryParse(linea, out int elegido) && elegido >= 1 && elegido <= legales.Count)
            {
                Console.WriteLine();
                return legales[elegido - 1];
            }
            Console.WriteLine("Opción inválida.");
        }
    }

    private static void MostrarEstado(EstadoPartida estado)
    {
        Console.WriteLine(
            $"Marcador — Vos: {estado.Contador.Puntos(EquipoHumano)}  Bot: {estado.Contador.Puntos(EquipoBot)}  (a {estado.Contador.Largo})");
        Console.WriteLine($"Muestra: {DescribirCarta(estado.Muestra.Carta)}");
        Console.WriteLine($"Tu mano: {string.Join("  ", estado.Manos[Humano.Valor].Select(DescribirCarta))}");

        if (estado.JugadasBaza.Count > 0)
        {
            var mesa = estado.JugadasBaza.Select(j => $"{(j.Jugador.Equals(Humano) ? "Vos" : "Bot")}={DescribirCarta(j.Carta)}");
            Console.WriteLine($"Mesa: {string.Join("  ", mesa)}");
        }

        if (estado.HayCantoPendiente)
            Console.WriteLine($"Truco cantado: {estado.TrucoPendiente} — responde {QuienResponde(estado.EquipoResponde!.Value)}.");
        if (estado.HayEnvidoPendiente)
            Console.WriteLine($"Envido cantado: {estado.EnvidoPendiente!.Ultimo} — responde {QuienResponde(estado.EnvidoPendiente.Responde)}.");
        if (estado.HayFlorPendiente)
        {
            var canto = estado.FlorPendiente!.EsContraFlorAlResto ? "Contra Flor al Resto" : "Con Flor Envido";
            Console.WriteLine($"Flor: {canto} — responde {QuienResponde(estado.FlorPendiente.Responde)}.");
        }

        Console.WriteLine();
    }

    private static string QuienResponde(EquipoId equipo) => equipo.Equals(EquipoHumano) ? "vos" : "el bot";

    private static void MostrarResultadoFinal(EstadoPartida estado)
    {
        int puntosHumano = estado.Contador.Puntos(EquipoHumano);
        int puntosBot = estado.Contador.Puntos(EquipoBot);
        Console.WriteLine(estado.Contador.Ganador.Equals(EquipoHumano)
            ? $"¡Ganaste {puntosHumano} a {puntosBot}!"
            : $"Ganó el bot {puntosBot} a {puntosHumano}.");
    }

    private static string DescribirCarta(Carta carta) => $"{carta.Numero} de {carta.Palo}";

    private static string Describir(Accion accion) => accion switch
    {
        TirarCarta t => $"Tirar {DescribirCarta(t.Carta)}",
        CantarTruco => "Cantar Truco / Retruco / Vale Cuatro",
        CantarEnvido c => $"Cantar {c.Canto}",
        CantarFlor => "¡La mía, flor!",
        CantarFlorEnvido => "Con Flor Envido",
        CantarContraFlorAlResto => "Contra Flor al Resto",
        Quiero => "Quiero",
        NoQuiero => "No quiero",
        IrseAlMazo => "Irse al mazo",
        DenunciarFlor => "Denunciar flor escondida",
        Pasar => "Pasar (no denunciar)",
        _ => accion.GetType().Name,
    };
}
