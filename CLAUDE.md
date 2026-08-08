# Truco Uruguayo

Juego de cartas de Truco Uruguayo para Android, iOS y Steam. Desarrollo solo.
Objetivo doble: publicar el juego y aprender game dev en el camino.
Preferencia explícita del autor: entender antes que ir rápido.

## Advertencia crítica: esto NO es el truco argentino

El Truco Uruguayo difiere del argentino en cosas estructurales: existe la
**muestra** (una carta que define un palo especial), existen las **piezas**
(cinco cartas que quedan por encima de las matas), existe la **flor**, y el
recuento de tantos es distinto.

**Si algo que "sabés" sobre truco no está escrito en `docs/RULES_Afinadas.md`,
asumí que estás equivocado.** No completes huecos de memoria con reglas
argentinas. Anotá el hueco como pregunta y esperá respuesta.

Esta es la fuente número uno de bugs en este proyecto. Un error de este tipo
pasa los tests, porque los tests se escriben con la misma regla equivocada.

## Fuentes de verdad

| Archivo | Qué es |
| --- | --- |
| `docs/RULES_Afinadas.md` | Las reglas. Fuente única. Si el código la contradice, gana el documento. |
| `docs/PREGUNTAS_ABIERTAS.md` | Casos que las reglas no resuelven. Ninguno se implementa sin decisión escrita acá. |

## Estructura del repo

| Ruta | Qué es | Estado |
| --- | --- | --- |
| `/core` | Lógica pura del juego en C#. Cero dependencias de motor. | En construcción |
| `/tests` | xUnit. Los tests son la especificación ejecutable de las reglas. | En construcción |
| `/game` | Unity 6. | **NO TOCAR** (Fase 4) |
| `/server` | ASP.NET Core + SignalR. | **NO TOCAR** (fase posterior) |
| `/docs` | Vault de Obsidian. Markdown plano en la raíz, sintaxis Obsidian libre en `/docs/notas`. | Activo |

## Reglas duras de `/core`

`/core` es una librería determinista y pura. Prohibido:

- `System.Random` sin semilla inyectada por constructor
- `DateTime.Now` / `DateTime.UtcNow`
- `Console`, ficheros, red, `Task`/`async`
- Estado estático mutable
- Cualquier `using UnityEngine`

La razón no es purismo: sin determinismo no se puede reproducir un bug, ni
grabar partidas, ni evitar que cliente y servidor diverjan cuando llegue el
multijugador.

## Forma de la API del dominio

El dominio se modela como un reductor puro:

```csharp
EstadoPartida                                          // inmutable, serializable
IReadOnlyList<Accion> AccionesLegales(EstadoPartida e, JugadorId j)
EstadoPartida Aplicar(EstadoPartida e, Accion a)       // lanza si la acción es ilegal
```

Esto es lo que hace que Unity solo dibuje estado y mande acciones, que SignalR
mande las mismas acciones por la red sin duplicar lógica, y que el bot sea una
función de `AccionesLegales` a `Accion`.

## Vocabulario

Los términos del dominio van en español rioplatense y no se traducen:
`Envido`, `RealEnvido`, `FaltaEnvido`, `Flor`, `ContraFlor`, `ContraFlorAlResto`,
`Truco`, `Retruco`, `ValeCuatro`, `Muestra`, `Pieza`, `Mata`, `Mano`, `Pie`,
`Baza`, `Parda`, `Tanto`, `IrseAlMazo`, `Redondilla`, `PicoAPico`, `Collera`,
`Trillera`, `Seña`.

La estructura del código va en inglés (`IReadOnlyList`, `Assert`, `record`).

## Comandos

```bash
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~Jerarquia"
```

## Cómo trabajar acá

- Un paso del plan por vez. Al terminar cada uno: qué cambió, por qué, qué test lo cubre.
- Todo cambio de reglas viene con un test xUnit que falla antes y pasa después.
- El autor escribe la lógica de reglas a mano a propósito. Tu rol es revisar,
  corregir, cerrar huecos y armar andamiaje — no reescribir su código sin avisar.
- Preguntá antes de: agregar dependencias, crear proyectos nuevos, refactors
  grandes, o tocar archivos fuera de `/core` y `/tests`.
