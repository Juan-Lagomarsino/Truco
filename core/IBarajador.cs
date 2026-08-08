namespace Domain;

/// <summary>
/// Estrategia intercambiable para barajar. Permite elegir cómo se mezcla el mazo
/// sin que el resto del dominio lo sepa.
///
/// La única implementación que vive en /core es determinista (con semilla), porque
/// core-dominio prohíbe System.Random sin semilla inyectada: sin eso no se puede
/// reproducir un bug, grabar una partida, ni evitar que cliente y servidor diverjan.
/// Un barajado "random" se arma en la capa de aplicación generando una semilla no
/// determinista y pasándosela a un <see cref="BarajadorConSemilla"/>.
/// </summary>
public interface IBarajador
{
    /// <summary>Devuelve una nueva permutación de las cartas. No muta la entrada.</summary>
    IReadOnlyList<Carta> Barajar(IReadOnlyList<Carta> cartas);
}
