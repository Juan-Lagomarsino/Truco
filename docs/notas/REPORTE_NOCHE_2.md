---
estado: cerrado — corrida nocturna terminada
tags: [nocturno, reporte]
---

# Reporte de la noche 2

Corrida siguiendo `docs/notas/PLAN_NOCTURNO_2.md`, rama `noche/cobertura` (creada desde
`noche/roadmap`), sin push. Build y `dotnet test` en verde en todo momento: **476 → 553
tests** (552 en verde + 1 `Skip` documentado), 9 commits, 0 warnings.

No se agregó ninguna regla nueva. Es profundidad sobre lo que ya estaba decidido: más
invariantes de propiedad, más cobertura de reglas cerradas poco testeadas, un fuzz de
honestidad del bot más fuerte, UX de la consola, y documentación XML de la API pública.

---

## Qué se hizo (commits, de más viejo a más nuevo)

1. **`8c1f3dd` Bloque A — invariantes de propiedad del reductor** (`tests/InvariantesFuzzTests.cs`)
   - A1: sobre 9 combinaciones (1v1/2v2/a6 × 3 semillas) y en cada estado de la partida
     completa (no sólo el final), toda acción candidata fuera de `AccionesLegales` hace
     lanzar a `Aplicar`. El universo de candidatas es exhaustivo: las 40 cartas × todos los
     tipos de acción × todos los jugadores.
   - A2: `AccionesLegales` nunca es vacía para todos los jugadores en un estado vivo (sí lo
     es en el estado terminal); toda acción legal aplicada no lanza.
   - A3: misma semilla + misma lista de acciones ⇒ mismo `EstadoPartida` campo a campo, en
     cada paso, extendido a los tres modos (esto ya lo cubría `GrabacionFuzzTests` vía el
     codec; acá se hace directo, sin pasar por texto).
   - A4: el puntaje de cada equipo nunca es negativo, nunca supera el largo, y es monótono
     no decreciente en una partida fuzz completa, en los tres modos.
   - A5: se amplió el corpus de semillas de `GrabacionTextoTests` (de 3 a 11) para el round
     trip del codec.
   - **Encontró un hallazgo real** (ver más abajo, H1).

2. **`48688d5` Bloque B — cobertura de reglas ya decididas** (Envido, Flor, 2v2, cierre)
   - Envido: Falta contra el fin del partido si el líder ya está en buenas (A4); Falta con
     los dos equipos empatados (B5); el 12 espejo cuenta como pieza también para el envido,
     no sólo la flor (C1, antes sólo estaba probado para flor).
   - Flor: `Contra Flor al Resto` con el rival sin flor cobra 3, simétrico al caso ya
     cubierto de `Con Flor Envido`; los cantos de flor no se encadenan (C3): con un bid
     pendiente sólo hay Quiero/NoQuiero, y una vez resuelto uno no se puede cantar otro; la
     flor se resuelve antes que el truco cuando quedan los dos pendientes (B4).
   - 2v2: quién abre la baza siguiente cuando dos del mismo equipo empatan arriba es el que
     tiró primero, no cualquiera (G1, agregado también como guarda a nivel de
     `Baza.Resolver` en `BazaTests`); cualquier jugador del equipo rival puede responder un
     canto, no sólo "el pie" (B9), probado para envido, truco y bid de flor; irse al mazo es
     acción del equipo — el rival cobra y la mano termina para los cuatro (B7).
   - Cierre de partido a mitad de mano (B6, `tests/CierrePartidoTests.cs`): si la flor sola
     ya cruza la meta, el truco que se hubiera ganado después nunca se acredita.

3. **`8606b3a` Bloque C — bot y CLI**
   - C2 (honestidad del bot): el test existente sólo cubría 1v1 (sin compañero). Se agregó
     `tests/PoliticaSimpleHonestidadTests.cs`: en partidas reales de 2v2 y de a 6, mezclar
     las cartas de todos menos el que decide (compañero incluido) nunca cambia la decisión
     de `PoliticaSimple`.
   - C3 (UX de `/cli`): `--help`/`-h`; validación de argumentos separada en
     `cli/Argumentos.cs` (puro, sin `Console` ni IO, para poder testearlo); mensajes de
     error claros (sin stack trace) para semilla inválida y para `--reproducir` con archivo
     inexistente o con formato inválido. `Program.Main` pasó de `void` a `int` (código de
     salida). Se agregó `tests -> cli` como referencia de proyecto (permitido por el plan)
     para probar `Argumentos` y `Program.Main` de punta a punta. No cambia el formato de
     grabación ni la lógica de juego.

4. **`7545bba` Bloque D — documentación XML de la API pública**
   - Un escaneo de todos los miembros públicos de `/core` encontró 27 sin `///` inmediato
     (en su mayoría constructores y propiedades chicas de records ya documentados a nivel
     de tipo, más dos huecos puntuales en `EstadoPartida`: `CantidadJugadores` y `Muestra`
     no tenían su propio `summary` aunque sus vecinas sí). Se completaron los 27. Cero
     cambios de comportamiento; build sigue en 0 warnings.

5. **`b83551e`, `0889fe7`, `874d2a7`** — retoques chicos encontrados en una segunda pasada:
   B9 también para un bid de flor en 2v2; más semillas en el fuzz de bot vs bot (C1); la
   Falta Envido es terminal, no se puede revirar sobre ella (B2, el "tope de revires" que
   faltaba junto al "sin límite" de Envido/Real Envido que ya estaba probado).

6. **`b4f1628`** — decisiones de andamiaje de esta corrida en `DECISIONES_NOCTURNAS.md` (D5-D7).

---

## Hallazgo: H1 (ver `docs/notas/HALLAZGOS_NOCHE_2.md` para el detalle completo)

El fuzz de A1 encontró que **`Partido.Aplicar` acepta un `CantarEnvido` de apertura que
`Partido.AccionesLegales` no ofrece**. Repro mínima: partida 1v1 recién creada (semilla 3),
turno del jugador 1; `AccionesLegales` para el jugador 0 está vacía, pero
`Aplicar(e, new CantarEnvido(J0, Envido))` no lanza y deja un envido pendiente.

Causa: `AccionesLegales`, en turno libre, sólo ofrece abrir el envido a `e.Turno`; pero
`PuedeIniciarEnvido` (el guard que usa `Aplicar`) sólo exige que ese jugador no haya tirado
todavía su carta — nunca chequea de quién es el turno. Es genuinamente ambiguo cuál lado es
el bug: la letra de la regla ("los que todavía no tiraron... sí pueden" tocar envido) podría
leerse como que cualquiera que no tiró puede abrirlo (entonces `AccionesLegales` es la
restrictiva de más), o como que sólo puede el del turno (entonces `PuedeIniciarEnvido` le
falta el chequeo). No lo resolví — es una decisión de regla, no algo mío.

**No se tocó la lógica del autor.** El test de propiedad excluye puntualmente esta forma
exacta de acción de la aserción "tiene que lanzar" (con un predicado que replica la
condición, documentado inline) y queda además un `[Fact(Skip = "...")]` con la
reproducción mínima aislada, para que no se pierda si alguien lo toca sin querer.

---

## Preguntas pendientes (todas juntas)

- **P1** (de la noche anterior, sigue abierta): señas con las 3 cartas de la mano buenas a
  la vez — ¿tope de 2 o se muestran las 3? Ver `PREGUNTAS_PENDIENTES.md`.
- **P2–P16** (de la noche anterior, sin cambios): decisiones de producto/arquitectura de
  Unity, servidor y publicación. No se tocaron esta noche (fuera de alcance).
- **P17** (nueva, de esta noche): ¿quién puede abrir el envido antes de que le llegue el
  turno — cualquiera que no tiró, o sólo el que tiene el turno? Ver el hallazgo H1 arriba y
  el detalle completo en `PREGUNTAS_PENDIENTES.md`.

---

## Qué NO se tocó (a propósito)

- `/game`, `/server`, cualquier cosa de publicación: fuera de alcance por el plan.
- La lógica de reglas del autor en `core/Partido.cs`: sólo se agregó andamiaje de tests
  alrededor, nunca se cambió una línea de la lógica existente.
- El agregador de "seña de mano completa" (bloqueado por P1, sin cambios).

## Cosa suelta, sin importancia

`tests/_ScratchRepro.cs` quedó como un archivo vacío (un comentario) en el working tree,
sin commitear. Lo usé para reproducir el hallazgo H1 antes de escribirlo como test de
verdad, y el sandbox no me dejó borrarlo con `rm` (guardarraíl de la noche). Es inofensivo
(compila como un no-op) pero se puede borrar a mano cuando quieras — `git status` no lo
tiene bajo seguimiento, así que no aparece en ningún commit.

---

## Próximo paso exacto para retomar

1. **Decidí P17** (y de paso podés revisar P1, que sigue esperando). Con esa respuesta, el
   próximo que trabaje esto puede: si la interpretación es "cualquiera que no tiró",
   ampliar `AccionesLegales` (el caso libre) para ofrecer la apertura de envido a todo
   jugador de cualquier equipo que no haya tirado su carta, no sólo a `e.Turno`; si es
   "sólo el del turno", agregarle a `PuedeIniciarEnvido` el chequeo
   `jugador.Equals(e.Turno)`. Cualquiera de las dos es un cambio de una línea en
   `core/Partido.cs`, con el test ya escrito (sólo hay que sacarle el `Skip` a
   `H1_CantarEnvidoDeAperturaFueraDeTurno_DeberiaLanzarPeroNoLanza` en
   `tests/InvariantesFuzzTests.cs` y borrar el predicado
   `EsHallazgoH1_CantarEnvidoDeAperturaFueraDeTurno` de la aserción de A1 una vez arreglado).
2. Si preferís seguir sumando cobertura en vez de resolver P17: el backlog de
   `PLAN_NOCTURNO_2.md` está esencialmente agotado en lo que se puede hacer sin inventar
   reglas ni decisiones de producto — las dos pasadas completas de esta noche no encontraron
   más huecos "chicos" de Bloque A-D. El próximo frente natural, si hay ganas, es volver a
   `PLAN_Unity.md` / `PLAN_Server.md` / `PLAN_Publicacion.md`, pero esos están bloqueados
   por las preguntas P2-P16 (decisiones tuyas de producto), no por falta de tests.
3. `git log main..noche/cobertura` tiene los 9 commits de esta noche; están todos en verde
   e individualmente revisables. La rama no se pusheó ni se mergeó a nada.
