namespace Domain;

/// <summary>
/// Barajador determinista. Dada una semilla, produce siempre la misma secuencia de
/// barajados. Cada llamada a <see cref="Barajar"/> avanza el generador, así manos
/// sucesivas salen distintas pero reproducibles: para repetir una partida entera
/// alcanza con la misma semilla y la misma secuencia de repartos.
/// </summary>
public sealed class BarajadorConSemilla : IBarajador
{
    private readonly Random _rng;

    /// <summary>La semilla con la que se creó este barajador.</summary>
    public int Semilla { get; }

    /// <summary>Crea un barajador determinista a partir de <paramref name="semilla"/>.</summary>
    public BarajadorConSemilla(int semilla)
    {
        Semilla = semilla;
        _rng = new Random(semilla);
    }

    /// <summary>Devuelve una permutación de <paramref name="cartas"/>. No muta la entrada.</summary>
    public IReadOnlyList<Carta> Barajar(IReadOnlyList<Carta> cartas)
    {
        // Fisher-Yates: barajado uniforme e in-place sobre una copia.
        var barajadas = cartas.ToArray();
        for (int i = barajadas.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (barajadas[i], barajadas[j]) = (barajadas[j], barajadas[i]);
        }
        return barajadas;
    }
}
