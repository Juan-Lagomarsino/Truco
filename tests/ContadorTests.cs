using Domain;

namespace Tests;

// Paso 10 — Contador del partido: puntaje por equipo, buenas/malas y fin de partido.
// RULES_Afinadas.md §"Explicacion de la Logica del Juego": el partido se parte en dos
// mitades. Por debajo de la mitad estás en malas; al llegar a la mitad, en buenas.
public class ContadorTests
{
    private static readonly EquipoId A = new(0);
    private static readonly EquipoId B = new(1);

    [Fact]
    public void ArrancaEnCero_YEnMalas()
    {
        var c = new Contador(30);

        Assert.Equal(0, c.Puntos(A));
        Assert.True(c.EnMalas(A));
    }

    [Fact]
    public void Sumar_AcumulaLosPuntosDeEseEquipo()
    {
        var c = new Contador(30).Sumar(A, 5).Sumar(A, 3);

        Assert.Equal(8, c.Puntos(A));
        Assert.Equal(0, c.Puntos(B));
    }

    [Theory]
    [InlineData(14, true)]   // debajo de la mitad
    [InlineData(15, false)]  // justo en la mitad ya es buenas
    [InlineData(20, false)]
    public void AlLlegarALaMitad_PasaDeMalasABuenas(int puntos, bool enMalas)
    {
        var c = new Contador(30).Sumar(A, puntos);

        Assert.Equal(enMalas, c.EnMalas(A));
        Assert.Equal(!enMalas, c.EnBuenas(A));
    }

    [Fact]
    public void ElPartidoTermina_CuandoUnEquipoLlegaAlLargo()
    {
        var c = new Contador(30).Sumar(B, 30);

        Assert.True(c.Termino);
        Assert.Equal(B, c.Ganador);
    }

    [Fact]
    public void AntesDeLlegarAlLargo_NoHayGanador()
    {
        var c = new Contador(30).Sumar(A, 29);

        Assert.False(c.Termino);
        Assert.Throws<InvalidOperationException>(() => _ = c.Ganador);
    }

    [Fact]
    public void LosPuntosNoPasanDelLargo()
    {
        // Con 28 y una mano que vale 4, no se pasa de 30.
        var c = new Contador(30).Sumar(A, 28).Sumar(A, 4);

        Assert.Equal(30, c.Puntos(A));
        Assert.True(c.Termino);
    }

    [Fact]
    public void SumarPuntosNegativos_EsRechazado()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Contador(30).Sumar(A, -1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    [InlineData(25)] // impar: no se puede partir en dos mitades iguales
    public void UnLargoInvalido_EsRechazado(int largo)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Contador(largo));
    }

    [Fact]
    public void ElContador_EsInmutable_SumarDevuelveUnoNuevo()
    {
        var original = new Contador(30);
        var despues = original.Sumar(A, 10);

        Assert.Equal(0, original.Puntos(A));   // el original no cambió
        Assert.Equal(10, despues.Puntos(A));
    }
}
