# Plan — capa de presentación en Unity (Fase 4)

Plan de scaffolding para cuando se autorice tocar `/game`. Nada de lo que sigue se
ejecuta ahora: es el mapa para no improvisar cuando llegue el momento. Está escrito
contra la API real de `/core` tal como existe hoy (post 17b-4, ~206+ tests en verde):
`EstadoPartida` (record inmutable), `Partido.AccionesLegales(EstadoPartida, JugadorId)`,
`Partido.Aplicar(EstadoPartida, Accion)`, y las acciones concretas de `Accion.cs`
(`TirarCarta`, `CantarTruco`, `CantarEnvido`, `CantarFlor`, `CantarFlorEnvido`,
`CantarContraFlorAlResto`, `Quiero`, `NoQuiero`, `IrseAlMazo`, `DenunciarFlor`, `Pasar`).

---

## 1. Enfoque general

Unity es una capa de presentación pura: dibuja `EstadoPartida` y manda `Accion`. Cero
lógica de reglas duplicada en C# de Unity. La regla operativa, sin excepciones:

> Si un `MonoBehaviour` necesita saber "¿puedo hacer X ahora?", la respuesta sale de
> `Partido.AccionesLegales(estado, jugador)` filtrada por tipo. Nunca de una condición
> local hardcodeada (`if (trucoCantado) ocultarBotón`).

Esto es lo mismo que ya dice `CLAUDE.md` sobre el reductor, aplicado a Unity: Unity
sólo dibuja estado y manda acciones, para que el bot (función de `AccionesLegales` a
`Accion`) y el futuro servidor (SignalR reenviando las mismas acciones) no dupliquen
nada. Un botón de "Retruco" que aparece porque Unity contó cantos por su cuenta, en vez
de porque `AccionesLegales` devolvió un `CantarTruco`, es exactamente el tipo de bug
que este proyecto existe para evitar (ver advertencia de `CLAUDE.md` sobre reglas mal
recordadas: acá el riesgo análogo es "UI mal recordada").

`EstadoPartida` ya expone lo necesario para pintar sin recalcular nada: `Muestra`,
`Manos`/`ManosIniciales`, `JugadasBaza`, `BazasGanadas`, `Turno`, `JugadorMano`,
`Truco`/`TrucoPendiente`, `EnvidoPendiente`, `FlorPendiente`, `Cierre` +
`DenunciasPendientes`, `Contador`. Todo lo que la UI necesita mostrar tiene un campo
correspondiente; si algún día hace falta un dato que no está, es señal de que falta en
el dominio, no que Unity lo debe inferir.

---

## 2. Cómo se linkea `/core`

`/core` (proyecto `core.csproj`, `TargetFramework net10.0`, `AssemblyName Core`,
`RootNamespace Domain`) no puede tener `using UnityEngine` ni `async`/`Task` (regla
dura de `CLAUDE.md`). Dos formas de traerlo a Unity:

### Opción A — DLL precompilada (recomendada)

Compilar `/core` fuera de Unity (`dotnet build -c Release`) y copiar `Core.dll` (+
`Core.pdb` para poder debuggear) a `Assets/Plugins/Core/` como plugin administrado.
Unity no recompila ese código nunca; sólo lo referencia.

- **Pros:** la frontera dominio/motor queda forzada por el toolchain, no por
  disciplina — es físicamente imposible que alguien agregue `using UnityEngine` adentro
  de un ensamblado ya compilado. Los tests de `/tests` siguen siendo la fuente de
  verdad y corren contra el mismo binario que se shipea. Domain reload de Unity más
  rápido (no recompila el dominio en cada play). Se puede versionar el DLL como
  artefacto de CI.
- **Contras:** hace falta un paso de build manual o un script (`build-core-plugin.sh`)
  que compile y copie después de cada cambio en `/core`; si alguien olvida correrlo,
  Unity juega contra una versión vieja del dominio sin avisar. Breakpoints requieren
  que el `.pdb` esté sincronizado con el `.dll`.

### Opción B — link de fuente

Meter los `.cs` de `/core` (o un symlink a la carpeta) dentro de un `asmdef` propio en
`Assets/Scripts/Domain/`, con **cero** referencias a `UnityEngine` en ese asmdef (se
puede desmarcar explícitamente en el asmdef). Unity los compila como parte del
proyecto.

- **Pros:** un solo lugar de verdad para el código fuente, sin paso de build extra;
  cambios en `/core` se ven al toque; debugging directo con breakpoints en el editor.
- **Contras:** nada impide *técnicamente* que alguien agregue una referencia a
  `UnityEngine` al asmdef más adelante (la frontera es una convención de configuración,
  no un hecho del binario); depende de que el guard de reglas duras (paso 2 del plan
  nocturno: test/analizador que falla si aparece `UnityEngine`/`async`/`Random` sin
  semilla en `/core`) seguirá corriendo sobre la carpeta de origen y no sobre una
  copia; y **el compilador de C# de Unity tiene que soportar el mismo `LangVersion`
  que usa `/core`** (records, `required` members) — a confirmar con la versión de
  Unity elegida (ver §7).

**Recomendación:** Opción A (DLL) para el link real que se shipea, con Opción B como
modo de desarrollo local opcional detrás de un `#if UNITY_EDITOR` o de un asmdef
separado que no se shipea — pero esto es justamente el tipo de decisión de tooling que
conviene confirmar cuando se abra la Fase 4, no adivinar ahora (ver §7).

---

## 3. Estructura de carpetas dentro de `/game`

A crear recién cuando se autorice (hoy `/game` es NO TOCAR):

```
game/
  Assets/
    Plugins/
      Core/
        Core.dll          # Opción A: binario compilado de /core
        Core.pdb
    Scripts/
      Domain/              # sólo si se opta por Opción B en vez de/además de Plugins/Core
      GameLoop/
        PartidaController.cs   # dueño del EstadoPartida actual; único que llama Partido.Aplicar
        BotRunner.cs            # invoca /bot cuando le toca al bot (coroutine, delay artificial)
      Render/
        MesaView.cs
        CartaView.cs
        MuestraView.cs
        ManoView.cs
        SenaView.cs             # capa cosmética, ver §4
        CantoHud.cs             # botones de Envido/Flor/Truco
        MarcadorView.cs         # Contador (malas/buenas, tantos)
      Input/
        SeleccionCartaInput.cs  # click de carta -> candidata TirarCarta
        CantoInput.cs           # click de botón -> candidata CantarX/Quiero/NoQuiero/...
        AccionValidator.cs      # único punto que compara contra AccionesLegales
      Mapeo/
        CartaSpriteMap.cs       # (Numero, Palo) -> Sprite, tabla generada a mano una vez
    Scenes/
      Mesa1v1.unity
    Prefabs/
      Carta.prefab
      Mesa.prefab
      BotonCanto.prefab
    Art/
      Placeholder/             # arte geométrico/texto hasta tener arte final
```

Separación estricta en tres capas dentro de `Scripts/`:

- **Render** sólo lee `EstadoPartida` y dibuja. No genera `Accion`, no decide nada.
- **Input** sólo traduce gestos del jugador humano en *candidatas* a `Accion` y las
  valida contra `AccionesLegales` antes de mandarlas. No dibuja nada.
- **GameLoop** es el único que tiene un `EstadoPartida` en memoria, el único que llama
  `Partido.Aplicar`, y el que notifica a Render cuando cambia (evento discreto, no
  polling por frame — el juego es por turnos).

---

## 4. Render

- **Cartas:** `CartaView` con sprite armado desde `(Carta.Numero, Carta.Palo)` vía
  `CartaSpriteMap` (atlas de 40 cartas + dorso). El valor `Carta` es un
  `readonly record struct`, así que la key del mapeo es trivial (`Equals`/`GetHashCode`
  ya vienen bien por ser record struct).
- **Muestra:** carta fija boca arriba (`EstadoPartida.Muestra.Carta`). Opcionalmente,
  un HUD de referencia que liste qué 2/4/5/11/10 son piezas esta mano
  (`Muestra.PaloDePiezas` + `Muestra.NumerosDePieza`), útil para un jugador nuevo.
- **Piezas en la propia mano:** resaltar (glow/badge) las cartas del jugador humano
  cuyo `Tantos.De(carta, muestra) > 7` (ese es, de hecho, el mismo umbral que usa
  `/core` internamente para distinguir pieza de no-pieza en `Envido`/`Flor`) o,
  alternativamente, ordenar visualmente la mano por `Jerarquia.Fuerza(carta, muestra)`
  para que el jugador vea de un vistazo cuál es su carta más fuerte. Ambas son
  funciones públicas y puras de `/core`: Unity las llama, no las reimplementa.
- **Mano / Pie:** indicador visual sobre el asiento de `EstadoPartida.JugadorMano`; el
  "pie" es el jugador anterior a él en la mesa (último en recibir cartas), se puede
  derivar del mismo dato sin pedirle nada nuevo a `/core`.
- **Baza en curso / bazas ganadas:** `MesaView` dibuja `JugadasBaza` (cartas ya tiradas
  esta baza, en orden) y un resumen de `BazasGanadas` (quién ganó cada una, o parda)
  para que se vea el historial de la mano.
- **Cantos como UI:** un banner que muestra el canto pendiente (`TrucoPendiente`,
  `EnvidoPendiente.Ultimo`, `FlorPendiente`) y a qué equipo le toca responder
  (`EquipoResponde` / `EnvidoPendiente.Responde` / `FlorPendiente.Responde`). El HUD de
  envido/flor propio del jugador humano (para que sepa qué está por cantar) se arma
  llamando `Envido.De(manoInicial, muestra)` / `Flor.Hay`+`Flor.De`, no reinventando la
  cuenta en Unity.
- **Señas (visual):** ver el matiz importante en §7 — hoy `Accion.cs` no tiene ninguna
  acción de seña. Las señas, según `RULES_Afinadas.md` §"Señas", "no cambian el estado
  legal de la mano ni la resolución de las bazas": son información entre compañeros,
  no una jugada del reductor. Esto se renderiza como una capa **puramente cosmética**:
  el jugador humano elige animar un gesto en su propio avatar (UI local, no pasa por
  `AccionesLegales`/`Aplicar`), y el avatar del compañero reproduce esa animación. No
  hay tipo de dominio que leer ni validar. Implementar la tabla de gestos en sí queda
  fuera de este plan (la skill del proyecto marca explícitamente "no implementes
  señas" hasta que `SEÑAS.md` esté cerrado del todo para `/core`); acá sólo se deja
  el gancho de UI.

---

## 5. Input

Flujo único, sin atajos:

1. El jugador humano hace click en una carta de su mano o en un botón de canto.
2. Eso arma una `Accion` *candidata* (`TirarCarta(jugadorHumano, carta)`,
   `CantarEnvido(jugadorHumano, EnvidoCanto.Envido)`, `Quiero(jugadorHumano)`, etc.).
3. `AccionValidator` pide `Partido.AccionesLegales(estadoActual, jugadorHumano)` y
   comprueba que la candidata está en esa lista (comparación por `Equals`, gratis por
   ser todas `record`/`record struct`).
4. Si está: se manda al `GameLoop`, que llama `Partido.Aplicar` y propaga el nuevo
   estado a Render. Si no está: se ignora el click (defensivo — la UI debería haber
   deshabilitado ese botón/carta de entrada, pero la fuente de verdad es siempre el
   validador, nunca el estado visual del botón).

La UI puede (y debe, por UX) deshabilitar visualmente botones/cartas no jugables de
antemano — pero eso es una optimización de UX, no la razón por la que algo es o no
legal. Concretamente: en cada cambio de estado, `CantoHud` reconstruye su lista de
botones filtrando `AccionesLegales(estado, jugadorHumano)` por tipo (`CantarTruco`,
`CantarEnvido`, `CantarFlor`, `CantarFlorEnvido`, `CantarContraFlorAlResto`, `Quiero`,
`NoQuiero`, `IrseAlMazo`, `DenunciarFlor`, `Pasar`) — nunca mantiene su propio booleano
tipo `puedeCantarRetruco`.

---

## 6. Loop de juego 1v1 humano vs bot

Depende de `/bot`, que todavía no existe (es el paso 4 del plan nocturno en curso: una
librería nueva que depende de `/core` y expone una función pura
`Accion Elegir(EstadoPartida, JugadorId)`, sin IO ni azar sin semilla, con la misma
disciplina de `/core`). Este plan asume esa firma; quien construya `/bot` es quien la
fija de verdad.

`PartidaController` (el único dueño del `EstadoPartida` en memoria):

1. Arranca con `Partido.Nueva(largo, semilla, repartidorInicial, cantidadJugadores: 2)`.
2. En cada cambio de estado, mira de quién es el turno / quién debe responder
   (`Turno`, `EquipoResponde`, `EnvidoPendiente.Responde`, `FlorPendiente.Responde`,
   `DenunciasPendientes`).
   - Si es el jugador humano: espera Input (§5).
   - Si es el bot: llama `Accion accion = Bot.Elegir(estado, jugadorBot)` y aplica
     `Partido.Aplicar(estado, accion)` directamente — es una función pura y rápida, no
     hace falta ni corrutina ni hilo aparte para la llamada en sí.
3. El **delay artificial** para que el bot no se sienta instantáneo/robótico
   (`yield return new WaitForSeconds(...)`) vive en `BotRunner`, **en Unity**, nunca
   en `/bot` ni en `/core` — la regla dura de "sin async/Task, determinista" sigue
   aplicando al bot igual que al dominio (mismo `CLAUDE.md`), así que cualquier timing
   o randomness de UX es responsabilidad exclusiva de la capa de presentación.
4. Tras cada `Aplicar` (humano o bot), `PartidaController` dispara un evento con el
   `EstadoPartida` nuevo; todas las vistas de Render se suscriben a ese evento y
   redibujan sólo lo que cambió (no hay animación continua dependiente de polling: es
   un juego por turnos, el estado cambia en saltos discretos).
5. `Partido.Aplicar` lanza excepción si la acción es ilegal. Si eso pasa en Unity es
   por definición un bug de Input (mandó algo que no pasó por `AccionesLegales`) o de
   `BotRunner` (el bot devolvió algo fuera de su propia lista legal) — nunca algo para
   silenciar con un try/catch que ignora el error; debe loguearse fuerte y idealmente
   parar el loop para poder diagnosticarlo.

Para 2v2/3v3/6 el mismo loop escala en principio (`AccionesLegales`/`Aplicar` ya
soportan más de un jugador), pero el HUD de "quién es el humano y quién es bot en cada
asiento" y la UI de señas entre compañeros humanos son trabajo de Unity adicional, no
del dominio — no está detallado acá porque el primer hito realista es 1v1.

---

## 7. Riesgos

- **Determinismo roto por lógica colada en Unity.** El riesgo central de todo este
  documento. Un `if` de reglas escrito directamente en un `MonoBehaviour` (en vez de
  preguntarle a `AccionesLegales`) compila, funciona en el momento, y el día que
  llegue el servidor autoritativo diverge en silencio entre cliente y servidor. Es la
  misma clase de bug que describe `CLAUDE.md` para las reglas mal recordadas, aplicada
  a la UI. Mitigación: todo botón/carta jugable sale de `AccionesLegales`, siempre.
- **Compatibilidad de lenguaje/target framework.** `/core` usa C# con `record`,
  `readonly record struct` y miembros `required` (`TargetFramework net10.0`,
  `LangVersion latest`). Hay que confirmar, con la versión concreta de Unity elegida,
  que su compilador (Roslyn embebido) soporta esa sintaxis, y que su runtime
  (Mono o IL2CPP) puede cargar/ejecutar un assembly compilado contra `net10.0` sin
  problemas de referencia de BCL. Si no, hace falta retargetear o multi-target
  `core.csproj` (p. ej. agregar `netstandard2.1`) — eso es un cambio a un archivo de
  `/core` y, aunque técnicamente está permitido tocar `/core`, es una decisión de
  arquitectura que conviene confirmar antes (ver §"Decisiones que necesitan mi OK").
- **IL2CPP + stripping agresivo.** Si se shipea con IL2CPP (necesario para iOS), el
  linker puede eliminar miembros de `Core.dll` que sólo se usan por reflection interna
  de los `record` (`Equals`/`GetHashCode`/`PrintMembers`) si el stripping es agresivo.
  Mitigación: `link.xml` preservando el ensamblado `Core` completo, o nivel de
  stripping "Low"/"Minimal" para ese plugin.
- **Licencias de arte.** El mazo de este juego no es un mazo español genérico: hay que
  poder mostrar visualmente piezas, matas y la muestra de forma distinguible. Un asset
  pack de "cartas españolas" comprado en una store probablemente no alcanza sin un
  overlay propio (borde/badge de pieza, indicador de mata) — verificar licencia de
  cualquier pack antes de integrarlo, y no asumir que "cartas españolas" cubre las
  reglas propias del Truco Uruguayo.
- **Performance de UI con muchas cartas.** En 3v3/6 hay hasta 18 cartas en juego
  simultáneo más botones de canto más (eventualmente) señas. Si todo vive en un único
  `Canvas` de uGUI que se reconstruye por frame, los rebuilds pueden ser costosos.
  Mitigación: Canvases separados por zona de mesa, y redibujar sólo en el evento
  discreto de cambio de `EstadoPartida` (§6), nunca por polling en `Update()`.
- **Mapeo espejo desincronizado.** `CartaSpriteMap` es una tabla escrita a mano que
  espeja el inventario de `/core` (40 cartas, `Numero ∈ [1..7,10..12]`, 4 palos). Si
  `/core` cambiara esa forma (no debería, es el mazo español fijo, pero por completitud
  del riesgo) el mapeo quedaría desactualizado sin que nada avise. No es urgente
  cubrirlo ahora, pero si esto crece conviene un test de Unity que valide que el mapeo
  cubre las 40 cartas exactas que `Mazo.Completo()` genera.
- **Señas como falso amigo de "fácil".** Se ve como una animación simple, pero
  `RULES_Afinadas.md` es explícito en que son información pura y no tocan el estado
  legal. La tentación de "total, ya que estoy, hago que el bot las lea" sería agregar
  lógica de reglas (bicheo) que ni siquiera está decidida (`RULES_Afinadas.md` la
  marca como idea a futuro, no implementada). No hacerlo sin pasar antes por
  `PREGUNTAS_ABIERTAS.md`.

---

## Decisiones que necesitan mi OK

- **Versión exacta de Unity** (Unity 6 LTS y build number específico) — y, atada a
  esa elección, si su compilador soporta `record`/`required` de `/core` tal como está
  hoy, o si hace falta retargetear `core.csproj` (multi-target a `netstandard2.1` u
  otro TFM) antes de poder referenciarlo. Esto último toca un archivo de `/core` y
  aunque el reparto de roles permite tocar `/core`, el retargeting es una decisión de
  arquitectura, no un detalle de implementación.
- **Método de link de `/core`**: DLL precompilada en `Assets/Plugins` (recomendado en
  §2) vs. link de fuente con asmdef propio. Si es DLL, falta decidir si el script de
  build/copy se automatiza (pre-build step de Unity, o manual) y quién es responsable
  de correrlo antes de cada sesión de trabajo en Unity.
- **URP vs Built-in Render Pipeline.**
- **UI Toolkit vs uGUI** para HUD de cantos, marcador y botones — impacta directamente
  la estructura de `Scripts/Render` y `Scripts/Input` de §3.
- **Arte placeholder vs arte final desde el arranque**, y de qué fuente (assets
  comprados, gratuitos, o encargados) — con el tema de licencias de §7 sin resolver
  hasta que se elija.
- **Diseño futuro de señas como posible `Accion` no vinculante.** Hoy se modelan
  100% cosméticas en Unity (§4), consistente con que `RULES_Afinadas.md` dice que no
  cambian el estado legal. Pero el día que exista `/server` con reconexión, puede
  convenir que la seña sí viaje como mensaje de red (aunque no como `Accion` del
  reductor) para que un jugador que se reconecta vea las señas que se perdió. Esto es
  diseño de `/server`, no de Unity, pero conviene decidirlo antes de escribir el
  primer prototipo de `SenaView` para no rehacerlo.
- **Interfaz pública de `/bot`.** Este plan asume una firma
  `Accion Elegir(EstadoPartida, JugadorId)`, pero `/bot` todavía no existe (paso 4 del
  plan nocturno). La firma real la fija quien lo construya; si difiere, `BotRunner`
  se ajusta a eso, no al revés.
