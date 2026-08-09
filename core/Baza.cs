namespace Domain;

/// <summary>
/// Resuelve una baza: dadas las cartas que se tiraron a la mesa con el equipo de cada
/// una, dice qué jugada ganó. RULES_Afinadas.md §"Como se resuelve la mano": gana la
/// carta más alta y su equipo se lleva la baza. Si el máximo de fuerza lo comparten
/// equipos distintos, la baza es parda; si lo comparten dos del mismo equipo, gana ese
/// equipo (no es parda). En 1v1 cada jugador es su equipo, así que se reduce a ganar la
/// carta más alta con parda por empate.
/// </summary>
public static class Baza
{
    /// <summary>
    /// Resuelve la baza. Las <paramref name="jugadas"/> van en orden de juego y el
    /// resultado referencia la posición de la jugada ganadora en esa lista.
    /// </summary>
    public static ResultadoBaza Resolver(IReadOnlyList<(Carta Carta, EquipoId Equipo)> jugadas, Muestra muestra)
    {
        if (jugadas.Count == 0)
            throw new ArgumentException("Una baza necesita al menos una carta jugada.", nameof(jugadas));

        var fuerzas = new int[jugadas.Count];
        int maxFuerza = int.MinValue;
        for (int i = 0; i < jugadas.Count; i++)
        {
            fuerzas[i] = Jerarquia.Fuerza(jugadas[i].Carta, muestra);
            if (fuerzas[i] > maxFuerza) maxFuerza = fuerzas[i];
        }

        // Entre las cartas más altas: si son todas del mismo equipo, gana el equipo; si hay
        // dos equipos distintos arriba, la baza es parda. La ganadora es la primera del máximo.
        int ganador = -1;
        EquipoId? equipoArriba = null;
        bool parda = false;
        for (int i = 0; i < jugadas.Count; i++)
        {
            if (fuerzas[i] != maxFuerza) continue;

            if (equipoArriba is null)
            {
                equipoArriba = jugadas[i].Equipo;
                ganador = i;
            }
            else if (!jugadas[i].Equipo.Equals(equipoArriba.Value))
            {
                parda = true;
            }
        }

        return parda ? ResultadoBaza.Parda : ResultadoBaza.Gana(ganador);
    }
}
