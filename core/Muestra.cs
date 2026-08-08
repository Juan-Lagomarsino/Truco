namespace Domain;

/// <summary>
/// La carta que se saca después de repartir y define el palo especial de la mano.
/// RULES_Afinadas.md §"La Muestra": su palo es el de las piezas (2,4,5,11,10 de ese
/// palo son las cinco cartas más fuertes). La carta que salió de muestra no la tiene
/// nadie, pero igual participa de la jerarquía.
///
/// Esta es sólo la identidad y los hechos que se derivan de ella. La fuerza y los
/// tantos de cada carta se calculan aparte, tomando una Muestra como contexto.
/// </summary>
public readonly record struct Muestra
{
    /// <summary>
    /// Los números que son pieza, en orden de fuerza de mayor a menor.
    /// §"Jerarquia": Piezas = [2,4,5,11,10]. Ojo, el orden no es el numérico:
    /// el 11 le gana al 10.
    /// </summary>
    public static readonly IReadOnlyList<int> NumerosDePieza = new[] { 2, 4, 5, 11, 10 };

    public Carta Carta { get; }

    public Muestra(Carta carta)
    {
        Carta = carta;
    }

    /// <summary>El palo cuyas 2,4,5,11,10 son piezas: el palo de la muestra.</summary>
    public Palo PaloDePiezas => Carta.Palo;

    /// <summary>
    /// True si la carta que salió de muestra es ella misma una pieza (su número ∈ {2,4,5,11,10}).
    /// Decide el 12 espejo: el 12 del palo de la muestra copia a la muestra sólo cuando
    /// la muestra es pieza. §"Jerarquia".
    /// </summary>
    public bool EsPieza => EsNumeroDePieza(Carta.Numero);

    /// <summary>¿Este número es uno de los cinco que forman pieza? No depende del palo.</summary>
    public static bool EsNumeroDePieza(int numero) => NumerosDePieza.Contains(numero);
}
