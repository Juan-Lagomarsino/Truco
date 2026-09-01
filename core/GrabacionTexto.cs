namespace Domain;

/// <summary>
/// Codec de texto plano para una <see cref="Grabacion"/>: transformación de datos pura
/// (string ↔ Grabacion), sin ningún IO — leer o escribir el archivo en sí queda fuera de
/// /core (ver docs/notas/DISENO_Grabacion.md §3.3 y DECISIONES_NOCTURNAS.md D4).
///
/// Formato v1, una línea por dato, pensado para ser auditable a mano:
/// <code>
/// Grabacion v1
/// largo 30
/// semilla 2024
/// cantidadJugadores 2
/// repartidorInicial ninguno
/// acciones 2
/// TirarCarta 0 3 Oro
/// CantarTruco 1
/// </code>
/// <c>repartidorInicial</c> es <c>ninguno</c> cuando es null (usa el default de
/// <see cref="Partido.Nueva"/>). Cada línea de acción es el nombre del record concreto de
/// <see cref="Accion"/> seguido de sus campos en el orden del constructor: JugadorId como
/// su Valor, Carta como "Numero Palo", EnvidoCanto como el nombre del enum.
/// </summary>
public static class GrabacionTexto
{
    private const string Marca = "Grabacion v1";

    /// <summary>Codifica la grabación al formato de texto v1 descrito arriba.</summary>
    public static string Escribir(Grabacion g)
    {
        var lineas = new List<string>
        {
            Marca,
            $"largo {g.Largo}",
            $"semilla {g.Semilla}",
            $"cantidadJugadores {g.CantidadJugadores}",
            $"repartidorInicial {(g.RepartidorInicial is { } j ? j.Valor.ToString() : "ninguno")}",
            $"acciones {g.Acciones.Count}",
        };
        foreach (var accion in g.Acciones)
            lineas.Add(EscribirAccion(accion));

        return string.Join('\n', lineas);
    }

    /// <summary>Decodifica una Grabacion del formato de texto v1. Lanza <see cref="FormatException"/> si el texto no respeta el formato.</summary>
    public static Grabacion Leer(string texto)
    {
        var lineas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lineas.Length == 0 || lineas[0] != Marca)
            throw new FormatException($"Falta la marca de versión '{Marca}'.");

        int largo = LeerCampoInt(lineas[1], "largo");
        int semilla = LeerCampoInt(lineas[2], "semilla");
        int cantidadJugadores = LeerCampoInt(lineas[3], "cantidadJugadores");
        var repartidorTexto = LeerCampo(lineas[4], "repartidorInicial");
        JugadorId? repartidorInicial = repartidorTexto == "ninguno" ? null : new JugadorId(int.Parse(repartidorTexto));
        int cantidadAcciones = LeerCampoInt(lineas[5], "acciones");

        var acciones = new List<Accion>(cantidadAcciones);
        for (int i = 0; i < cantidadAcciones; i++)
            acciones.Add(LeerAccion(lineas[6 + i]));

        return new Grabacion
        {
            Largo = largo,
            Semilla = semilla,
            RepartidorInicial = repartidorInicial,
            CantidadJugadores = cantidadJugadores,
            Acciones = acciones,
        };
    }

    private static string EscribirAccion(Accion accion) => accion switch
    {
        TirarCarta t => $"TirarCarta {t.Jugador.Valor} {t.Carta.Numero} {t.Carta.Palo}",
        CantarTruco c => $"CantarTruco {c.Jugador.Valor}",
        CantarEnvido c => $"CantarEnvido {c.Jugador.Valor} {c.Canto}",
        CantarFlor c => $"CantarFlor {c.Jugador.Valor}",
        CantarFlorEnvido c => $"CantarFlorEnvido {c.Jugador.Valor}",
        CantarContraFlorAlResto c => $"CantarContraFlorAlResto {c.Jugador.Valor}",
        Quiero c => $"Quiero {c.Jugador.Valor}",
        NoQuiero c => $"NoQuiero {c.Jugador.Valor}",
        IrseAlMazo c => $"IrseAlMazo {c.Jugador.Valor}",
        DenunciarFlor c => $"DenunciarFlor {c.Jugador.Valor}",
        Pasar c => $"Pasar {c.Jugador.Valor}",
        _ => throw new NotSupportedException($"No sé serializar una acción de tipo {accion.GetType().Name}."),
    };

    private static Accion LeerAccion(string linea)
    {
        var partes = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var jugador = new JugadorId(int.Parse(partes[1]));
        return partes[0] switch
        {
            "TirarCarta" => new TirarCarta(jugador, new Carta(int.Parse(partes[2]), Enum.Parse<Palo>(partes[3]))),
            "CantarTruco" => new CantarTruco(jugador),
            "CantarEnvido" => new CantarEnvido(jugador, Enum.Parse<EnvidoCanto>(partes[2])),
            "CantarFlor" => new CantarFlor(jugador),
            "CantarFlorEnvido" => new CantarFlorEnvido(jugador),
            "CantarContraFlorAlResto" => new CantarContraFlorAlResto(jugador),
            "Quiero" => new Quiero(jugador),
            "NoQuiero" => new NoQuiero(jugador),
            "IrseAlMazo" => new IrseAlMazo(jugador),
            "DenunciarFlor" => new DenunciarFlor(jugador),
            "Pasar" => new Pasar(jugador),
            _ => throw new FormatException($"Acción desconocida: '{partes[0]}'."),
        };
    }

    private static string LeerCampo(string linea, string clave)
    {
        var partes = linea.Split(' ', 2);
        if (partes.Length != 2 || partes[0] != clave)
            throw new FormatException($"Esperaba una línea '{clave} <valor>', encontré '{linea}'.");
        return partes[1];
    }

    private static int LeerCampoInt(string linea, string clave) => int.Parse(LeerCampo(linea, clave));
}
