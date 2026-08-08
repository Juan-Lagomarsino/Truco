namespace Domain;

public class Mazo
{
    private readonly List<Carta> _cartas = new List<Carta>();

    // Pendiente: Paso 3 del plan. Acá va la generación determinista de las 40
    // cartas (N × P, sin 8 ni 9), el sacado de la muestra y el reparto.
    // El cuerpo viejo tenía tres bugs (ver Fase 0: bucle infinito, argumentos
    // invertidos, e incluía 8 y 9) y no compilaba contra la Carta nueva, así que
    // lo dejo vacío hasta implementar el paso con su test.
    public Mazo()
    {
    }
}
