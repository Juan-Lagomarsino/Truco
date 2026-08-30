using Bot;
using Domain;

namespace Tests;

// Paso 18a — /bot: PoliticaSimple jugando una partida completa contra sí misma (bot vs bot),
// para 1v1, 2v2, 3v3 y el modo de a 6, sin deadlock. Es el análogo de los fuzz de /core
// (DosVsDosTests.UnaPartida2v2Completa, ModoDeA6FuzzTests) pero con una política real de
// decisión en vez de "elegí la acción número pasos % cantidad".
public class PoliticaSimpleFuzzTests
{
    [Theory]
    [InlineData(2, 3)]
    [InlineData(2, 77)]
    [InlineData(2, 2024)]
    [InlineData(4, 3)]
    [InlineData(4, 77)]
    [InlineData(4, 2024)]
    [InlineData(6, 3)]
    [InlineData(6, 77)]
    [InlineData(6, 2024)]
    public void BotVsBot_TerminaConUnGanador_SinDeadlock(int cantidadJugadores, int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: cantidadJugadores);
        int pasos = 0;
        var puntosPorEquipo = new Dictionary<int, int> { [0] = 0, [1] = 0 };

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 40000, "La partida no debería tardar tanto.");

            JugadorId? conTurno = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var jugador = new JugadorId(j);
                if (Partido.AccionesLegales(e, jugador).Count > 0) { conTurno = jugador; break; }
            }
            Assert.NotNull(conTurno); // nunca hay deadlock: siempre hay alguien con una acción legal

            var accion = PoliticaSimple.Elegir(e, conTurno!.Value);
            e = Partido.Aplicar(e, accion); // Aplicar lanza si la acción fuera ilegal

            foreach (var equipo in new[] { 0, 1 })
            {
                int actual = e.Contador.Puntos(new EquipoId(equipo));
                Assert.True(actual >= puntosPorEquipo[equipo], "Los puntos no pueden decrecer.");
                puntosPorEquipo[equipo] = actual;
            }
        }

        int p0 = e.Contador.Puntos(new EquipoId(0));
        int p1 = e.Contador.Puntos(new EquipoId(1));
        Assert.True(p0 >= 30 ^ p1 >= 30); // un solo ganador
    }
}
