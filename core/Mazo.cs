namespace Domain;
public class Mazo
{
    private List<Carta> _cartas = new List<Carta>();

    public Mazo() // Es medio hardcoded, pero es 1 maso solo. 
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j < 13; i++)
            {
                _cartas.Add(new Carta(i, j));
            }
        }
    }
}