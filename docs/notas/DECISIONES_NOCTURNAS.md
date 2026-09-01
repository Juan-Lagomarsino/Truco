---
estado: en progreso — corrida nocturna
tags: [nocturno, decisiones, arquitectura]
---

# Decisiones nocturnas

Cosas que decidí solo durante la corrida (arquitectura, andamiaje, nombres) porque
`PLAN_NOCTURNO.md` las clasifica como decisiones mías, no preguntas para el autor. Cada
una con el porqué.

---

## D1. Guarda de reglas duras: guarda textual en /tests, no analizador Roslyn

**Decisión:** implementé la guarda del paso 2 (`tests/GuardaReglasDurasTests.cs`) como
tests xUnit que leen el código fuente de `/core` como texto y lo escanean con regex, en
vez de un analizador Roslyn (`DiagnosticAnalyzer`).

**Por qué:** un analizador Roslyn es más preciso (entiende sintaxis real, no texto), pero
requiere agregar paquetes NuGet nuevos (`Microsoft.CodeAnalysis.CSharp` como mínimo), y
`PLAN_NOCTURNO.md` prohíbe instalar dependencias nuevas sin OK. La guarda textual cubre el
caso real (nadie va a ofuscar código para esquivarla) y ya está verificada contra las 34
archivos actuales de `/core` sin falsos positivos, incluyendo los casos límite reales del
código (`new Random(semilla)`, `static readonly` en `Mazo.cs`/`Muestra.cs`, métodos
estáticos puros en todos lados). Si más adelante se quiere una guarda más estricta, migrar
a un analizador es un paso aparte que si necesita el OK para la dependencia nueva.

## D2. Señas: `DeCarta` solo, sin agregador de mano

**Decisión:** implementé únicamente `Domain.Señas.DeCarta(Carta, Muestra)` (mapeo de una
carta a su seña), no una función que decida qué señas mostrar con una mano completa de 3
cartas.

**Por qué:** el mapeo carta→seña está 100% cerrado en `RULES_Afinadas.md` y no tiene
ningún hueco. La función de "mano completa" sí tiene un hueco real (ver
`PREGUNTAS_PENDIENTES.md` P1, el caso de 3 cartas buenas simultáneas) que ninguno de los
dos documentos de reglas resuelve. Implementar solo la mitad bien definida evita una
implementación a medias con una rama adivinada; queda la puerta abierta a agregar el
agregador de mano en un commit aparte una vez que P1 tenga respuesta.

## D3. Nombre de archivo `core/Señas.cs` con eñe

**Decisión:** usé `Señas.cs` (con eñe) como nombre de archivo, igual que
`docs/notas/SEÑAS.md` ya existente, en vez de `Senas.cs`.

**Por qué:** el vocabulario del dominio en `CLAUDE.md` ya usa `Seña` con eñe y no se
traduce; el proyecto ya tiene un archivo `SEÑAS.md` con el mismo criterio. Confirmé que
compila sin problemas (los identificadores Unicode están soportados en C#) y los 436 tests
de la suite corren en verde con el archivo así nombrado.

## D4. `GrabacionTexto` (codec de la Grabacion) vive en /core, no en /cli

**Decisión:** el encoder/decoder de texto plano de una `Grabacion`
(`core/GrabacionTexto.cs`) vive en `/core`, no en `/cli` junto con el IO de archivo.

**Por qué:** `docs/notas/DISENO_Grabacion.md` (§3.3, §6.4) dejó esto marcado como
decisión abierta del autor, porque toca "qué entra en /core". Pero convertir un objeto a
`string` y viceversa no es entrada/salida (no toca `Console`, archivos ni red): es
transformación de datos pura, igual que cualquier `ToString()`/parseo que ya vive en
`/core`. `CLAUDE.md` ya da por sentado que `EstadoPartida` es serializable; esto extiende
la misma idea a `Grabacion`. Ponerlo en `/core` además lo deja testeable en `/tests` sin
depender de que exista `/cli`, y evita duplicar el formato el día que `/server` también
necesite leer/escribir grabaciones. Sólo el IO real (`File.ReadAllText`/`WriteAllText`)
queda afuera, en `/cli`.

---

## D5. Plan nocturno 2 — rama `noche/cobertura`, sin cambios de reglas

Continuación de las decisiones de arriba, ya en la segunda corrida nocturna
(`docs/notas/PLAN_NOCTURNO_2.md`). Acá van sólo las decisiones de andamiaje/estructura de
esta corrida; los hallazgos de comportamiento van a `HALLAZGOS_NOCHE_2.md` y las preguntas
de reglas a `PREGUNTAS_PENDIENTES.md`.

## D6. Hallazgo H1: se documenta y se acota, no se deshabilita todo el test de propiedad

**Decisión:** cuando el fuzz de A1 (`tests/InvariantesFuzzTests.cs`) encontró que `Aplicar`
acepta un `CantarEnvido` de apertura que `AccionesLegales` no ofrece (H1, ver
`HALLAZGOS_NOCHE_2.md`), no deshabilité todo el test de propiedad con `Skip`. En cambio,
agregué un predicado (`EsHallazgoH1_...`) que replica la condición exacta de la divergencia
(usando sólo campos públicos del estado) y la excluye puntualmente de la aserción "tiene que
lanzar", dejando el resto de A1 verificado sobre miles de combinaciones estado×acción.
Además dejé un `[Fact(Skip = "...")]` con la reproducción mínima aislada.

**Por qué:** `Skip`-ear todo `Aplicar_ConAccionQueNoEsLegal_...` para esconder un solo caso
hubiera tirado por la borda la cobertura de A1 sobre el resto de las acciones y estados (que
sí es real y sí hubiera atrapado un bug futuro). Acotar la excepción a su forma exacta
mantiene la guardia activa en todo lo demás, y si alguien "arregla" el código sin que el
predicado dejara de aplicar, el test de propiedad volvería a fallar (porque el predicado ya
no encontraría ningún candidato que excluir del tipo esperado) — avisando que hay que
revisar esta nota.

## D7. `/cli`: `Argumentos.cs` puro y `Main` devuelve código de salida, no `void`

**Decisión:** el parseo/validación de argumentos de la consola (C3) quedó en un tipo nuevo,
`cli/Argumentos.cs`, sin ningún `Console` ni IO — sólo toma `string[]` y devuelve tuplas
`(ok, valor, error)`. `Program.Main` pasó de `void` a `int` (código de salida: 0 éxito, 1
error de uso), y agregué `tests -> cli` como referencia de proyecto (`dotnet add
reference`, explícitamente permitido por el plan) para poder probar `Argumentos` y
`Program.Main` de punta a punta desde `/tests`.

**Por qué:** la consigna era "UX no invasiva" con mensajes de error claros y sin tocar la
lógica de juego. Sin separar el parseo, la única forma de probarlo hubiera sido lanzar el
`.exe` como proceso aparte (lento, frágil, y sin forma limpia de capturar el código de
salida junto con stdout/stderr) o no probarlo. Devolver `int` en vez de usar
`Environment.Exit` importa especialmente para los tests: `Environment.Exit` mataría el
proceso del test runner si se llamara a `Program.Main` directo; con `return`, es una llamada
a método normal. El camino interactivo de "jugar" (que pide `Console.ReadLine`) se deja sin
probar por la misma razón de siempre: un test no tiene un humano tipeando.

---
