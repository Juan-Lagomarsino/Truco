namespace Domain;

public class Carta
{
    private int _numeroCarta;
    private int _paloCarta; // Palos 0,1,2,3 Son 0-Oro, 1-Copa, 2-Espada, 3-Basto

    public Carta(int numeroCarta, int paloCarta)
    {
        _numeroCarta = numeroCarta;
        _paloCarta = paloCarta;
    }

    public int NumeroCarta => _numeroCarta;

    public int PaloCarta => _paloCarta;
}


