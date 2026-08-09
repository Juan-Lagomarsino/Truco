# Preguntas abiertas

Casos que `RULES_Afinadas.md` no resuelve. **Ninguno se implementa sin decisión
escrita acá.** Cuando decidas una, la muevo a la sección **Solucionadas** (al
final) con su campo `Decisión` y la fecha; a partir de ahí es regla y va a
`RULES_Afinadas.md`.

Estado: `ABIERTA` / `DECIDIDA`

---

## Bloque B — Detectadas al leer el documento (no estaban anotadas)

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

## Solucionadas

Las que ya se decidieron. Cada una queda con su `Decisión` y la fecha; a partir de
ahí son regla y van a `RULES_Afinadas.md`.

### B1. Precedencia entre las reglas de recuento de flor
**Estado:** DECIDIDA (2026-08-08)
**Decisión:** Gana el recuento que da más tantos, que en la práctica equivale a la
precedencia por cantidad de piezas (3 piezas → 2 piezas → 1 pieza + 2 del mismo
palo → 3 del mismo palo). El motivo no es "que dé más" por sí mismo: con una o más
piezas siempre te quedás con el valor entero de la pieza más alta (2→30, 4→29,
5→28, 11→27, 10→27), y si hay una segunda pieza se le suma su segundo dígito
(unidades). Ej.: un 2 y un 4 del palo de la muestra dan 30 + 9 = 39 antes de la
tercera carta. Caso original: muestra 3 de Oro con 2/6/7 de Oro → "una pieza + dos
del mismo palo" = 30+6+7 = 43.

### C1. ¿El 12 espejo cuenta como pieza para formar y contar flor y envido?
**Estado:** DECIDIDA (2026-08-08)
**Decisión:** Sí, es pieza a todos los efectos: fuerza, valor y detección de
flor/envido. Un 12 espejo + otra pieza forma flor por "dos piezas".

### C2. ¿Existe un canto "Contra Flor" a secas?
**Estado:** DECIDIDA (2026-08-08)
**Decisión:** No existe. Los únicos cantos de flor son "La mía flor", "Con flor
envido" y "Contra flor al resto".
**Pendiente:** sacar `ContraFlor` del vocabulario de `CLAUDE.md` (queda
`ContraFlorAlResto`). Lo edita el autor.

### C3. Estructura de los cantos de flor
**Estado:** DECIDIDA (2026-08-08)
**Decisión:** Cada canto de flor es independiente; **no** forman una escalera de
reviro (a diferencia del envido).
**A checkear:** conviene verificarlo jugando en la mesa real. En algunas variantes
los cantos de flor sí escalan; si resultara que acá también, revisar esta decisión
junto con B3.

### D1. Quién reparte la primera mano
**Estado:** DECIDIDA (2026-08-08)
**Contexto:** el reductor necesita un repartidor inicial. El que reparte no es
mano; el mano es el jugador siguiente.
**Decisión:** es un parámetro al crear la partida, con default el jugador 0 (así el
jugador 1 es mano en la primera mano). El reparto rota una posición por mano.

### D2. Quién abre la baza siguiente tras una parda
**Estado:** DECIDIDA (2026-08-08)
**Contexto:** el documento dice que abre la baza siguiente el que ganó la anterior,
pero no dice qué pasa cuando la baza fue parda.
**Decisión:** tras una parda, abre el jugador que es mano en esa ronda. (En la
primera baza, que siempre abre el mano, coincide.)

### D3. Reparto y semilla en el reductor
**Estado:** DECIDIDA (2026-08-08)
**Contexto:** el reductor tiene que repartir cada mano de forma determinista.
**Decisión:** `EstadoPartida` guarda la semilla del barajado y el número de mano;
cada mano se reparte de forma determinista desde el estado (barajador con semilla
derivada de la base y el número de mano). Así se puede jugar y reproducir una
partida entera desde un test. El corte del rival queda como acción futura; con
barajado por semilla no hace falta para la equidad.

### A1. Hasta qué momento se puede tocar el envido
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** El envido se puede tocar durante la primera ronda, con gating por
jugador: un jugador que **ya tiró** su carta de la primera baza no puede iniciar el
envido; los que todavía no tiraron (incluidos sus compañeros) sí pueden. El reviro
está siempre permitido: aunque ya hayas tirado, podés revirar un envido en curso. La
ventana se cierra cuando termina la primera baza. En 1v1 se reduce a "el jugador en
turno puede tocar antes de jugar su primera carta".

### A2. ¿La flor es obligatoria?
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** No es estrictamente obligatoria. Para cobrar los tantos hay que
cantarla; si no la cantás, no los cobrás. Esconderla es válido, pero habilita que el
otro equipo **denuncie** ("tenías flor"): si la denuncia es correcta, los tantos de
la flor escondida pasan al equipo que denuncia. Interactúa con A5 (irse al mazo con
flor no cantada: esa flor no vale para el que se va).

### A3. Cuántos puntos entrega quien no quiere una Contra Flor al Resto
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** 3 (una flor), igual que no querer Con Flor Envido. Ver B3.

### A4. Falta Envido: ¿contra el final del partido o el final de las malas?
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** Contra el fin de la etapa del equipo que va primero. Si el que va
primero está en malas, la Falta vale los puntos que le faltan para llegar a la mitad
(fin de las malas); si ya está en buenas, los que le faltan para ganar el partido.

### A5. Qué pasa si el mano se va al mazo antes de tirar la primera carta
**Estado:** DECIDIDA (2026-08-09) — afirmada para implementar, con posible revisión.
**Decisión:** Se permite: entrega 1 punto (nada gritado) y la mano termina. Sobre la
flor: si se va al mazo y él (o alguien de su equipo) tenía flor y **no la cantó**, esa
flor no vale. Si tenía flor, **la canta y después se va**, son 3 de la flor y el punto
de la mano va al rival por irse; pero si el rival responde que también tiene flor, se
juegan las flores (enfrentamiento).

### B2. Cuántas veces se puede revirar el envido
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** Sin límite de reviros: se puede seguir tocando Envido / Real Envido
hasta que alguien quiera o no quiera. La Falta Envido es terminal (no se sube más).

### B3. "Con flor envido": cuánto entrega el que no quiere
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** 3, los tantos de la flor que ya estaba cantada.

### B4. Orden de resolución cuando quedan flor y truco pendientes
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** La flor se resuelve antes que el truco, misma lógica que el envido: se
resuelve, se suman los tantos, y recién ahí se contesta el truco.

### B5. Falta Envido cuando los dos equipos van iguales
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** Con empate de puntaje, ambos equipos tienen la misma Falta (da igual el
equipo). En el desempate del envido (puntos de envido iguales) gana el mano, como en
toda situación de empate.

### B6. Cierre del partido a mitad de mano
**Estado:** DECIDIDA (2026-08-09)
**Decisión:** Orden de acreditación de tantos dentro de una mano: flor → envido →
truco. El partido termina apenas un equipo llega al objetivo; si los tantos de flor ya
cruzan la meta, la mano no se termina de jugar.

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
