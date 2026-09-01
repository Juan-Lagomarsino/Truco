namespace Domain;

/// <summary>
/// Los cuatro palos del mazo español. El orden es arbitrario: ninguna regla del
/// Truco depende del valor numérico del palo (las matas y la fuerza se definen
/// por tabla explícita en RULES_Afinadas.md, no por índice de palo).
/// </summary>
public enum Palo
{
    Oro,
    Copa,
    Espada,
    Basto,
}

/// <summary>
/// La identidad de una carta: número + palo, y nada más.
/// RULES_Afinadas.md §"Jerarquia en formato programacion": Carta = (Numero, Palo),
/// con Numero ∈ N = [1,2,3,4,5,6,7,10,11,12] (mazo español, sin 8 ni 9) y Palo ∈ P.
///
/// La <b>fuerza</b> (para resolver bazas) y los <b>tantos</b> (para envido y flor)
/// NO viven acá: dependen de la muestra y son funciones aparte. Ver core-dominio.
/// </summary>
public readonly record struct Carta
{
    /// <summary>El número de la carta. Pertenece a {1,2,3,4,5,6,7,10,11,12} (sin 8 ni 9).</summary>
    public int Numero { get; }

    /// <summary>El palo de la carta.</summary>
    public Palo Palo { get; }

    /// <summary>Crea la carta. Lanza si el número o el palo no existen en el mazo español.</summary>
    public Carta(int numero, Palo palo)
    {
        if (!EsNumeroDelMazo(numero))
            throw new ArgumentOutOfRangeException(
                nameof(numero), numero,
                "El mazo español solo tiene los números 1..7 y 10..12 (sin 8 ni 9).");

        if (!Enum.IsDefined(palo))
            throw new ArgumentOutOfRangeException(
                nameof(palo), palo, "Palo desconocido.");

        Numero = numero;
        Palo = palo;
    }

    private static bool EsNumeroDelMazo(int numero) =>
        numero is (>= 1 and <= 7) or (>= 10 and <= 12);
}
