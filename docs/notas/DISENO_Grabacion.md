# Diseño — Grabación / reproducción de partidas

Estado: diseño solamente, no implementado. Este documento es el input directo
para escribir el test xUnit test-first (ítem 6 del plan nocturno): "grabar una
partida fuzz, reproducirla, assertear estado idéntico paso a paso".

Fuentes leídas: `CLAUDE.md`, `docs/PREGUNTAS_ABIERTAS.md` (decisión D3), y el
`/core` real completo: `EstadoPartida.cs`, `Accion.cs`, `Partido.cs`,
`BarajadorConSemilla.cs`, `IBarajador.cs`, `Reparto.cs`, `Mazo.cs`,
`Contador.cs`, `JugadorId.cs`, `EquipoId.cs`, `Carta.cs`, `Muestra.cs`,
`EnvidoCanto.cs`, `Jugada.cs`, `GanadorBaza.cs`, `Cobro.cs`, `EstadoEnvido.cs`,
`EstadoFlorBid.cs`, `CierrePendiente.cs`, `FaseCiclo.cs`, `NivelTruco.cs`, y
`tests/ModoDeA6FuzzTests.cs` como patrón de fuzz existente.

---

## 0. Por qué el modelo alcanza (la garantía de D3)

D3 ya decidió esto: `EstadoPartida` guarda `Semilla` (base) y `NumeroDeMano`;
cada mano se reparte con un barajador cuya semilla es
`SemillaDeMano(semilla, numeroDeMano) = semilla + numeroDeMano` (ver
`Partido.SemillaDeMano`, línea 743 de `Partido.cs`). Repartir es entonces una
función pura de `(semilla, numeroDeMano)`.

Leyendo `Partido.cs` completo: **no hay ninguna otra fuente de no-determinismo**
en el reductor. `Partido.Nueva` y `Partido.Aplicar` no usan `System.Random` sin
semilla, ni reloj, ni estado estático, ni nada externo — son funciones puras de
sus argumentos, como exige `CLAUDE.md`. Todo lo que cambia entre dos corridas
con los mismos argumentos es literalmente imposible en este código.

Se sigue que la tupla:

```
(Largo, Semilla, RepartidorInicial, CantidadJugadores, Acciones)
```

alcanza para reconstruir la partida completa, byte a byte, incluyendo el modo
de a 6 con pico a pico: `Fase`, `IndicePico` y `Activos` no se graban aparte
porque **se derivan** en cada `Aplicar` a partir de `e` (el estado previo,
reconstruido) y `Contador` (también reconstruido) — ver
`SiguienteManoDeA6`, `AvanzarPico`, `IniciarPicoInicial` en `Partido.cs`
(líneas 557–620): ninguna de esas funciones lee nada que no esté ya en el
`EstadoPartida` de entrada. Mismo argumento para `Cierre` /
`DenunciasPendientes` (ventana de denuncia de flor): se recalculan de
`FlorResuelta` + `ManosIniciales`, no son inputs externos.

Esto responde el ítem 5 (edge cases) de una vez: **no hace falta grabar nada
más que la lista de Acciones y los cuatro parámetros de `Partido.Nueva`**,
para irse al mazo a mitad de mano, denuncias de flor, o pico a pico. Ver el
detalle caso por caso en §5.

---

## 1. Qué es una Grabación

Un `record` inmutable en `/core`, con los mismos parámetros que
`Partido.Nueva(int largo, int semilla, JugadorId? repartidorInicial, int cantidadJugadores)`
más la lista de acciones aplicadas en orden:

```csharp
namespace Domain;

/// <summary>
/// Todo lo necesario para reconstruir una partida completa, byte a byte, desde
/// cero: los parámetros con que se creó (ver <see cref="Partido.Nueva"/>) y la
/// secuencia de acciones que se le aplicaron. El reparto de cada mano es
/// determinista por semilla (D3 en PREGUNTAS_ABIERTAS.md), así que no hace
/// falta grabar nada del estado intermedio.
/// </summary>
public sealed record Grabacion
{
    public required int Largo { get; init; }
    public required int Semilla { get; init; }
    public JugadorId? RepartidorInicial { get; init; }
    public required int CantidadJugadores { get; init; }
    public required IReadOnlyList<Accion> Acciones { get; init; }
}
```

Nombre de archivo propuesto: `core/Grabacion.cs`. Es un tipo de datos puro,
sin comportamiento — coherente con el resto de `/core` (`Reparto`, `Cobro`,
etc. son lo mismo: registros de datos que otro código interpreta).

---

## 2. API en `/core`: cómo se construye y se reproduce

### 2.1 Construcción: responsabilidad del caller, no de `/core`

`/core` **no** intercepta ni acumula acciones dentro de `Partido.Aplicar`. Eso
violaría la forma del reductor (`Aplicar` sólo devuelve el estado siguiente,
no un historial) y forzaría a cargar con una lista creciente en cada llamada
aunque nadie quiera grabar nada.

En cambio, grabar es trivial para cualquier loop que ya está jugando la
partida (el fuzz test existente, el bot, la consola futura): basta con
acumular en una `List<Accion>` local, al lado del estado, cada vez que se
llama a `Aplicar`:

```csharp
var acciones = new List<Accion>();
var e = Partido.Nueva(largo, semilla, repartidorInicial, cantidadJugadores);
while (!e.Terminado)
{
    var accion = /* elegir de AccionesLegales */;
    acciones.Add(accion);
    e = Partido.Aplicar(e, accion);
}

var grabacion = new Grabacion
{
    Largo = largo,
    Semilla = semilla,
    RepartidorInicial = repartidorInicial,
    CantidadJugadores = cantidadJugadores,
    Acciones = acciones,
};
```

Ese patrón ya existe casi textual en `ModoDeA6FuzzTests.cs` — sólo hace falta
agregar el `acciones.Add(elegida)` antes de `Aplicar`.

### 2.2 Reproducción: función pura en `/core`

`/core` sí ofrece la reproducción, porque es pura (un fold de `Aplicar` sobre
`Acciones`, arrancando de `Partido.Nueva`) y es exactamente el tipo de función
que ya vive en `Partido`:

```csharp
namespace Domain;

/// <summary>
/// Reconstruye una partida a partir de una <see cref="Grabacion"/>, aplicando
/// sus acciones en orden desde una partida nueva con los mismos parámetros.
/// Puro y determinista: mismo resultado siempre para la misma grabación.
/// </summary>
public static class Grabador
{
    /// <summary>El estado final, después de aplicar todas las acciones.</summary>
    public static EstadoPartida Reproducir(Grabacion g)
    {
        var estado = Partido.Nueva(g.Largo, g.Semilla, g.RepartidorInicial, g.CantidadJugadores);
        foreach (var accion in g.Acciones)
            estado = Partido.Aplicar(estado, accion);
        return estado;
    }

    /// <summary>
    /// El estado después de cada paso, incluyendo el inicial (índice 0, antes de
    /// aplicar ninguna acción). Longitud = Acciones.Count + 1. Para comparar paso a
    /// paso contra una partida jugada en vivo.
    /// </summary>
    public static IEnumerable<EstadoPartida> ReproducirPasoAPaso(Grabacion g)
    {
        var estado = Partido.Nueva(g.Largo, g.Semilla, g.RepartidorInicial, g.CantidadJugadores);
        yield return estado;
        foreach (var accion in g.Acciones)
        {
            estado = Partido.Aplicar(estado, accion);
            yield return estado;
        }
    }
}
```

Archivo propuesto: `core/Grabador.cs`. Es deliberadamente delgado — no hace
nada que `Partido` no supiera hacer ya; sólo evita que cada caller reescriba
el mismo `foreach`.

### 2.3 IO: fuera de `/core`

"Grabar a disco" y "leer de disco" **no** son responsabilidad de `/core`
(`CLAUDE.md` prohíbe archivos ahí). Eso queda para la capa de aplicación —
hoy la candidata natural es `/cli` (ítem 5 del plan nocturno, en construcción
en paralelo) o, si no encaja ahí, un proyecto mínimo futuro. Esas dos
funciones son dos líneas cada una una vez que existe el codec de texto (§3):

```csharp
// Fuera de /core, p.ej. en /cli:
public static class GrabacionArchivo
{
    public static void Escribir(Grabacion g, string ruta) =>
        File.WriteAllText(ruta, GrabacionTexto.Escribir(g));

    public static Grabacion Leer(string ruta) =>
        GrabacionTexto.Leer(File.ReadAllText(ruta));
}
```

Ninguna de las dos existe todavía porque `/cli` mismo está en construcción en
paralelo; el punto de este documento es que cuando exista, conectarlas es
trivial y no hay ambigüedad de dónde van.

---

## 3. Serialización

### 3.1 `System.Text.Json` está disponible sin agregar nada

Confirmado por inspección, no por memoria: `core.csproj` no tiene ningún
`PackageReference` y aun así el SDK instalado (`net10.0`, `~/.dotnet`) trae
`System.Text.Json.dll` en el framework compartido
(`~/.dotnet/shared/Microsoft.NETCore.App/10.0.10/System.Text.Json.dll`) y en
el pack de referencia. Usarlo no es "agregar una dependencia" en el sentido
que pide aviso en `CLAUDE.md` — ya está en la caja del SDK.

### 3.2 Pero JSON tiene un costo concreto acá: `Accion` es polimórfico

`Accion` es un `abstract record` sin propiedades, con 11 subtipos concretos
(`TirarCarta`, `CantarTruco`, `CantarEnvido`, `CantarFlor`,
`CantarFlorEnvido`, `CantarContraFlorAlResto`, `Quiero`, `NoQuiero`,
`IrseAlMazo`, `DenunciarFlor`, `Pasar`). Serializar una lista tipada como
`IReadOnlyList<Accion>` con `System.Text.Json` por default serializa cada
elemento **como `Accion`** (el tipo declarado), que no tiene propiedades: se
pierde toda la data. Para que ande hace falta polimorfismo explícito
(`[JsonDerivedType]` sobre `Accion`, disponible desde .NET 7) — 11 atributos
más, y hay que decidir si eso vive en `Accion.cs` (mezclando un atributo de
serialización en un archivo de dominio que hoy es JSON-agnóstico) o en un
tipo envoltorio aparte.

### 3.3 Recomendación: texto plano hecho a mano, no JSON

Dado el costo de §3.2 y la preferencia explícita del proyecto por código
entendido y escrito a mano antes que "rápido", propongo un formato de texto
línea por línea, sin librería:

```
Grabacion v1
largo 30
semilla 2024
cantidadJugadores 6
repartidorInicial 0
acciones 7
TirarCarta 0 3 Oro
CantarTruco 1
Quiero 0
CantarEnvido 2 Envido
NoQuiero 3
IrseAlMazo 4
Pasar 5
```

Reglas del formato:
- Primera línea: marca de versión (`Grabacion v1`), para poder cambiar el
  formato más adelante sin ambigüedad.
- Cuatro líneas `clave valor` con los parámetros de `Partido.Nueva`.
  `repartidorInicial` es `ninguno` cuando es `null` (se usa el default de
  `Partido.Nueva`, jugador 0).
- `acciones N`: cantidad de líneas que siguen, una por acción.
- Cada línea de acción: el nombre del record concreto (coincide 1:1 con los
  nombres en `Accion.cs`) seguido de sus campos separados por espacio, en el
  orden del constructor. `JugadorId` se escribe como su `Valor` (un `int`).
  `Carta` como `Numero Palo` (`Palo` es el nombre del enum: `Oro`, `Copa`,
  `Espada`, `Basto`). `EnvidoCanto` como el nombre del enum
  (`Envido`/`RealEnvido`/`FaltaEnvido`).

Encoder/decoder propuesto: `core/GrabacionTexto.cs`, `static class` con
`Escribir(Grabacion) -> string` y `Leer(string) -> Grabacion`, con un
`switch` explícito sobre el tipo concreto de cada `Accion` (para escribir) y
sobre el primer token de cada línea (para leer). Es más código a mano que
`[JsonDerivedType]`, pero cada línea es auditable sin conocer System.Text.Json
y no toca `Accion.cs`.

**Por qué esto puede vivir en `/core` sin violar la regla de "nada de IO"**:
convertir un objeto a `string` y viceversa no es entrada/salida — no toca
`Console`, archivos ni red. Es transformación de datos pura, igual que
`Carta.ToString()`. `CLAUDE.md` ya da por sentado que `EstadoPartida` es
"serializable"; este documento extiende esa idea a `Grabacion`. La I/O real
(`File.ReadAllText`/`WriteAllText`) es la única parte que queda afuera (§2.3).

**Decisión abierta, no tomada acá**: si el autor prefiere que `/core` no sepa
nada de texto/formatos y que todo el codec viva también en la capa de
aplicación (junto con el IO de archivo), es una alternativa válida — el único
costo es que entonces el codec no se puede testear en `/tests` sin que exista
antes la capa de aplicación. Marcarlo en `PREGUNTAS_PENDIENTES.md` si se
quiere confirmación antes de implementar.

---

## 4. Test concreto: grabar, reproducir, assertear paso a paso

### 4.1 El peligro que hay que esquivar: `Assert.Equal` sobre `EstadoPartida` entero puede mentir

Esto es un hallazgo concreto, no una suposición: `EstadoPartida` es un
`sealed record` cuyo `Equals` generado por el compilador compara cada
propiedad con `EqualityComparer<T>.Default` **del tipo declarado**. Para las
propiedades `IReadOnlyList<...>` (`Manos`, `ManosIniciales`, `BazasGanadas`,
`JugadasBaza`, `Activos`, `DenunciasPendientes`), el tipo en tiempo de
ejecución es `List<T>` o un array, que no sobreescribe `Equals` — así que esa
comparación es **por referencia**, no por contenido. Dos partidas jugadas por
caminos de código distintos (original vs. reproducida) nunca comparten
instancia de lista, así que `estadoA.Equals(estadoB)` da `false` aunque el
contenido sea idéntico.

Además, `Contador` es una `sealed class` **sin** `Equals` sobreescrito (no es
un `record`): comparar dos `Contador` con `==`/`Equals` es también por
referencia.

xUnit's `Assert.Equal<T>(a, b)` sólo hace comparación estructural automática
de colecciones cuando `T` (el tipo del *llamado*, no de un campo anidado) es
en sí mismo `IEnumerable`. Si se llama `Assert.Equal(estadoOriginal,
estadoReproducido)` directamente sobre dos `EstadoPartida`, xUnit cae en el
`IEquatable<EstadoPartida>` generado por el record (porque `EstadoPartida` no
es `IEnumerable`), que tiene el problema de arriba. **Conclusión: no se puede
comparar dos `EstadoPartida` con un solo `Assert.Equal`.** Hace falta un
comparador campo por campo, donde cada campo de lista se compare con su
propio `Assert.Equal` (ahí sí, el tipo del llamado es la lista, y xUnit sí
hace la comparación estructural correcta elemento a elemento).

### 4.2 Helper de comparación (vive en `/tests`, no en `/core`)

```csharp
private static void AssertEstadosIguales(EstadoPartida a, EstadoPartida b)
{
    // Contador es una clase sin Equals de valor: comparar sus puntos, no la referencia.
    Assert.Equal(a.Contador.Largo, b.Contador.Largo);
    Assert.Equal(a.Contador.Puntos(new EquipoId(0)), b.Contador.Puntos(new EquipoId(0)));
    Assert.Equal(a.Contador.Puntos(new EquipoId(1)), b.Contador.Puntos(new EquipoId(1)));

    Assert.Equal(a.Semilla, b.Semilla);
    Assert.Equal(a.NumeroDeMano, b.NumeroDeMano);
    Assert.Equal(a.CantidadJugadores, b.CantidadJugadores);
    Assert.Equal(a.Repartidor, b.Repartidor);
    Assert.Equal(a.Muestra, b.Muestra);

    // Listas: cada Assert.Equal acá sí hace comparación estructural (el tipo del
    // llamado es la lista, no EstadoPartida) — pero hay que llamarlas una por una.
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
```

(`EstadoEnvido`, `EstadoFlorBid`, `CierrePendiente`, `Cobro`, `Muestra`,
`Carta`, `JugadorId`, `EquipoId`, `NivelTruco` no tienen campos de colección
ni de clase-sin-Equals adentro, así que su `Equals`/`record` default anda
bien directo — verificado leyendo cada uno de esos archivos.)

### 4.3 El test, siguiendo el patrón de `ModoDeA6FuzzTests.cs`

```csharp
using Domain;

namespace Tests;

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
            Assert.NotNull(elegida);

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
}
```

Este test **falla antes** de implementar `Grabacion`/`Grabador` (no compila:
los tipos no existen) y **pasa después** si `Grabador.Reproducir[PasoAPaso]`
está bien implementado. Es la forma más directa de "falla antes, pasa
después" para un tipo nuevo: el propio test es la especificación de la API.

Test adicional recomendado, más barato, para el codec de texto (§3.3):

```csharp
[Theory]
[InlineData(3, 2)]
[InlineData(2024, 6)]
public void GrabacionTexto_RoundTrip_ReproduceLoMismoQueLaGrabacionOriginal(int semilla, int cantidadJugadores)
{
    // arma una Grabacion como arriba (o reusa un helper compartido),
    // la pasa por Escribir -> Leer, y compara Grabador.Reproducir de las dos
    // con AssertEstadosIguales sobre el estado final.
}
```

---

## 5. Edge cases — por qué el modelo alcanza para cada uno

| Caso | Por qué alcanza (Semilla + Acciones) |
| --- | --- |
| Irse al mazo a mitad de mano | `IrseAlMazo` es una `Accion` más en la lista, como cualquier otra. `TerminarMano`/`CerrarMano` recalculan todo desde el estado previo (ya reconstruido) y `ValorTruco(e.Truco)`; no leen nada externo. |
| Denuncia de flor (`Cierre`/`DenunciasPendientes`) | `DenunciarFlor` y `Pasar` son acciones grabadas normales. La ventana de cierre (`Cierre`, `DenunciasPendientes`) la abre `TerminarMano` calculando `Reclamadores(e)` a partir de `FlorResuelta` y `ManosIniciales`, que ya están en el estado reconstruido — no es un input aparte. |
| Modo de a 6, redondilla ↔ pico a pico | `CantidadJugadores = 6` es uno de los cuatro parámetros de `Grabacion`. `Fase`, `IndicePico`, `Activos` se derivan en `SiguienteManoDeA6`/`AvanzarPico`/`IniciarPicoInicial` sólo a partir de `e` y `Contador` (ver §0) — nunca se leen de afuera, así que se reconstruyen solos al reproducir. |
| Falta Envido = 6 / Contra Flor al Resto = 12 en el pico | `FaltaEnvido(e)` mira `e.Fase` para decidir 6 fijo; `e.Fase` ya se reconstruye como en la fila anterior. Nada que grabar aparte. |
| Corte a la mitad del ciclo de a 6 (B10) | Depende de `contador.EnBuenas(equipo)`, y `Contador` es 100% función de los puntos acreditados por acciones ya grabadas. |
| Empates de envido/flor resueltos por "el equipo mano" (B8) | `JugadorMano` es una propiedad calculada de `Repartidor`/`Fase`/`IndicePico`/`CantidadJugadores` — todos reconstruidos, no State oculto. |

No encontré ningún caso, revisando `Partido.cs` entero, donde el resultado de
`Aplicar` dependa de algo que no esté ya en `(EstadoPartida, Accion)`. Si en
el futuro se agrega algo que sí introduzca una fuente de azar fuera del
barajador (por señas con timing, por ejemplo, cuando se implementen), este
documento quedaría desactualizado y habría que revisarlo — pero hoy no existe
tal cosa en `/core`.

---

## 6. Riesgos y alternativas descartadas

### 6.1 Alternativa descartada: grabar snapshots de `EstadoPartida` en cada paso

Rechazada por tres razones concretas:

1. **No prueba lo que hay que probar.** El objetivo de este feature es
   verificar que `Partido.Aplicar` + el barajador son deterministas de punta a
   punta. Si "reproducir" significa "cargar el snapshot guardado", nunca se
   vuelve a ejecutar el reductor — el test dejaría de ejercitar exactamente el
   código que se quiere validar.
2. **Acopla el formato de grabación a la forma interna de `EstadoPartida`.**
   Cada campo nuevo que se agregue a `EstadoPartida` (y ya se agregaron varios
   a lo largo del proyecto: `Activos`, `Fase`, `IndicePico`, `Cierre`...)
   rompería retrocompatibilidad de todas las grabaciones viejas. Grabar
   `Accion` es más estable: es la misma interfaz que ya usan (o van a usar)
   Unity, SignalR y el bot — server y cliente ya están comprometidos a no
   romperla livianamente.
3. **Es más pesado sin necesidad.** Un snapshot completo repite las tres
   manos de cada jugador, la muestra, todo el historial de bazas, etc., en
   cada paso. Una acción son unos pocos bytes.

### 6.2 Riesgo real: el gotcha de `Assert.Equal` (§4.1)

Ya cubierto arriba, pero vale remarcarlo: si quien implemente el test prueba
primero `Assert.Equal(estadoA, estadoB)` a secas, va a ver un `false`
"misterioso" con estados que a simple vista son iguales. No es un bug de
`Partido` ni de `Grabador`: es la semántica de `record` + colecciones de
referencia. El helper de §4.2 existe exactamente para evitar perder tiempo
ahí.

### 6.3 Riesgo menor: parser de texto poco tolerante

El `Leer` de §3.3 va a lanzar excepciones poco amigables ante texto mal
formado (por ejemplo, un nombre de `Accion` que no exista, o un `Palo`
mal escrito). Para el uso previsto (grabaciones que genera el propio
`Escribir`, no input de usuario libre) esto es aceptable: no hace falta un
parser tolerante a errores todavía. Si en algún momento se expone edición
manual de archivos de grabación, ahí sí conviene revisar mensajes de error.

### 6.4 Decisión abierta para el autor

Si preferís que `/core` no toque siquiera texto (ver nota al final de §3.3),
es una alternativa razonable — cambia únicamente **dónde** vive
`GrabacionTexto`, no la forma de `Grabacion`/`Grabador`. No lo decidí por mi
cuenta porque toca la pregunta "qué entra en `/core`", que las skills de este
proyecto piden consultar antes de asumir.
