# Prompt maestro — primera sesión de Claude Code

> Cómo usarlo: copiá todo lo que está debajo de la línea y pegalo como primer
> mensaje en Claude Code, con el repo abierto. `CLAUDE.md` y `.claude/skills/`
> ya deberían estar en su lugar; este prompt asume que están.
>
> Está diseñado para que la primera sesión **no produzca código**. La Fase 0
> termina con una lista de preguntas. Eso es lo correcto: el documento de reglas
> tiene ambigüedades reales y cerrarlas antes vale más que cualquier avance.

---

Vamos a trabajar juntos en el dominio de este juego de Truco Uruguayo. Antes que
nada leé `CLAUDE.md`, `docs/RULES_Afinadas.md` completo y `docs/PREGUNTAS_ABIERTAS.md`.
Después leé todo el código existente de `/core` y `/tests`.

Tres cosas de contexto antes de que empieces.

**Primero.** El Truco Uruguayo no es el argentino. Tiene muestra, piezas y flor,
y el recuento de tantos es distinto. Casi todo el material sobre truco que
existe en el mundo es argentino, así que si algo que "sabés" sobre el juego no
está escrito en `RULES_Afinadas.md`, asumí que estás equivocado. No completes
huecos de memoria: anotalos como pregunta. Este es el modo de falla más caro del
proyecto, porque un error de este tipo pasa los tests que vos mismo escribiste.

**Segundo.** Yo escribo la lógica de reglas a mano a propósito: aprender es la
mitad del objetivo del proyecto. Tu rol es revisar, corregir, cerrar huecos y
armar el andamiaje. No reescribas mi código sin decirlo antes. Si algo está mal,
mostrame primero el caso que falla.

**Tercero.** El alcance de esta etapa es exclusivamente `/core` y `/tests`.
`/game` (Unity) y `/server` no existen para vos todavía.

---

## Fase 0 — Reconocimiento (esta sesión, sin escribir código)

No modifiques ningún archivo. Devolveme, en este orden:

**a. Modelo actual.** Qué tipos existen en `/core`, qué responsabilidad tiene
cada uno, y quién decide qué. Si el diseño actual se aparta de la forma de
reductor puro descrita en `CLAUDE.md`, decilo con nombre y apellido.

**b. Cobertura de reglas.** Tabla de tres columnas: regla de `RULES_Afinadas.md`
→ dónde está implementada (archivo, tipo) → estado. Estados posibles:
`completa`, `parcial`, `ausente`, `contradice el documento`. Recorré el
documento sección por sección; incluí las que están ausentes, que son el punto.

**c. Contradicciones y errores.** Todo lugar donde el código y el documento no
coinciden. Para cada uno: qué dice el documento, qué hace el código, y qué caso
concreto lo evidencia.

**d. Ambigüedades nuevas.** `docs/PREGUNTAS_ABIERTAS.md` ya tiene una lista.
Agregá las que encuentres y que no estén ahí, con el mismo formato: el caso
concreto, las opciones, tu recomendación y el impacto. No las decidas vos.

**e. Plan.** Pasos chicos e independientes, ordenados por dependencia. Cada paso
con: qué archivos toca, qué regla del documento implementa, qué test lo prueba,
y por qué va en ese lugar del orden y no antes. Como referencia del orden que
tiene sentido, arrancá por lo que no depende de nada — carta, muestra, mazo,
fuerza — y dejá para el final lo que depende de todo: la máquina de estados de
la partida y los modos de varios jugadores.

**Al terminar la Fase 0, pará.** No avances al plan hasta que yo lo apruebe y
hasta que las preguntas abiertas que bloqueen los primeros pasos estén decididas.

---

## Fase 1 en adelante — Ejecución

Cuando apruebe el plan, trabajamos así:

**Un paso por vez.** Al cerrar cada uno me decís: qué cambiaste, por qué, y qué
test lo cubre. Después parás. No encadenes pasos.

**Test primero.** Toda regla nueva o corregida viene con un test xUnit que falla
antes y pasa después. Mostrame que falla antes de implementar. Si el test pasa
antes de tocar el código, el test está mal escrito.

**Ante ambigüedad, pará.** Si el caso que estás por implementar no está resuelto
en `RULES_Afinadas.md` ni decidido en `PREGUNTAS_ABIERTAS.md`, no elijas un
default razonable. Agregalo al archivo con opciones y recomendación, y
preguntame. Un default silencioso acá no rompe nada hoy y aparece meses después
jugando una partida real.

**Gana el documento.** Si el código y `RULES_Afinadas.md` se contradicen, el
documento tiene razón — y avisame, porque ese código lo escribí yo.

**Podés proponer cambios a las reglas, no hacerlos.** Si encontrás una
contradicción interna en `RULES_Afinadas.md` o un caso que falta, me lo listás
como sugerencia. El documento lo edito yo.

---

## Restricciones técnicas

`/core` es una librería pura y determinista. Prohibido: `System.Random` sin
semilla inyectada por constructor, `DateTime.Now`, `Console`, I/O, red, `async`,
estado estático mutable, y cualquier `using UnityEngine`. Si te parece que
necesitás alguna de esas, el diseño está mal en otro lado: decilo, no lo
construyas.

La forma de la API del dominio es un reductor:

```csharp
EstadoPartida                                          // inmutable, serializable
IReadOnlyList<Accion> AccionesLegales(EstadoPartida e, JugadorId j)
EstadoPartida Aplicar(EstadoPartida e, Accion a)
```

`AccionesLegales` es la única autoridad sobre qué se puede hacer en cada
momento. No dupliques validación en otro lado.

Vocabulario del dominio en español rioplatense, sin traducir (`Envido`, `Flor`,
`Muestra`, `Pieza`, `Baza`, `Parda`, `Mano`, `Pie`). Estructura del lenguaje en
inglés.

**Preguntá antes de:** agregar paquetes NuGet, crear proyectos o soluciones
nuevas, refactorizar más de un archivo a la vez, cambiar la forma de la API del
dominio, o editar `RULES_Afinadas.md`.

**Fuera de alcance:** señas (el documento las nombra pero no las especifica),
barajado con animación, corte realista, trampas, y todo lo que viva en `/game` o
`/server`.

---

Empezá por la Fase 0. Tomate el tiempo de leer el documento entero antes de
escribir la primera línea de la respuesta.
