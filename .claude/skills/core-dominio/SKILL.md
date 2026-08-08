---
name: core-dominio
description: Reglas de arquitectura del proyecto /core, la librería de lógica pura del juego en C#. Usá esta skill siempre que vayas a crear, mover o modificar tipos en /core, elegir una firma de método, agregar una dependencia, o decidir dónde vive una responsabilidad.
when_to_use: /core, dominio, arquitectura, EstadoPartida, Accion, reductor, determinismo, inmutable, record, dependencia, namespace, dónde va esta clase
---

# Arquitectura de /core

## Qué es /core

Una librería de clases .NET que contiene el juego entero como función pura.
Sin motor, sin red, sin interfaz, sin reloj. Si `/core` está bien hecho, se
puede jugar una partida completa desde un test.

## La forma de la API

Todo el dominio se expresa como un reductor:

```csharp
EstadoPartida                                          // inmutable, serializable
IReadOnlyList<Accion> AccionesLegales(EstadoPartida e, JugadorId j)
EstadoPartida Aplicar(EstadoPartida e, Accion a)       // lanza si es ilegal
```

**Por qué importa esta forma y no otra:** Unity va a dibujar `EstadoPartida` y
mandar `Accion`. SignalR va a mandar la misma `Accion` por la red sin duplicar
una línea de reglas. El bot va a ser una función de `AccionesLegales` a
`Accion`. Y cada test es un `Aplicar` encadenado sobre un estado literal.

Si en cambio el estado muta por adentro, todo eso hay que reescribirlo en la
fase de Unity. Antes de proponer cualquier diseño que rompa esta forma,
preguntá.

**`AccionesLegales` es la única autoridad sobre qué se puede hacer.** No
disperses validación por la UI ni por los handlers de cada canto. Si un jugador
no puede cantar envido en este momento, `Envido` no aparece en la lista, y
`Aplicar` lo rechaza. Una sola definición, dos usos.

## Prohibiciones

Estas no son preferencias de estilo. Cada una rompe una capacidad concreta:

| Prohibido | Qué rompe |
| --- | --- |
| `System.Random` sin semilla inyectada | Reproducir bugs, grabar partidas, sincronía cliente-servidor |
| `DateTime.Now` / `UtcNow` | Determinismo de los tests |
| `Console`, I/O de archivos, `HttpClient` | Portabilidad a Unity y al servidor |
| `async` / `Task` | Nada en `/core` espera nada |
| Estado estático mutable | Tests en paralelo, múltiples partidas simultáneas |
| `using UnityEngine` | Todo |

Si te parece que necesitás una de estas, es señal de que la responsabilidad va
en otra capa. Decilo antes de implementarla.

## Convenciones de tipos

- Valores del dominio (`Carta`, `Muestra`, `Tanto`, `JugadorId`, `EquipoId`) van
  como `readonly record struct` o `record`. Igualdad por valor, sin sorpresas.
- `EstadoPartida` y todo lo que cuelgue de él son inmutables. Los cambios se
  expresan con `with`.
- Colecciones expuestas siempre como `IReadOnlyList<T>` / `IReadOnlyDictionary<,>`.
- Nada de `null` en el dominio: usá tipos explícitos para la ausencia
  (`Baza.Parda`, `Canto.Ninguno`) en vez de nullables sueltos.
- Nullable reference types habilitado en el `.csproj`.

## Separación interna

Mantené separadas tres cosas que es tentador mezclar:

1. **Fuerza** de una carta (para resolver bazas) — depende de la muestra.
2. **Tantos** de una carta (para envido y flor) — depende de la muestra, y NO es
   una función monótona de la fuerza. El 11 y el 10 de la muestra tienen distinta
   fuerza y los mismos tantos.
3. **Identidad** de una carta (número + palo) — no depende de nada.

Un mismo tipo `Carta` con tres funciones distintas sobre ella. No metas la
fuerza adentro de `Carta`.

## Vocabulario

Términos del dominio en español rioplatense, sin traducir. Estructura del
lenguaje en inglés. Ver `CLAUDE.md` para la lista.
