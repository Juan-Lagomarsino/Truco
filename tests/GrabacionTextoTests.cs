using Domain;

namespace Tests;

// Paso 19b — Codec de texto de una Grabacion (core/GrabacionTexto.cs): transformación de
// datos pura, sin IO (ver DECISIONES_NOCTURNAS.md D4). El IO real de archivo queda para
// una capa de aplicación (/cli).
public class GrabacionTextoTests
{
    [Fact]
    public void Escribir_UnaGrabacionChica_DaElFormatoDocumentado()
    {
        var g = new Grabacion
        {
            Largo = 30,
            Semilla = 2024,
            RepartidorInicial = null,
            CantidadJugadores = 2,
            Acciones = new Accion[]
            {
                new TirarCarta(new JugadorId(0), new Carta(3, Palo.Oro)),
                new CantarTruco(new JugadorId(1)),
                new Quiero(new JugadorId(0)),
                new CantarEnvido(new JugadorId(1), EnvidoCanto.Envido),
                new NoQuiero(new JugadorId(0)),
            },
        };

        var texto = GrabacionTexto.Escribir(g);

        Assert.Equal(
            "Grabacion v1\n" +
            "largo 30\n" +
            "semilla 2024\n" +
            "cantidadJugadores 2\n" +
            "repartidorInicial ninguno\n" +
            "acciones 5\n" +
            "TirarCarta 0 3 Oro\n" +
            "CantarTruco 1\n" +
            "Quiero 0\n" +
            "CantarEnvido 1 Envido\n" +
            "NoQuiero 0",
            texto);
    }

    [Fact]
    public void Leer_ElFormatoDocumentado_DaLaGrabacionOriginal()
    {
        const string texto =
            "Grabacion v1\n" +
            "largo 30\n" +
            "semilla 2024\n" +
            "cantidadJugadores 6\n" +
            "repartidorInicial 2\n" +
            "acciones 3\n" +
            "TirarCarta 0 3 Oro\n" +
            "CantarEnvido 1 FaltaEnvido\n" +
            "IrseAlMazo 4";

        var g = GrabacionTexto.Leer(texto);

        Assert.Equal(30, g.Largo);
        Assert.Equal(2024, g.Semilla);
        Assert.Equal(6, g.CantidadJugadores);
        Assert.Equal(new JugadorId(2), g.RepartidorInicial);
        Assert.Equal(
            new Accion[]
            {
                new TirarCarta(new JugadorId(0), new Carta(3, Palo.Oro)),
                new CantarEnvido(new JugadorId(1), EnvidoCanto.FaltaEnvido),
                new IrseAlMazo(new JugadorId(4)),
            },
            g.Acciones);
    }

    [Theory]
    [InlineData(3, 2)]
    [InlineData(77, 4)]
    [InlineData(2024, 6)]
    public void EscribirYLeer_RoundTrip_ReproduceElMismoEstadoFinal(int semilla, int cantidadJugadores)
    {
        var grabacion = JugarUnaGrabacionFuzz(semilla, cantidadJugadores);

        var releida = GrabacionTexto.Leer(GrabacionTexto.Escribir(grabacion));

        Assert.Equal(grabacion.Largo, releida.Largo);
        Assert.Equal(grabacion.Semilla, releida.Semilla);
        Assert.Equal(grabacion.CantidadJugadores, releida.CantidadJugadores);
        Assert.Equal(grabacion.RepartidorInicial, releida.RepartidorInicial);
        Assert.Equal(grabacion.Acciones, releida.Acciones); // el tipo del llamado es la lista: compara estructural

        var estadoOriginal = Grabador.Reproducir(grabacion);
        var estadoReleido = Grabador.Reproducir(releida);
        Assert.Equal(estadoOriginal.Contador.Puntos(new EquipoId(0)), estadoReleido.Contador.Puntos(new EquipoId(0)));
        Assert.Equal(estadoOriginal.Contador.Puntos(new EquipoId(1)), estadoReleido.Contador.Puntos(new EquipoId(1)));
        Assert.Equal(estadoOriginal.NumeroDeMano, estadoReleido.NumeroDeMano);
    }

    private static Grabacion JugarUnaGrabacionFuzz(int semilla, int cantidadJugadores)
    {
        const int largo = 30;
        var estado = Partido.Nueva(largo, semilla, cantidadJugadores: cantidadJugadores);
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
            Assert.NotNull(elegida);

            acciones.Add(elegida!);
            estado = Partido.Aplicar(estado, elegida!);
        }

        return new Grabacion
        {
            Largo = largo,
            Semilla = semilla,
            RepartidorInicial = null,
            CantidadJugadores = cantidadJugadores,
            Acciones = acciones,
        };
    }
}
