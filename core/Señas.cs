namespace Domain;

/// <summary>
/// Gestos del código de señas entre compañeros. RULES_Afinadas.md §"Señas": avisan qué
/// cartas tenés sin que te escuche el rival. Sólo tienen sentido con compañero (a 4 o a
/// 6); no cambian el estado legal del juego ni la resolución de bazas — son información,
/// no una Accion del reductor.
/// </summary>
public enum Seña
{
    LevantarCejas,
    TirarBeso,
    ArrugarNariz,
    GuiñoDerecho,
    GuiñoIzquierdo,
    MuecaDerecha,
    MuecaIzquierda,
    MorderLabioInferior,
    BocaLevementeAbierta,
    PuntaDeLaLengua,
    InflarCachetes,
    SacarDientesDeAbajo,
}

/// <summary>
/// Mapeo carta → seña. RULES_Afinadas.md §"Señas", "Tabla de señas por carta": se señea
/// la fuerza de la carta en esa mano (pieza, mata, chica o nada), no la carta física, así
/// que el resultado depende de la muestra igual que <see cref="Jerarquia"/> y <see cref="Tantos"/>.
///
/// Sólo cubre la seña de UNA carta ("la que hacés si esa es tu mejor carta"). Elegir qué
/// señas hacer con una mano completa de tres cartas (incluida "cerrar ambos ojos" cuando
/// las tres son malas, y qué pasa si las tres son buenas a la vez) es una decisión aparte
/// todavía sin cerrar — ver docs/notas/PREGUNTAS_PENDIENTES.md.
/// </summary>
public static class Señas
{
    /// <summary>
    /// La seña de esta carta dada la muestra, o null si es una carta mala (blanca o negra,
    /// sin seña propia).
    /// </summary>
    public static Seña? DeCarta(Carta carta, Muestra muestra)
    {
        int n = carta.Numero;
        Palo p = carta.Palo;
        Palo y = muestra.PaloDePiezas;

        if (p == y)
        {
            if (Muestra.EsNumeroDePieza(n))
                return SeñaDePieza(n);

            // El 12 del palo de la muestra espeja a la muestra (sólo si la muestra es
            // pieza) y hace la seña de la pieza que copia.
            if (n == 12 && muestra.EsPieza)
                return SeñaDePieza(muestra.Carta.Numero);
        }

        // Matas: identidad fija, no dependen de la muestra.
        if (n == 1 && p == Palo.Espada) return Seña.MuecaDerecha;
        if (n == 1 && p == Palo.Basto) return Seña.MuecaDerecha;
        if (n == 7 && p == Palo.Espada) return Seña.MuecaIzquierda;
        if (n == 7 && p == Palo.Oro) return Seña.MuecaIzquierda;

        // Chicas y falsos.
        if (n == 3) return Seña.MorderLabioInferior;
        if (n == 2 && p != y) return Seña.BocaLevementeAbierta;
        if (n == 1 && (p == Palo.Oro || p == Palo.Copa)) return Seña.PuntaDeLaLengua;

        // Blancas y negras: sin seña propia.
        return null;
    }

    private static Seña SeñaDePieza(int numero) => numero switch
    {
        2 => Seña.LevantarCejas,
        4 => Seña.TirarBeso,
        5 => Seña.ArrugarNariz,
        11 => Seña.GuiñoDerecho,
        10 => Seña.GuiñoIzquierdo,
        _ => throw new ArgumentOutOfRangeException(nameof(numero), numero, "No es un número de pieza."),
    };
}
