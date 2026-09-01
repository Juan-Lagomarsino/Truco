---
estado: en progreso — corrida nocturna
tags: [nocturno, hallazgos]
---

# Hallazgos de la corrida nocturna 2

Casos donde un test nuevo mostró que `/core` contradice `RULES_Afinadas.md` (o se
contradice a sí mismo: `AccionesLegales` y `Aplicar` en desacuerdo). Política del plan: no
se parchea la lógica del autor a escondidas, se documenta acá con reproducción mínima y el
suite queda en verde (test marcado `Skip` si hace falta).

---

## H1. `Aplicar` acepta un `CantarEnvido` de apertura que `AccionesLegales` no ofrece

**Encontrado por:** `tests/InvariantesFuzzTests.cs`, propiedad A1 ("toda acción que no está
en `AccionesLegales` tiene que hacer lanzar a `Aplicar`"). Falló para las 9 combinaciones
de (cantidadJugadores, semilla) del Theory, siempre en el primer estado de la partida.

**Repro mínima:**

```csharp
var e = Partido.Nueva(largo: 30, semilla: 3, cantidadJugadores: 2);
// e.Turno.Valor == 1 (jugador 1 es mano; repartidor por defecto es 0)
Partido.AccionesLegales(e, new JugadorId(0)).Count // == 0 (no es su turno)

var accion = new CantarEnvido(new JugadorId(0), EnvidoCanto.Envido);
var resultado = Partido.Aplicar(e, accion); // NO LANZA
// resultado.EnvidoPendiente == EstadoEnvido { Ultimo = Envido, ValorSiQuiero = 2,
//                                              ValorSiNoQuiero = 1, Responde = EquipoId(1) }
```

`AccionesLegales(e, jugador0)` dice que el jugador 0 no tiene ninguna acción legal (no es
su turno, no hay nada pendiente). Pero `Partido.Aplicar` igual acepta que abra el envido, y
deja el estado con un envido pendiente como si fuera válido.

**Diagnóstico — dónde diverge el código (`core/Partido.cs`):**

- `AccionesLegales`, en el caso "turno libre" (sin flor/envido/truco pendiente), primero
  filtra `if (!jugador.Equals(e.Turno)) return Array.Empty<Accion>();` (línea ~75-76) y
  recién después, sólo para `e.Turno`, llama a `AgregarAperturasDeEnvido`. Es decir: **para
  abrir el envido en un turno libre, `AccionesLegales` exige que sea tu turno.**
- `AplicarCantarEnvido`, en la rama de apertura (cuando `!e.HayEnvidoPendiente`), valida con
  `PuedeIniciarEnvido(e, ce.Jugador)` (línea ~382), que es:
  ```csharp
  !e.EnvidoJugado && !e.FlorResuelta && e.BazasGanadas.Count == 0
      && !e.JugadasBaza.Any(j => j.Jugador.Equals(jugador))
  ```
  **Esto no chequea `jugador.Equals(e.Turno)` en ningún lado.** Sólo exige que ese jugador
  todavía no haya tirado su carta en la baza en curso — nada sobre de quién es el turno.

Resultado: cualquier jugador que todavía no tiró (incluido uno cuyo turno ni siquiera
llegó) puede "abrir" el envido vía `Aplicar` aunque `AccionesLegales` no se lo ofrezca.

**Por qué no lo toqué:** `PuedeIniciarEnvido` es lógica de reglas escrita a mano por el
autor (la firma coincide con la decisión A1 de `PREGUNTAS_ABIERTAS.md`: *"los que todavía
no tiraron (incluidos sus compañeros) sí pueden"* tocar envido). Es genuinamente ambiguo
si el bug está en `PuedeIniciarEnvido` (le falta el chequeo de turno) o en
`AccionesLegales` (es más restrictiva de lo que dice la regla, y en 2v2/3v3 le estaría
negando incorrectamente a un compañero que todavía no tiró la chance de abrir el envido
antes de que le llegue el turno a él mismo). Elegir cuál lado es el correcto es decidir una
regla del truco, no algo que me toque resolver solo.

**Pregunta relacionada:** ver `PREGUNTAS_PENDIENTES.md` P17.

**Cómo se manejó el test:** `InvariantesFuzzTests.Aplicar_ConAccionQueNoEsLegal_...` excluye
explícitamente esta forma exacta de acción (`CantarEnvido` de apertura donde
`PuedeIniciarEnvido` daría `true`) de la aserción "tiene que lanzar", con un comentario que
apunta acá, para poder seguir verificando A1 sobre todo lo demás sin ensuciar el suite.
Además hay una reproducción mínima marcada `[Fact(Skip = ...)]` en
`tests/InvariantesFuzzTests.cs` (`H1_CantarEnvidoDeAperturaFueraDeTurno_DeberiaLanzarPeroNoLanza`)
para que quede visible y no se repita el hueco si alguien la habilita sin arreglar nada.
