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
