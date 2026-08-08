---
name: truco-uruguayo
description: Reglas, jerarquía y recuento de tantos del Truco Uruguayo (con muestra, piezas y flor). Usá esta skill SIEMPRE que el trabajo toque cartas, fuerza, cantos, envido, flor, truco, piezas, muestra, resolución de bazas, puntajes o cualquier lógica dentro de /core, aunque el pedido no nombre "reglas" explícitamente.
when_to_use: envido, real envido, falta envido, flor, contraflor, truco, retruco, vale cuatro, pieza, muestra, mata, chica, negra, blanca, mano, pie, baza, parda, tanto, irse al mazo, redondilla, pico a pico, /core, jerarquía de cartas, puntaje
---

# Truco Uruguayo — dominio

## Regla cero

`docs/RULES_Afinadas.md` es la ÚNICA fuente de las reglas. Leelo completo antes
de escribir o corregir lógica de reglas. Esta skill es un índice y una lista de
trampas, no un reemplazo del documento.

**El Truco Uruguayo no es el argentino.** Tiene muestra, piezas y flor. Si una
regla que recordás no está en `RULES_Afinadas.md`, estás recordando otro juego.
No la implementes: anotala como pregunta.

**Ante contradicción entre código y documento, gana el documento** — y avisale
al autor, porque probablemente el código lo escribió él a mano.

## Antes de implementar cualquier caso dudoso

Consultá `docs/PREGUNTAS_ABIERTAS.md`. Ese archivo contiene los casos que las
reglas no resuelven, cada uno con su decisión. Si el caso que estás por
implementar está ahí sin decidir, **parás y preguntás**. No elijas un default
"razonable" en silencio: un default silencioso acá se convierte en un bug que
solo aparece jugando.

## Trampas conocidas

Estas son las cosas que se implementan mal si se leen rápido:

**El 12 espejo tiene condición.** El 12 del palo de la muestra copia a la
muestra *solo si el número de la muestra es pieza* (2, 4, 5, 11 o 10). Si la
muestra es, por ejemplo, un 3 de Oro, el 12 de Oro es un 12 común y va al nivel
13 junto con los otros tres. El espejo aplica tanto a la fuerza como al recuento
de tantos.

**Las piezas las define el palo, no el número.** Con muestra 3 de Oro, el 2, 4,
5, 11 y 10 de Oro siguen siendo piezas aunque la muestra no lo sea.

**La carta de la muestra no se reparte.** Las manos salen de 39 cartas, no de
40. Pero la tabla de fuerza sí la incluye: es un orden, no un inventario.

**11 y 10 de la muestra valen los dos 27 al contar**, aunque para la fuerza el
11 le gane al 10. Fuerza y tantos son dos funciones distintas; no las unifiques.

**Empate de nivel es parda, no error.** Dos cartas del mismo nivel de la tabla
(por ejemplo 3 de Oro y 3 de Copa) empatan.

**El envido va antes que el truco.** Si se gritó truco y el envido todavía está
disponible, primero se resuelve el envido y se suman esos puntos, después se
contesta el truco.

**La flor anula el envido**, no al revés.

**Solo revira el equipo que quiso el canto anterior.** Nunca se revira el canto
propio.

**Las unidades de una pieza son su último dígito**: 2→30→0, 4→29→9, 5→28→8,
11→27→7, 10→27→7. Se usan al contar flores con dos o tres piezas.

## Invariantes que deben quedar cubiertas por tests

- El mazo tiene 40 cartas; los números son 1,2,3,4,5,6,7,10,11,12 y los palos
  Basto, Oro, Espada, Copa.
- La tabla de fuerza cubre exactamente 40 cartas para cualquier muestra.
- Nunca hay dos cartas jugables con la misma fuerza de pieza.
- Envido de una mano ∈ [0, 37]. Flor de una mano ∈ [20, 47].
- Una mano no puede tener envido con dos piezas (dos piezas ya es flor), salvo
  que la decisión sobre flor opcional lo permita — ver PREGUNTAS_ABIERTAS.
- Ninguna mano puede terminar sin ganador.
- El puntaje de un equipo nunca decrece.

## Fuera de alcance por ahora

**Señas.** El documento las menciona pero no las especifica. No las modeles ni
dejes ganchos especulativos: cuando exista la tabla de seña por carta, se diseña
con datos reales.

**Barajado realista, corte, y trampas.** Son ideas anotadas para más adelante.
El barajado de `/core` es una permutación determinista a partir de una semilla
inyectada. La animación es problema de Unity, no del dominio.
