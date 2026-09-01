namespace Domain;

/// <summary>
/// Resuelve una mano a partir de los resultados de sus bazas.
/// RULES_Afinadas.md §"Como se resuelve la mano": gana el equipo que gane dos bazas;
/// la regla corta de las pardas es "si hay parda, gana el que ganó la primera baza
/// que no fue parda, y si todas son pardas, gana el que es mano".
///
/// Devuelve <see cref="ResultadoMano.Indefinida"/> mientras falte jugar: así el
/// reductor sabe si tiene que pedir otra baza o cerrar la mano.
/// </summary>
public static class Mano
{
    /// <summary>
    /// Resuelve la mano a partir de las bazas jugadas hasta ahora (a lo sumo tres).
    /// <paramref name="mano"/> es el equipo del jugador mano, para desempatar cuando todas
    /// las bazas jugadas fueron parda.
    /// </summary>
    public static ResultadoMano Resolver(IReadOnlyList<GanadorBaza> bazas, EquipoId mano)
    {
        if (bazas.Count > 3)
            throw new ArgumentException("Una mano tiene a lo sumo tres bazas.", nameof(bazas));

        var victorias = new Dictionary<EquipoId, int>();
        EquipoId? primerGanadorReal = null;
        int pardas = 0;

        foreach (var baza in bazas)
        {
            if (baza.EsParda) { pardas++; continue; }

            var equipo = baza.Equipo;
            victorias[equipo] = victorias.GetValueOrDefault(equipo) + 1;
            primerGanadorReal ??= equipo;
        }

        // Dos bazas reales ganadas: la mano ya es de ese equipo (la tercera no se juega).
        foreach (var (equipo, ganadas) in victorias)
            if (ganadas >= 2)
                return ResultadoMano.Gana(equipo);

        // Todas las bazas jugadas fueron parda.
        if (primerGanadorReal is null)
            return bazas.Count == 3
                ? ResultadoMano.Gana(mano)      // parda, parda, parda → gana el mano
                : ResultadoMano.Indefinida;     // todavía puede definirse

        // Hay un ganador real y además hubo alguna parda: el rival a lo sumo empata 1-1,
        // y la parda desempata a favor del que ganó la primera baza no parda. Ya está decidido.
        if (pardas > 0)
            return ResultadoMano.Gana(primerGanadorReal.Value);

        // Sin pardas y sin dos victorias: la mano está repartida (1-1), falta la tercera.
        return ResultadoMano.Indefinida;
    }
}
