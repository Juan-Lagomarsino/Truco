# Observaciones de implementación

Decisiones de implementación que no son reglas del juego (no van a
`RULES_Afinadas.md` ni a `PREGUNTAS_ABIERTAS.md`), pero conviene dejar anotadas.

---

## O1. Reparto round-robin

**Fecha:** 2026-08-08
**Dónde:** `core/Mazo.cs`, método `Repartir`.

El mazo se reparte round-robin: la carta `i` va al jugador `i % cantidadJugadores`
(una carta a cada jugador por vuelta, tres vueltas). El documento no especifica la
convención de reparto.

**Por qué no afecta el juego:** la muestra es la carta en la posición `3 × jugadores`
sin importar la convención, y sobre un mazo ya barajado da igual repartir round-robin
que en bloques — sólo cambia qué carta física le toca a cada jugador para una semilla
dada. Round-robin es además un método sólido de randomizar un arreglo.

**Pendiente (fase de juego):** cuando se anime el reparto en Unity, habrá que
reproducir visualmente este orden de reparto para que se vea como en la mesa real.

---

## O2. El envido lo inicia el jugador en turno (no es out-of-turn)

**Fecha:** 2026-08-09
**Dónde:** `core/Partido.cs`, `PuedeIniciarEnvido` / `AccionesLegales`.

El juego es siempre por turnos: en cada momento hay exactamente un jugador que le
toca, nunca se elige quién tira. El canto (envido, flor) lo inicia el jugador en
turno, que es como está implementado. La cláusula de A1 sobre "los compañeros que no
tiraron" se refiere a que un compañero puede cantar cuando le llega su turno, no fuera
de turno. No hay refinamiento pendiente acá: el modelo por turnos es el correcto.

---

## O3. Pico a pico: posibilidad de jugar los tres a la vez

**Fecha:** 2026-08-09

En el modo de a 6, el pico a pico son tres manos de 1v1 que se juegan en secuencia
(mientras una se juega, los otros cuatro esperan sin ver sus cartas). El juego real es
así, secuencial. **Posibilidad de diseño (no implementar por ahora):** un toggle para
jugar los tres pico a pico a la vez y hacerlo más fluido. El diseño del dominio debería
dejar la puerta abierta a esto, pero la regla base es secuencial.

