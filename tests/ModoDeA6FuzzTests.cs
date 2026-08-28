using Domain;

namespace Tests;

// Paso 17b-4 — Fuzz del modo de a 6: una partida entera de seis jugadores, con redondillas
// (3v3) y picos a pico (tres 1v1 en secuencia) alternados, termina sin deadlock, con un solo
// equipo ganador y puntos que nunca decrecen. Es el análogo de DosVsDosTests.UnaPartida2v2-
// Completa para el modo de a 6, y ejercita end-to-end el schedule (17b-2) y los tantos del
// pico (17b-3: Falta 6, Contra Flor al Resto 12). Ver B10 en PREGUNTAS_ABIERTAS.md.
public class ModoDeA6FuzzTests
{
    private static readonly EquipoId E0 = new(0);
    private static readonly EquipoId E1 = new(1);

    [Theory]
    [InlineData(3)]
    [InlineData(77)]
    [InlineData(2024)]
    public void UnaPartidaDeA6Completa_TerminaConUnGanador(int semilla)
    {
        var e = Partido.Nueva(largo: 30, semilla: semilla, cantidadJugadores: 6);
        int pasos = 0, puntos0 = 0, puntos1 = 0;
        bool jugoAlgunPico = false;

        while (!e.Terminado)
        {
            Assert.True(pasos++ < 40000, "La partida no debería tardar tanto.");

            // Invariante del modo: en un pico juega exactamente una pareja de enfrentados
            // (dos jugadores, de equipos opuestos); en una redondilla juegan los seis.
            if (e.Fase == FaseCiclo.PicoAPico)
            {
                jugoAlgunPico = true;
                Assert.Equal(2, e.Activos.Count);
                Assert.NotEqual(e.EquipoDe(e.Activos[0]), e.EquipoDe(e.Activos[1]));
                Assert.Equal(3, Math.Abs(e.Activos[0].Valor - e.Activos[1].Valor)); // j vs j+3
            }
            else
            {
                Assert.Empty(e.Activos);
            }

            Accion? elegida = null;
            for (int j = 0; j < e.CantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(e, new JugadorId(j));
                if (legales.Count > 0) { elegida = legales[pasos % legales.Count]; break; }
            }
            Assert.NotNull(elegida); // nunca hay deadlock

            e = Partido.Aplicar(e, elegida!);

            int n0 = e.Contador.Puntos(E0), n1 = e.Contador.Puntos(E1);
            Assert.True(n0 >= puntos0 && n1 >= puntos1, "Los puntos no pueden decrecer.");
            puntos0 = n0; puntos1 = n1;
        }

        Assert.True(jugoAlgunPico, "La partida de a 6 debería atravesar al menos un pico a pico.");
        Assert.True(e.Contador.Puntos(E0) >= 30 ^ e.Contador.Puntos(E1) >= 30); // un solo ganador
    }
}
