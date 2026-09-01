using Bot;
using Domain;

namespace Tests;

// Bloque C2 del plan nocturno 2: PoliticaSimple tiene que decidir mirando SÓLO lo que ese
// jugador ve (su propia mano, actual e inicial, y el estado público de la mesa) — nunca las
// cartas de otro jugador, ni siquiera las de su compañero de equipo. PoliticaSimpleTests ya
// prueba esto con un ejemplo puntual en 1v1 (sólo hay rival, no compañero); acá se generaliza
// con una propiedad de fuzz sobre partidas reales de 2v2 y de a 6 (que sí tienen compañero):
// si se reparten de nuevo las cartas de TODOS los jugadores menos el que decide (mezclando
// compañero y rivales entre sí), la decisión de ese jugador no puede cambiar.
public class PoliticaSimpleHonestidadTests
{
    [Theory]
    [InlineData(4, 3)]
    [InlineData(4, 77)]
    [InlineData(4, 2024)]
    [InlineData(6, 3)]
    [InlineData(6, 77)]
    [InlineData(6, 2024)]
    public void NuncaLeeCartasAjenas_NiDeRivalNiDeCompanero_EnUnaPartidaFuzzCompleta(
        int cantidadJugadores, int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: cantidadJugadores);
        int pasos = 0;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 40000, "La partida no debería tardar tanto.");

            JugadorId? conTurno = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var jugador = new JugadorId(j);
                if (Partido.AccionesLegales(e, jugador).Count > 0) { conTurno = jugador; break; }
            }
            Assert.NotNull(conTurno);
            var quienDecide = conTurno!.Value;

            var eConCartasAjenasMezcladas = MezclarCartasDeLosDemas(e, quienDecide);

            var decisionReal = PoliticaSimple.Elegir(e, quienDecide);
            var decisionConMezcla = PoliticaSimple.Elegir(eConCartasAjenasMezcladas, quienDecide);

            Assert.Equal(decisionReal, decisionConMezcla); // no debería importar qué tiene el resto

            e = Partido.Aplicar(e, decisionReal);
        }
    }

    // Devuelve un estado idéntico salvo que las manos (actuales e iniciales) de todos los
    // jugadores que NO son `quienDecide` quedan rotadas entre sí (compañero incluido): cada
    // uno se queda con las cartas que tenía otro. La mano de `quienDecide` no se toca.
    private static EstadoPartida MezclarCartasDeLosDemas(EstadoPartida e, JugadorId quienDecide)
    {
        var otros = Enumerable.Range(0, e.CantidadJugadores)
            .Where(j => j != quienDecide.Valor)
            .ToList();
        if (otros.Count < 2) return e; // 1v1: no hay con qué mezclar (ver PoliticaSimpleTests)

        var manos = e.Manos.ToArray();
        var manosIniciales = e.ManosIniciales.ToArray();
        for (int i = 0; i < otros.Count; i++)
        {
            int destino = otros[i];
            int origen = otros[(i + 1) % otros.Count];
            manos[destino] = e.Manos[origen];
            manosIniciales[destino] = e.ManosIniciales[origen];
        }

        return e with { Manos = manos, ManosIniciales = manosIniciales };
    }
}
