using Domain;

namespace Tests;

// Paso 19 — Grabación/reproducción: semilla + lista de Acciones alcanza para reconstruir
// una partida entera, paso a paso, byte a byte. Ver docs/notas/DISENO_Grabacion.md para el
// razonamiento completo (por qué el modelo alcanza para irse al mazo, denuncias de flor y
// el modo de a 6 con pico a pico, y por qué Assert.Equal no sirve directo sobre
// EstadoPartida).
public class GrabacionFuzzTests
{
    [Theory]
    [InlineData(3, 2)]     // 1v1
    [InlineData(77, 4)]    // 2v2
    [InlineData(2024, 6)]  // modo de a 6: cubre redondilla + pico a pico
    public void GrabarYReproducir_DaElMismoEstadoPasoAPaso(int semilla, int cantidadJugadores)
    {
        const int largo = 30;

        var estado = Partido.Nueva(largo, semilla, cantidadJugadores: cantidadJugadores);
        var estadosOriginales = new List<EstadoPartida> { estado };
        var acciones = new List<Accion>();

        int pasos = 0;
        while (!estado.Terminado)
        {
            Assert.True(pasos++ < 40000, "La partida no debería tardar tanto.");

            Accion? elegida = null;
            for (int j = 0; j < estado.CantidadJugadores; j++)
            {
                var legales = Partido.AccionesLegales(estado, new JugadorId(j));
                if (legales.Count > 0) { elegida = legales[pasos % legales.Count]; break; }
            }
            Assert.NotNull(elegida); // nunca hay deadlock

            acciones.Add(elegida!);
            estado = Partido.Aplicar(estado, elegida!);
            estadosOriginales.Add(estado);
        }

        var grabacion = new Grabacion
        {
            Largo = largo,
            Semilla = semilla,
            RepartidorInicial = null,
            CantidadJugadores = cantidadJugadores,
            Acciones = acciones,
        };

        var reproducidos = Grabador.ReproducirPasoAPaso(grabacion).ToList();

        Assert.Equal(estadosOriginales.Count, reproducidos.Count);
        for (int i = 0; i < estadosOriginales.Count; i++)
            AssertEstadosIguales(estadosOriginales[i], reproducidos[i]);
    }

    // Assert.Equal(estadoA, estadoB) a secas no sirve: EstadoPartida es un record cuyo
    // Equals compara las propiedades IReadOnlyList<...> por referencia (List<T>/arrays no
    // sobreescriben Equals), y Contador es una clase sin Equals de valor. Hay que comparar
    // campo por campo, dejando que cada Assert.Equal sobre una lista haga su propia
    // comparación estructural (ahí sí, porque el tipo del llamado es la lista).
    private static void AssertEstadosIguales(EstadoPartida a, EstadoPartida b)
    {
        Assert.Equal(a.Contador.Largo, b.Contador.Largo);
        Assert.Equal(a.Contador.Puntos(new EquipoId(0)), b.Contador.Puntos(new EquipoId(0)));
        Assert.Equal(a.Contador.Puntos(new EquipoId(1)), b.Contador.Puntos(new EquipoId(1)));

        Assert.Equal(a.Semilla, b.Semilla);
        Assert.Equal(a.NumeroDeMano, b.NumeroDeMano);
        Assert.Equal(a.CantidadJugadores, b.CantidadJugadores);
        Assert.Equal(a.Repartidor, b.Repartidor);
        Assert.Equal(a.Muestra, b.Muestra);

        Assert.Equal(a.Manos.Count, b.Manos.Count);
        for (int j = 0; j < a.Manos.Count; j++)
            Assert.Equal(a.Manos[j], b.Manos[j]);
        Assert.Equal(a.ManosIniciales.Count, b.ManosIniciales.Count);
        for (int j = 0; j < a.ManosIniciales.Count; j++)
            Assert.Equal(a.ManosIniciales[j], b.ManosIniciales[j]);
        Assert.Equal(a.Activos, b.Activos);
        Assert.Equal(a.BazasGanadas, b.BazasGanadas);
        Assert.Equal(a.JugadasBaza, b.JugadasBaza);
        Assert.Equal(a.DenunciasPendientes, b.DenunciasPendientes);

        Assert.Equal(a.Fase, b.Fase);
        Assert.Equal(a.IndicePico, b.IndicePico);
        Assert.Equal(a.Abridor, b.Abridor);
        Assert.Equal(a.Turno, b.Turno);
        Assert.Equal(a.Truco, b.Truco);
        Assert.Equal(a.TrucoPendiente, b.TrucoPendiente);
        Assert.Equal(a.EquipoResponde, b.EquipoResponde);
        Assert.Equal(a.EquipoQuePuedeRevirar, b.EquipoQuePuedeRevirar);
        Assert.Equal(a.EnvidoPendiente, b.EnvidoPendiente);
        Assert.Equal(a.EnvidoJugado, b.EnvidoJugado);
        Assert.Equal(a.FlorResuelta, b.FlorResuelta);
        Assert.Equal(a.CobroFlor, b.CobroFlor);
        Assert.Equal(a.CobroEnvido, b.CobroEnvido);
        Assert.Equal(a.FlorPendiente, b.FlorPendiente);
        Assert.Equal(a.Cierre, b.Cierre);
    }
}
