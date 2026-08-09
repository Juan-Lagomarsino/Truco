namespace Domain;

/// <summary>
/// Resuelve una baza: dadas las cartas que se tiraron a la mesa, dice cuál ganó.
/// RULES_Afinadas.md §"Como se resuelve la mano": gana la carta más alta; si el
/// máximo de fuerza está empatado, la baza es parda.
///
/// Alcance: resolución por carta, correcta para 1v1. El caso de equipos (dos cartas
/// máximas del mismo equipo ganan en vez de empardar) se resuelve en una capa
/// superior cuando se implemente el 2v2 (Paso 16).
/// </summary>
public static class Baza
{
    /// <summary>
    /// Resuelve la baza. Las <paramref name="jugadas"/> van en orden de juego y el
    /// resultado referencia la posición de la carta ganadora en esa lista.
    /// </summary>
    public static ResultadoBaza Resolver(IReadOnlyList<Carta> jugadas, Muestra muestra)
    {
        if (jugadas.Count == 0)
            throw new ArgumentException("Una baza necesita al menos una carta jugada.", nameof(jugadas));

        int ganador = 0;
        int mejorFuerza = Jerarquia.Fuerza(jugadas[0], muestra);
        bool maximoEmpatado = false;

        for (int i = 1; i < jugadas.Count; i++)
        {
            int fuerza = Jerarquia.Fuerza(jugadas[i], muestra);

            if (fuerza > mejorFuerza)
            {
                mejorFuerza = fuerza;
                ganador = i;
                maximoEmpatado = false;
            }
            else if (fuerza == mejorFuerza)
            {
                maximoEmpatado = true;
            }
        }

        return maximoEmpatado ? ResultadoBaza.Parda : ResultadoBaza.Gana(ganador);
    }
}
