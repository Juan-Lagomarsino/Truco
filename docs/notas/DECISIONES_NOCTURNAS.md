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

---
