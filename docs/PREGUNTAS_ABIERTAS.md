# Preguntas abiertas

Casos que `RULES_Afinadas.md` no resuelve. **Ninguno se implementa sin decisión
escrita acá.** Cuando decidas una, escribí la respuesta en el campo `Decisión` y
la fecha; a partir de ahí es regla y va a `RULES_Afinadas.md`.

Estado: `ABIERTA` / `DECIDIDA`

---

## Bloque A — Las que ya estaban anotadas en las reglas

### A1. Hasta qué momento se puede tocar el envido
**Estado:** ABIERTA
**Opciones:** (a) durante toda la primera ronda; (b) hasta que el jugador mano
tira su primera carta; (c) hasta que cada jugador tira la suya, por jugador.
**Recomendación:** (a). Es la variante más común en Montevideo y la más simple de
modelar: la ventana se cierra al terminar la primera baza.
**Impacto:** `AccionesLegales` — define cuándo `Envido` está en la lista.

### A2. ¿La flor es obligatoria?
**Estado:** ABIERTA
**Opciones:** (a) obligatoria, se canta o se pierde; (b) opcional, se puede
esconder.
**Recomendación:** (a) obligatoria. Si es opcional se abre una cascada: un
jugador con dos piezas podría esconder flor y jugar envido, lo que rompe la
afirmación del documento de que "nunca vas a tener envido con dos piezas".
**Impacto:** grande. Toca el cálculo de envido, la validación de acciones y los
tests de invariantes.

### A3. Cuántos puntos entrega quien no quiere una Contra Flor al Resto
**Estado:** ABIERTA
**Opciones:** (a) 3 (una flor); (b) 4; (c) los tantos de las flores en juego.
**Recomendación:** (a) 3, por simetría con "el que no quiere entrega los puntos
del último canto querido", donde el último querido es la propia flor.

### A4. Falta Envido: ¿contra el final del partido o contra el final de las malas?
**Estado:** ABIERTA
**Contexto:** si el equipo que va primero todavía está en malas, hay dos
lecturas.
**Recomendación:** contra el final del partido, siempre. Es lo que dice la letra
actual del documento y evita un caso especial.
**Impacto:** interactúa con B5 (empate) y con el pico a pico, donde la falta vale
6 fijo.

### A5. Qué pasa si el mano se va al mazo antes de tirar la primera carta
**Estado:** ABIERTA
**Recomendación:** se permite, entrega 1 punto (nada gritado) y la mano termina.
El envido y la flor no llegaron a existir, así que no hay nada que resolver.
Pero ojo con la interacción con A2: si la flor es obligatoria y el que se va la
tenía, ¿la pierde o la cobra? Decidilo junto con esta.

---

## Bloque B — Detectadas al leer el documento (no estaban anotadas)

### B1. Precedencia entre las reglas de recuento de flor
**Estado:** ABIERTA
**El caso:** muestra 3 de Oro, mano = 2 de Oro, 6 de Oro, 7 de Oro. Aplican dos
reglas del documento a la vez: "una pieza + dos del mismo palo" da 30+6+7 = **43**,
y "tres del mismo palo" da 20+2+6+7 = **35**.
**Recomendación:** precedencia estricta por cantidad de piezas: tres piezas →
dos piezas → una pieza → tres del mismo palo. Da 43.
**Impacto:** alto y silencioso. Sin esta decisión, el orden de los `if` decide el
resultado.

### B2. Cuántas veces se puede revirar el envido
**Estado:** ABIERTA
**El caso:** el documento dice "sobre un envido se puede decir envido de nuevo",
y la tabla muestra `Envido, Envido` = 4. ¿Se admite un tercer envido? ¿Se admite
`Envido, Envido, Real Envido`?
**Recomendación:** máximo dos envidos acumulados; después solo Real Envido o
Falta Envido. Es lo habitual y acota el árbol de estados.

### B3. "Con flor envido": cuánto entrega el que no quiere
**Estado:** ABIERTA
**El caso:** el documento da el valor querido (5 tantos) y flaggea el no quiero
de la Contra Flor al Resto, pero no el de Con Flor Envido.
**Recomendación:** 3, los tantos de la flor que ya estaba cantada.

### B4. Orden de resolución cuando quedan flor y truco pendientes
**Estado:** ABIERTA
**El caso:** el documento fija que el envido va antes que el truco, pero no dice
lo mismo de la flor.
**Recomendación:** flor antes que truco, misma lógica que el envido: se resuelve,
se suman los tantos, y recién ahí se contesta el truco.

### B5. Falta Envido cuando los dos equipos van iguales
**Estado:** ABIERTA
**El caso:** "los puntos que le faltan al equipo que va primero" no está definido
si van empatados.
**Recomendación:** con empate, la falta se cuenta contra ese puntaje común (da lo
mismo qué equipo se tome). Verificá que el código no elija arbitrariamente el
equipo 0.

### B6. Cierre del partido a mitad de mano
**Estado:** ABIERTA
**El caso:** una mano puede repartir tantos de flor, de envido y de truco. Si los
de flor ya cruzan la meta, ¿termina ahí o se juega la mano completa?
**Recomendación:** el partido termina en el instante en que un equipo llega al
objetivo. Eso obliga a definir el **orden de acreditación** de los tantos dentro
de una mano: flor → envido → truco. Escribilo, porque decide partidas.

### B7. Irse al mazo en partidas de equipo
**Estado:** ABIERTA
**El caso:** en 2v2 y 3v3, ¿irse al mazo es acción del jugador o del equipo?
**Recomendación:** del equipo. El jugador que se va abandona la mano entera para
su lado y los puntos en juego pasan al rival. Modelarlo como acción individual
que descarta solo a un jugador multiplicaría los casos de resolución de bazas.

### B8. Desempate de envido: definición exacta de "más cerca de la mano"
**Estado:** ABIERTA
**El caso:** con 4 y 6 jugadores hace falta una definición operativa.
**Recomendación:** distancia = cantidad de posiciones en sentido antihorario
desde el jugador mano. El mano tiene distancia 0 y gana todos los empates.
**Impacto:** también aplica al desempate de flor.

### B9. Quién puede responder un canto
**Estado:** ABIERTA
**El caso:** el documento dice que se grita truco "cuando es tu turno", pero no
dice quién del equipo rival contesta quiero / no quiero, ni si un compañero
puede contestar antes.
**Recomendación:** cualquier jugador del equipo rival puede contestar; el primero
que contesta compromete al equipo. Necesario para que `AccionesLegales` sepa a
quién ofrecerle la respuesta.

### B10. Rotación del pico a pico en partidas de a 6
**Estado:** ABIERTA
**El caso:** el documento dice que se alternan una redondilla y tres manos de
uno contra uno, pero no dice qué jugador enfrenta a cuál en cada una de las tres,
ni cómo rota el reparto.
**Recomendación:** dejar el modo de 6 jugadores **fuera del alcance inicial**.
Implementá 1v1 y 2v2, dejá la estructura preparada, y volvé al 3v3 cuando la
mesa esté jugable. Es el modo con más reglas propias y menos especificadas.

---

## Notas de verificación

Cosas que revisé del documento y **cierran bien**, para que no se re-discutan:

- La tabla de 19 niveles cubre exactamente las 40 cartas, tanto cuando la muestra
  es pieza (el 12 espejo sube y deja 3 doces en el nivel 13) como cuando no lo es
  (los 4 doces en el nivel 13).
- Envido máximo 37 (pieza 2 = 30, más un 7) y flor máxima 47 (tres piezas: 30 + 9
  + 8) coinciden con lo que dice el documento.
- Flor mínima 20 (tres del mismo palo siendo 10, 11 y 12, que valen 0).
- Los cinco ejemplos numéricos del documento dan lo que dicen que dan.
