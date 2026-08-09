# Handoff — implementar el pico a pico (modo de a 6)

Prompt para arrancar el pico a pico en un chat nuevo de Claude Code, parado en la raíz
del repo. Copiá y pegá lo que está debajo de la línea.

---

Vamos a seguir el dominio de Truco Uruguayo en `/core`. Antes de escribir nada, leé
`CLAUDE.md`, `docs/RULES_Afinadas.md` completo, y en `docs/PREGUNTAS_ABIERTAS.md` la
sección **Solucionadas** (sobre todo **B10**, que especifica el modo de a 6) y las
**Notas de verificación**. Después leé todo `/core` y `/tests`.

## Contexto: qué está hecho

El dominio ya juega **1v1, 2v2 y 3v3 (redondilla)** completos y con todos los cantos
(truco, envido, flor con base + Con Flor Envido + Contra Flor al Resto + denuncia +
collera/trillera), irse al mazo, y acreditación diferida flor→envido→truco. Todo está
commiteado y hay ~206 tests en verde. Las preguntas abiertas están todas decididas.

El reductor es puro: `EstadoPartida` (record inmutable), `Partido.AccionesLegales` y
`Partido.Aplicar`. Ver la skill `core-dominio`.

**Ya está la base del pico a pico (paso 17b-1):** `EstadoPartida.Activos` es la lista
de jugadores que juegan la mano (vacío = todos). La baza cierra cuando tiraron los
activos, el turno cicla entre ellos, y envido/flor/denuncia cuentan sólo a los activos.
Ver `tests/JugadoresActivosTests.cs`.

## La tarea: el pico a pico

Falta implementar el modo de a 6, especificado en **B10** (`PREGUNTAS_ABIERTAS.md`,
Solucionadas). Resumen:

- El modo de a 6 (`cantidadJugadores == 6`) alterna **una redondilla (3v3) y un pico a
  pico**, hasta que un equipo llega a la mitad; de ahí en más, sólo redondillas.
- Un **pico a pico** son **tres manos de 1v1 en secuencia**, de un solo reparto (6 manos
  + muestra). Parejas por asiento: jugador `j` contra `j+3`. Con mano `m` (0-indexed), los
  tres picos son `(m, m+3)`, `(m+1, m+4)`, `(m+2, m+5)`, jugados en ese orden; el mano de
  cada pico es el jugador más bajo (`m`, `m+1`, `m+2`).
- **Repartidor:** rota una silla por reparto (uno para la redondilla, uno para el pico a
  pico). El mano de un reparto es `repartidor + 1`.
- **Transición a la mitad:** se termina el **estado** en curso (la redondilla, o el pico
  a pico entero = los 3) y recién ahí, si un equipo está en buenas, se sigue sólo con
  redondillas. Llegar al largo (fin de partido) corta en el acto.
- **Falta Envido = 6 y Contra Flor al Resto = 12** dentro del pico a pico (17b-3).
- Puntos siempre al equipo (en el 1v1 del pico, el equipo es el jugador).
- El toggle "jugar los 3 picos a la vez" es idea de diseño, **no** se implementa
  (OBSERVACIONES O3). La regla base es secuencial.

## Sub-pasos sugeridos (uno por vez, test primero, commit al cerrar)

- **17b-2 — Schedule del modo de a 6.** Máquina de estados redondilla ↔ pico a pico.
  Guía de arquitectura: agregá a `EstadoPartida` el estado del ciclo (p. ej. un enum
  `Redondilla`/`PicoAPico` y el índice de pico actual 0..2). En `Partido`, donde hoy
  `CerrarMano`/`TerminarMano` llaman a `RepartirMano` para la mano siguiente, ramificá
  para `cantidadJugadores == 6`: si venís de un pico y quedan picos, avanzá al pico
  siguiente (cambiás `Activos`, `Turno`, `Abridor` y reseteás baza/cantos/cobros, sin
  repartir de nuevo, usando las mismas 6 manos); si terminó el estado, decidí el próximo
  estado según el schedule y repartí. `RepartirMano` para un pico a pico setea las 6
  manos y arranca el pico 0. Ojo con `JugadorMano`/`Abridor` del pico (el más bajo de la
  pareja), que no siguen la fórmula `repartidor+1` de la redondilla.
- **17b-3 — Falta 6 y Contra Flor al Resto 12 en el pico.** Hoy `FaltaEnvido` se calcula
  contra el largo/mitad; en el pico a pico del modo de a 6, la Falta vale 6 fijo y la
  Contra Flor al Resto 12. Meté la condición.
- **17b-4 — Fuzz de a 6.** Como `DosVsDosTests.UnaPartida2v2Completa`, pero de 6:
  una partida entera (redondillas + picos alternados) termina, sin deadlock, con un solo
  equipo ganador y puntos que no decrecen.

## Cómo trabajar

- Un sub-paso por vez. Test que falla antes y pasa después. Al cerrar cada uno: qué
  cambiaste, por qué, y qué test lo cubre. Después parás.
- Ante una ambigüedad que no esté en `RULES_Afinadas.md` ni decidida en
  `PREGUNTAS_ABIERTAS.md`, **pará y preguntá** (no elijas un default silencioso).
- Alcance: sólo `/core` y `/tests`. `/game` y `/server` no se tocan.
- Mensajes de commit en español, imperativo, sin punto final, sin coautoría (ver la
  skill `commit`). No hagas `push` salvo que te lo pidan.

## Entorno

`dotnet` no está en el PATH (vive en `~/.dotnet`). Para build/test:

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1; export DOTNET_NOLOGO=1
dotnet test
```

Arrancá leyendo los archivos y confirmame el plan del 17b-2 (qué campos agregás a
`EstadoPartida` y cómo ramificás el schedule) antes de escribir código.
