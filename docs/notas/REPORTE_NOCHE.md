---
estado: corrida nocturna completa
tags: [nocturno, reporte]
---

# Reporte de la noche

Corrida autónoma siguiendo `docs/notas/PLAN_NOCTURNO.md`, rama `noche/roadmap` (creada
desde `devIA`). Ítems 1 a 6 (HACER) completos; ítems 7 a 14 (PLANIFICAR) completos como
documentos. Build y suite en verde en todo momento: 216 tests al arrancar, **476 al
terminar**, 0 fallas.

## Qué se hizo

1. **Suite verificada** (paso 1): build limpio, 216/216 tests en verde al arrancar. No hizo
   falta arreglar nada.
2. **Guarda de reglas duras de /core** (paso 2): `tests/GuardaReglasDurasTests.cs` escanea
   todo `core/*.cs` buscando `Random` sin semilla, `DateTime.Now/UtcNow`, IO/consola/red,
   `async`/`Task`, estático mutable y `UnityEngine`. Incluye tests de la guarda en sí con
   snippets sintéticos.
3. **Señas en /core** (paso 3): `docs/notas/SEÑAS.md` estaba cerrado, así que se implementó
   `Domain.Señas.DeCarta(Carta, Muestra)` — el mapeo de una carta suelta a su seña. **No**
   se implementó la función de "seña de la mano completa" (con el "cerrar ambos ojos"):
   encontré un hueco real que ni `RULES_Afinadas.md` ni `SEÑAS.md` resuelven (qué pasa con
   tres cartas buenas a la vez). Ver preguntas pendientes abajo (P1).
4. **`/bot`** (paso 4): librería nueva con `PoliticaSimple.Elegir(EstadoPartida, JugadorId)`
   — umbrales fijos sobre `Envido.De`/`Flor.De`/`Jerarquia.Fuerza`, mirando sólo la mano
   propia (nunca lee cartas ajenas, ni de compañero). El primer intento de fuzz encontró un
   bug real (el bot llamaba `Envido.De` sobre una mano con flor, que `/core` rechaza a
   propósito) — corregido tratando esas manos como "no compite", igual criterio que
   `Partido.EnvidoParaComparar` ya usa internamente.
5. **`/cli`** (paso 5): consola jugable 1v1 vos contra `PoliticaSimple`. Console/IO sólo
   ahí. Probada de punta a punta con `dotnet run --project cli -- <semilla>` jugando toda
   una partida hasta el final, dos semillas distintas, sin errores.
6. **Grabación/reproducción** (paso 6): `Grabacion` (record: semilla + parámetros de
   `Partido.Nueva` + lista de `Accion`) y `Grabador.Reproducir`/`ReproducirPasoAPaso` en
   `/core`, más `GrabacionTexto` (codec de texto, sin IO) y `GrabacionArchivo` en `/cli`
   (el IO real). La consola ahora graba cada partida jugada y puede reproducirla con
   `--reproducir <archivo>`. Test-first siguiendo `docs/notas/DISENO_Grabacion.md`: fuzz de
   partida completa en 1v1/2v2/modo de a 6, grabada y reproducida paso a paso con un
   comparador campo por campo (`Assert.Equal` directo sobre `EstadoPartida` no sirve: sus
   listas comparan por referencia).
7–14. **Planes escritos, nada ejecutado**: `docs/notas/PLAN_Unity.md`,
   `docs/notas/PLAN_Server.md`, `docs/notas/PLAN_Publicacion.md` y
   `docs/notas/DISENO_Grabacion.md` (este último, insumo directo del paso 6). Cada uno con
   su sección de decisiones que necesitan tu OK, consolidadas abajo. No se tocó `/game` ni
   `/server`, no se creó ninguna cuenta ni se instaló nada.

## Commits (rama `noche/roadmap`, en orden)

1. `bda6679` — test(core): guarda automática de las reglas duras de /core
2. `c02bcf6` — feat(core): mapeo carta a seña (17c-1)
3. `8ef6dc1` — docs(notas): planes de Unity/server/publicación y diseño de grabación
4. `c7a0fd5` — docs(notas): consolidar preguntas pendientes de los planes nocturnos
5. `9c0754c` — feat(bot): política simple y honesta, EstadoPartida a Acción (18a)
6. `20a663c` — feat(cli): consola jugable 1v1 humano contra PoliticaSimple (18b)
7. `5d3a9b7` — feat(core): grabación y reproducción determinista de partidas (19a)
8. `df05258` — feat(core,cli): codec de texto para Grabacion y wiring en la consola (19c)

Ningún push. La rama sigue local, sin tocar `main` ni `devIA`.

## Decisiones que tomé sola/o (arquitectura, no reglas)

Detalle completo con el porqué de cada una en `docs/notas/DECISIONES_NOCTURNAS.md`:

- **D1** — La guarda de reglas duras es textual (regex sobre el código fuente), no un
  analizador Roslyn, para no agregar dependencias nuevas sin tu OK.
- **D2** — De señas sólo implementé `DeCarta` (una carta suelta), no el agregador de mano
  completa, porque ese agregador sí tiene un hueco real de reglas (ver P1).
- **D3** — El archivo se llama `core/Señas.cs` (con eñe), igual criterio que
  `docs/notas/SEÑAS.md` ya existente.
- **D4** — `GrabacionTexto` (el codec de texto de una Grabacion) vive en `/core`, no en
  `/cli`: es transformación de datos pura, sin IO, mismo criterio que "EstadoPartida es
  serializable".

## Todas las preguntas pendientes juntas

Detalle completo en `docs/notas/PREGUNTAS_PENDIENTES.md`. Resumen:

- **P1 (regla del truco, la única que es realmente una "pregunta de reglas"):** ¿qué
  señas se hacen si las tres cartas de la mano son "buenas" a la vez? Ni
  `RULES_Afinadas.md` ni `SEÑAS.md` lo dicen ("pueden ser hasta dos" no aclara si es un
  tope duro). Mi recomendación: tope de 2, mostrando las de mayor jerarquía — pero no lo
  implementé, queda como decisión tuya.
- **P2–P16 (decisiones de negocio/arquitectura de los planes, no de reglas):** de
  `PLAN_Unity.md` (versión de Unity, link a /core, URP/uGUI, arte), `PLAN_Server.md`
  (autenticación, persistencia, hosting, reconexión), y `PLAN_Publicacion.md` (orden de
  publicación, presupuesto, recurso de Mac para iOS, monetización, nombre del juego). Todas
  con su propia sección "Decisiones que necesitan mi OK" en cada documento.

## Próximo paso exacto para retomar

1. Decidir **P1** (señas con 3 buenas) y, si hay respuesta, implementar el agregador de
   "seña de la mano completa" en `core/Señas.cs` (test-first, ya con `DeCarta` como base).
2. Revisar los planes de Unity/server/publicación y sus preguntas de negocio; ninguna
   bloquea seguir en `/core`.
3. Si seguís el orden del plan original: ítems 7–9 (Unity) son los siguientes en pasar de
   "plan" a "hacer", pero necesitan tu OK explícito antes de tocar `/game` (está marcado
   NO TOCAR hasta Fase 4 en `CLAUDE.md`).
4. Mergear `noche/roadmap` a `devIA` cuando lo revises (no lo hice yo: son 8 commits,
   ningún push, rama local nada más).
