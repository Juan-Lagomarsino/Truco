namespace Domain;

/// <summary>
/// El puntaje de un partido entre dos equipos. Inmutable: sumar puntos devuelve un
/// Contador nuevo, así el puntaje nunca decrece ni se muta por accidente.
/// RULES_Afinadas.md §"Explicacion de la Logica del Juego": el partido se parte en
/// dos mitades iguales; por debajo de la mitad estás en malas, al llegar a la mitad
/// pasás a buenas, y ganás al llegar al largo.
/// </summary>
public sealed class Contador
{
    // Puntos por equipo, indexados por EquipoId.Valor. Siempre dos equipos.
    private readonly int[] _puntos;

    /// <summary>A cuántos puntos se juega el partido.</summary>
    public int Largo { get; }

    /// <summary>Crea un contador en cero para un partido a <paramref name="largo"/> puntos.</summary>
    public Contador(int largo)
    {
        if (largo <= 0)
            throw new ArgumentOutOfRangeException(nameof(largo), largo, "El largo tiene que ser positivo.");
        if (largo % 2 != 0)
            throw new ArgumentException("El partido se parte en dos mitades iguales: el largo tiene que ser par.", nameof(largo));

        Largo = largo;
        _puntos = new int[2];
    }

    private Contador(int largo, int[] puntos)
    {
        Largo = largo;
        _puntos = puntos;
    }

    /// <summary>La mitad del partido: la frontera entre malas y buenas.</summary>
    public int Mitad => Largo / 2;

    /// <summary>Los puntos acumulados de <paramref name="equipo"/>.</summary>
    public int Puntos(EquipoId equipo) => _puntos[Indice(equipo)];

    /// <summary>
    /// Devuelve un contador nuevo con <paramref name="puntos"/> sumados a un equipo.
    /// El puntaje no pasa del largo (una mano que cruza la meta la clava en el largo).
    /// </summary>
    public Contador Sumar(EquipoId equipo, int puntos)
    {
        if (puntos < 0)
            throw new ArgumentOutOfRangeException(nameof(puntos), puntos, "No se pueden sumar puntos negativos.");

        var nuevos = (int[])_puntos.Clone();
        int i = Indice(equipo);
        nuevos[i] = Math.Min(nuevos[i] + puntos, Largo);
        return new Contador(Largo, nuevos);
    }

    /// <summary>Está en malas mientras no llegue a la mitad.</summary>
    public bool EnMalas(EquipoId equipo) => Puntos(equipo) < Mitad;

    /// <summary>Está en buenas desde que llega a la mitad.</summary>
    public bool EnBuenas(EquipoId equipo) => Puntos(equipo) >= Mitad;

    /// <summary>El partido terminó cuando un equipo llegó al largo.</summary>
    public bool Termino => _puntos.Any(p => p >= Largo);

    /// <summary>El equipo que ganó el partido. Lanza si todavía no terminó.</summary>
    public EquipoId Ganador
    {
        get
        {
            for (int i = 0; i < _puntos.Length; i++)
                if (_puntos[i] >= Largo)
                    return new EquipoId(i);

            throw new InvalidOperationException("El partido todavía no terminó.");
        }
    }

    private int Indice(EquipoId equipo)
    {
        if (equipo.Valor >= _puntos.Length)
            throw new ArgumentOutOfRangeException(nameof(equipo), equipo.Valor, "El contador es para dos equipos (0 y 1).");
        return equipo.Valor;
    }
}
