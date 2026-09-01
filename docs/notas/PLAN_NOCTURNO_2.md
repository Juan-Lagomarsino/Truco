---
estado: listo para lanzar
tags: [nocturno, plan, cobertura]
---

# Plan nocturno 2 — endurecer /core sin inventar reglas

Corrida desatendida de Claude Code. Lanzar parado en la raíz del repo con
`claude --permission-mode bypassPermissions` y decir: **"seguí docs/notas/PLAN_NOCTURNO_2.md"**.

El objetivo de esta noche NO es agregar reglas nuevas: la noche anterior ya dejó /core, /bot,
/cli y grabación funcionando (ver `REPORTE_NOCHE.md`, 476 tests en verde). Esta noche es
**profundidad, no ancho**: más cobertura de tests e invariantes sobre las reglas que YA están
decididas, y endurecer /bot y /cli. Nada de /game, nada de /server, nada nuevo que dependa de una
decisión mía.

---

## Antes de tocar nada

Leé, en este orden: `CLAUDE.md`, `docs/RULES_Afinadas.md` completo, `docs/PREGUNTAS_ABIERTAS.md`
(en especial la sección "Solucionadas": esas reglas están cerradas y son terreno para tests),
`docs/notas/PREGUNTAS_PENDIENTES.md` (lo que quedó bloqueado), y todo `/core`, `/tests`, `/bot`,
`/cli`. El código ya escrito es una decisión de diseño, no un borrador.

---

## Regla de oro de la noche

- Trabajás SOLO toda la noche, sin esperarme nunca, hasta terminar lo que puedas o quedarte sin
  contexto/uso. **Nunca frenes a preguntarme.**
- Si algo necesita una decisión mía o no lo podés resolver solo: anotalo en
  `docs/notas/PREGUNTAS_PENDIENTES.md` (contexto, qué decidir, opciones, tu recomendación
  fundamentada) y **saltá al siguiente ítem que sí puedas hacer**. El orden es guía, no traba.
- Decisiones que SÍ tomás vos (no son preguntas): nombres, estructura de tests, andamiaje, cómo
  organizar un fixture. Anotá el porqué en `docs/notas/DECISIONES_NOCTURNAS.md`.

## LÍNEA ROJA (la fuente número uno de bugs de este proyecto)

- **No inventes ni completes de memoria NINGUNA regla del truco** que no esté escrita en
  `RULES_Afinadas.md` o resuelta en `PREGUNTAS_ABIERTAS.md`. Esto NO es el truco argentino:
  existe muestra, piezas, flor y el recuento es distinto. Si un test depende de una regla ambigua,
  eso ES una pregunta pendiente: anotala y saltá. Nunca la resuelvas vos.
- Un test escrito con una regla equivocada **pasa igual** y esconde el bug meses. Si no estás 100%
  seguro de qué dice la regla, no escribas el assert: anotá el hueco y seguí.

## Política ante un bug (crítico — leelo dos veces)

Tu rol es revisar, corregir huecos y **armar andamiaje (tests, tipos, estructura)** — NO reescribir
la lógica de reglas que el autor escribió a mano. Si un test nuevo revela que el código contradice
`RULES_Afinadas.md`:

1. **No parchees la lógica del autor a escondidas.** El objetivo es que él VEA el caso que falla.
2. Escribí el caso que falla como reproducción mínima en `docs/notas/HALLAZGOS_NOCHE_2.md`
   (creá el archivo): qué regla del doc, qué hace el código, semilla/acciones que lo reproducen,
   y tu diagnóstico. Si además necesita una decisión, entra a `PREGUNTAS_PENDIENTES.md`.
3. **El suite queda en verde igual.** No commitees un test en rojo. Si querés dejar el caso como
   test, marcalo `[Fact(Skip = "hallazgo: ver HALLAZGOS_NOCHE_2.md")]` con el motivo. Nunca dejes
   el build ni `dotnet test` en rojo en un commit.

## Anti-desborde (no te vayas del carril)

- Trabajá SÓLO dentro de este repo. No crees ni edites archivos fuera de la carpeta del proyecto.
- Un ítem del backlog por vez, en orden de prioridad. Cerrá el ítem (commit verde) antes de abrir
  el siguiente. No encadenes cinco a la vez.
- Si un ítem se te empieza a ramificar en algo grande (un refactor, un rediseño, "sería mejor si
  también..."), pará: anotalo como idea en `PREGUNTAS_PENDIENTES.md` y volvé al carril. La noche es
  para cerrar ítems chicos, no para abrir frentes nuevos.
- No persigas cobertura de algo cuya regla no esté cerrada. Si te trabás 15 min en un ítem, saltá.

## Guardarraíles duros

- **Aislamiento — sin red:** trabajás 100% offline. **No** uses internet para nada: sin
  WebSearch, sin WebFetch, sin `curl`/`wget`, sin clonar repos, sin restaurar/descargar paquetes,
  sin consultar nada online. La única red que puede aparecer es la que el SDK use por su cuenta para
  compilar con lo que YA está cacheado localmente; no dispares ninguna descarga vos. No leas ni
  escribas credenciales, tokens ni config de git remota. Si algo parece necesitar la red, no lo
  hagas: anotalo en `PREGUNTAS_PENDIENTES.md` y saltá.
- **Aislamiento — sin salir del directorio:** operá SÓLO dentro de la carpeta de este repo. No
  leas, crees ni edites archivos fuera de ella (nada de `$HOME`, dotfiles globales, `/etc`, otros
  proyectos, ni la papelera). No cambies configuración del sistema ni del usuario. Todo lo que
  produzcas vive dentro del repo.
- **Rama:** `git switch -c noche/cobertura` (desde `noche/roadmap`, el HEAD actual). **NO** push.
  **NO** toques `main` ni `devIA`. **NO** borres archivos (nada de `rm`, `reset --hard`, `clean`).
- **Alcance de archivos:** `/core`, `/tests`, `/bot`, `/cli`, `/docs/notas`. **NO TOCAR** `/game`
  ni `/server` (no existen para vos: Unity es Fase 4, server es posterior).
- **Reglas duras de /core** (el andamiaje de tests puede usar IO/Random, pero /core no): sin
  `Random` sin semilla, sin `DateTime.Now/UtcNow`, sin `Console`/ficheros/red, sin `async`/`Task`,
  sin estático mutable, sin `using UnityEngine`. Console/IO **sólo** en `/cli`.
- **No instalar NADA:** ni NuGet (`dotnet add package`), ni tools globales, ni apt/snap/pip/npm.
  Sólo el SDK y lo que ya está en el repo. `dotnet add reference` entre proyectos del repo sí vale.
  Si un ítem necesita una dependencia nueva: anotala en `PREGUNTAS_PENDIENTES.md` y saltá.
- **Commits:** uno chico por sub-paso, mensaje en español imperativo, sin punto final, sin
  coautoría. Build + `dotnet test` en verde antes de cada commit.
- **Compilar/testear:** `export DOTNET_ROOT=$HOME/.dotnet; export PATH=$HOME/.dotnet:$PATH; dotnet test`

---

## Backlog priorizado (hacé A, después B, después C, después D — y ciclá)

Prioridad de arriba hacia abajo. Dentro de cada bloque, ítems independientes: si uno se traba,
saltá al siguiente. Cuando llegues al final, volvé a empezar buscando huecos que hayan quedado.

### Bloque A — Invariantes y propiedades de /core (andamiaje puro, cero decisiones)

Tests de propiedad sobre muchas semillas (usá el `BarajadorConSemilla` que ya existe). Documentan
contratos que deben valer SIEMPRE; no dependen de ninguna regla ambigua.

- **A1.** `Aplicar` con acción ilegal **siempre lanza**: para un corpus de estados generados por
  fuzz, toda `Accion` que no esté en `AccionesLegales` debe hacer que `Aplicar` lance. Modos 1v1,
  2v2 y a 6.
- **A2.** `AccionesLegales` **nunca vacío** salvo estado terminal, y toda acción legal aplicada
  **no lanza**. (Complemento de A1.)
- **A3.** **Determinismo:** misma semilla + misma lista de acciones ⇒ `EstadoPartida` idéntico,
  campo por campo (reusá el comparador de `GrabacionFuzzTests`). Extendé a los tres modos y a un
  corpus grande de semillas si no está ya cubierto.
- **A4.** **Puntaje sano:** los puntos de cada equipo nunca son negativos, nunca superan el tope
  del partido, y son monótonos no decrecientes a lo largo de una partida completa fuzz.
- **A5.** **Round-trip de serialización:** `GrabacionTexto` codifica→decodifica sin pérdida sobre
  un corpus grande de partidas fuzz de los tres modos. Si existe (o es trivial exponer) un round
  trip de `EstadoPartida`, igual. Si serializar `EstadoPartida` necesitara API nueva pública en
  /core, eso es una decisión → anotala y quedate sólo con `Grabacion`.

### Bloque B — Cobertura de reglas YA decididas poco cubiertas (test-first, documenta lo existente)

Cada ítem: mirá qué dice `RULES_Afinadas.md` / la sección "Solucionadas" de `PREGUNTAS_ABIERTAS.md`,
y escribí tests que fijen ese comportamiento. Si el código ya lo hace bien, el test pasa y queda de
guarda. Si no, es un **hallazgo** (ver política de bug: a `HALLAZGOS_NOCHE_2.md`, no lo parchees).
Sólo escribí el assert de lo que el doc dice sin ambigüedad; ante duda, anotá y saltá.

- **B1. Envido:** revire y tope de revires (PA §B2), Falta Envido contra final de malas/partido
  (§A4), Falta Envido con equipos iguales (§B5), desempate por ser mano (§B8), ventana hasta cuándo
  se puede tocar (§A1), quién puede responder un canto (§B9).
- **B2. Flor:** obligatoriedad de la flor (§A2), puntos que entrega quien no quiere Contra Flor al
  Resto (§A3), "con flor envido" cuánto entrega el que no quiere (§B3), orden de resolución con flor
  y truco pendientes (§B4), denuncia de flor escondida (§F3), acreditación de la flor (§F4),
  enfrentamiento 1v1 contra rival sin flor (§F2), enfrentamiento con collera 2v2 (§G2).
- **B3. Truco/Retruco/ValeCuatro:** escalado y quién puede responder (§B9), interacción con irse al
  mazo, puntos en juego según nivel.
- **B4. Irse al mazo:** irse antes de tirar la primera carta siendo mano (§A5), irse en partidas de
  equipo (§B7), cierre del partido a mitad de mano (§B6).
- **B5. Modo de a 6:** redondilla y pico a pico (§B10), quién abre la baza siguiente cuando gana un
  equipo 2v2 (§G1). Ya hay tests de schedule y fuzz — buscá los huecos, no dupliques.
- **B6. Muestra y piezas:** el 12 espejo como pieza para formar/contar flor y envido (§C1), que NO
  existe un canto "Contra Flor" a secas (§C2), la estructura de los cantos de flor (§C3),
  jerarquía de piezas sobre matas en casos límite.

### Bloque C — /bot y /cli (andamiaje; IO sólo en /cli)

- **C1.** Fuzz **bot vs bot** en los tres modos (1v1, 2v2, a 6) × muchas semillas: la partida
  siempre termina, sin deadlock ni excepción, con puntaje válido. Extendé lo que ya haya.
- **C2.** Test de honestidad del bot: `PoliticaSimple` **nunca** decide mirando cartas ajenas ni
  del compañero — sólo la vista del jugador. Fijalo con un test que lo demuestre.
- **C3.** `/cli`: mejoras de UX **no invasivas** que no tocan /core — `--help`, validación de
  argumentos, mensajes de error claros cuando la semilla o el archivo de reproducción son inválidos.
  Sin cambiar el formato de grabación ni la lógica de juego.

### Bloque D — Documentación de la API pública de /core (sin cambiar comportamiento)

- **D1.** Comentarios XML (`///`) en los tipos y métodos públicos de `/core` (`EstadoPartida`,
  `AccionesLegales`, `Aplicar`, `Accion`, y los records del dominio): qué es, qué invariantes
  respeta, qué lanza. Cero cambios de comportamiento; si al documentar encontrás una contradicción
  con el doc de reglas, es un hallazgo (no lo arregles).

---

## Qué NO hacer esta noche (bloqueado)

- **El agregador de "seña de mano completa"** (qué señas mostrar con 3 cartas): bloqueado por P1 en
  `PREGUNTAS_PENDIENTES.md` (caso de 3 cartas buenas sin regla). Ya está `Señas.DeCarta` (carta
  suelta); el agregador NO se implementa hasta que decida P1.
- Tocar `/game` o `/server`, scaffoldear Unity o el servidor, o ejecutar cualquier cosa de
  publicación. Los planes ya están escritos (`PLAN_Unity/Server/Publicacion.md`) y esperan mi OK.
- Refactors de más de un archivo, cambiar la forma de la API del dominio, o editar
  `RULES_Afinadas.md`. Propuestas de corrección van como lista a `PREGUNTAS_PENDIENTES.md`, no las
  aplicás.

---

## Reporte final

Antes de quedarte sin contexto o al terminar todo lo posible, escribí
`docs/notas/REPORTE_NOCHE_2.md`: qué hiciste, lista de commits (con qué test cubre cada uno),
decisiones tomadas (link a `DECISIONES_NOCTURNAS.md`), hallazgos (link a `HALLAZGOS_NOCHE_2.md`),
todas las preguntas pendientes juntas, y el **próximo paso exacto** para retomar. Después pará.

Seguí ciclando por el backlog mientras haya algo que puedas avanzar; no pares sólo porque juntaste
preguntas o hallazgos.
