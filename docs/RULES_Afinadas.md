# Reglas del Truco Uruguayo (con muestra y flor)

## Explicacion de la Logica del Juego

El juego consiste en dos equipos. Si se juega de a 2 es 1vs1, si se juega de a 4 es 2vs2, y si se juega de a 6 es 3vs3. Los jugadores de un equipo se sientan intercalados con los del otro.

La gracia del juego es sumar puntos. El juego se puede jugar a los puntos que quieras, pero siempre se va a partir en dos etapas. Buenas y malas. Si jugas a 40 es 20 y 20, y asi con todo. Mientras estas por debajo de la mitad estas en malas, cuando llegas a la mitad estas en buenas.

Se juega con el mazo español de 40 cartas, sin 8, sin 9 y sin comodines.

Como ganas puntos:

- Ganando la mano en el truco. Vale 1 punto si no se grito nada, o 2, 3 o 4 si se grito Truco, Retruco o Vale Cuatro y el otro equipo quiso.
- Ganando el envido. Vale 2, 3 o la falta segun lo que se haya tocado y querido.
- Cantando flor. Vale 3 puntos por cada flor, o mas si hay enfrentamiento de flores.
- Cuando el rival no quiere un canto. Te llevas los puntos del ultimo canto que si se quiso, y si no se quiso ninguno te llevas 1.
- Cuando el rival se va al mazo. Te llevas los puntos que estaban en juego en ese momento.

Como se juega cada ronda. Arranca tirando el jugador que es mano (El que esta seguido a el jugador que repartio). Despues sigue el de su derecha, y asi sucesivamente en sentido antihorario. El que tiro la carta mas alta gana la ronda para su equipo y es el que arranca tirando la ronda siguiente.

---

## La Muestra

Despues de repartir las tres cartas a cada jugador se saca una carta mas del mazo y esa es la muestra. Se deja boca arriba abajo del mazo, de manera que se vea, y el mazo queda a la derecha del que repartio apuntando al que es mano.

La muestra define el palo de las piezas, que son las cinco cartas mas fuertes de la mano. La carta que es la muestra no la tiene nadie, o sea que las manos se reparten sobre 39 cartas y no sobre 40.

Ejemplo si la muestra es el 4 de Copa, entonces el 2, 4, 5, 11 y 10 de Copa son piezas, pero como el 4 de Copa esta debajo del mazo en esa mano solo se pueden jugar cuatro de las cinco piezas, si alguno tuviera el 12 de Copa podria jugar ese 4 de copa que esta en la muestra.

---

## Jerarquia completa de las Cartas

**El orden de las cartas va de mejor a peor.**

Piezas (Se le llama piezas a el 2,4,5,11,10 cuando son del mismo palo que la muestra)

- 2 de la Muestra
- 4 de la Muestra
- 5 de la Muestra
- 11 de la Muestra
- 10 de la Muestra

Matas (Se le llama matas a 4 cartas puntuales)

- 1 de Espada
- 1 de Basto
- 7 de Espada
- 7 de Oro

Chicas (Se le llama chicas)

- 3 de Cualquier palo
- 2 de Cualquier palo menos la muestra
- 1 de Cualquier palo menos Espada y Basto

Negras (Se le llama negras)

- 12 de Cualquier palo (El 12 de la muestra, es una carta especial. Si la muestra es una pieza, es decir 2,4,5,11,10 el 12 actua de espejo, copiando asi la carta que es la muestra. Ejemplo si la muestra es un 2 de Oro y tengo un 12 de Oro, en realidad tengo un 2 de Oro. Si la muestra es un 1 de Oro es decir una muestra que no es Pieza, entonces mi 12 de Oro es un 12 normal.)
- 11 de Cualquier palo menos la muestra
- 10 de Cualquier palo menos la muestra

Blancas (Se le llama blancas)

- 7 de Cualquier palo menos Espada y Oro
- 6 de Cualquier palo
- 5 de Cualquier palo menos la muestra
- 4 de Cualquier palo menos la muestra

Como el 12 de la muestra pasa a ocupar el lugar de la pieza que quedo dada vuelta, nunca hay dos cartas con la misma fuerza de pieza.

---

## Jerarquia en formato programacion
 
**El orden de las cartas va de mejor a peor.**
 
### Carta = (Numero, Palo)
 
> Numero: Refiere al numero que tiene esa carta. Este numero pertenece a N = [1,2,3,4,5,6,7,10,11,12]
 
> Palo: Refiere al palo que tiene esa carta. Este palo pertenece a P = [Basto, Oro, Espada, Copa]
 
Sea la muestra (x,y) para la especificacion.
 
Sea Piezas = [2,4,5,11,10] (en ese orden, que es el orden de fuerza).
 
### Tabla de fuerza
 
1. (2, y)
2. (4, y)
3. (5, y)
4. (11, y)
5. (10, y)
6. (1, Espada)
7. (1, Basto)
8. (7, Espada)
9. (7, Oro)
10. (3, ∀ p ∈ P)
11. (2, ∀ p ∈ P / p != y)
12. (1, ∀ p ∈ P / p != Espada && p != Basto)
13. (12, ∀ p ∈ P / !(x ∈ Piezas && p = y))
14. (11, ∀ p ∈ P / p != y)
15. (10, ∀ p ∈ P / p != y)
16. (7, ∀ p ∈ P / p != Espada && p != Oro)
17. (6, ∀ p ∈ P)
18. (5, ∀ p ∈ P / p != y)
19. (4, ∀ p ∈ P / p != y)

Tres cosas importantes de esta tabla:
 
- El nivel 13 saca al 12 del palo de la muestra solo cuando la muestra es pieza, porque en ese caso ese 12 es espejo y ya esta arriba de todo, en el nivel que le corresponde a la pieza que copia. Si la muestra no es pieza, los cuatro 12 van en el nivel 13.
- Esto es un orden de fuerza, no un inventario de cartas repartibles. La carta que es la muestra igual aparece en la tabla aunque no la tenga nadie.
- Dos cartas que caen en el mismo nivel empatan y van parda. Ejemplo el 3 de Oro y el 3 de Copa estan las dos en el nivel 10.

---

## Como se resuelve la mano

La mano son tres rondas. Gana la mano el equipo que gane dos rondas. Si un equipo gana las dos primeras, la tercera no se juega.

| Primera | Segunda | Tercera | Gana la mano |
| --- | --- | --- | --- |
| A | A | No se juega | A |
| A | B | A | A |
| A | B | B | B |
| A | B | Parda | A |
| A | Parda | No se juega | A |
| Parda | A | No se juega | A |
| Parda | Parda | A | A |
| Parda | Parda | Parda | El que es mano |

La regla corta es: si hay parda, gana el que gano la primera ronda que no fue parda. Si todas son pardas, gana el que es mano.

---

## El toque de envido. Que es, como contarlo y como jugarlo.

El envido es apostar a quien tiene mas puntos en la mano. Se toca en la primera ronda. Cuando se quiere, el que es mano canta sus puntos y despues siguen los demas en sentido antihorario, pero solo cantan si superan lo que ya se dijo. El que no supera dice "son buenas". Si hay empate gana el que esta mas cerca de la mano.

Una mano puede tener entre 0 y 37 puntos.

### Cuanto vale cada carta

- 1, 2, 3, 4, 5, 6 y 7 que no son piezas valen su numero.
- 10, 11 y 12 que no son piezas valen 0.
- Las piezas valen: 2 de la muestra 30, 4 de la muestra 29, 5 de la muestra 28, 11 de la muestra 27, 10 de la muestra 27.
- El 12 de la muestra cuando es espejo vale lo mismo que la pieza que esta copiando.

Ojo que el 11 y el 10 de la muestra valen los dos 27, aunque para la fuerza el 11 le gana al 10.

### Como se cuenta

- Tres cartas de palos distintos y sin pieza: valen los puntos de la carta mas alta.
- Dos cartas del mismo palo y sin pieza: 20 + la suma de esas dos cartas.
- Con una pieza: los puntos de la pieza + los puntos de la mejor de las otras dos cartas, sin importar el palo.

Nunca vas a tener envido con dos piezas, porque dos piezas ya es flor.

Ejemplo la muestra es 3 de Oro y tengo 2 de Oro, 7 de Oro y 5 de Copa. El 2 de Oro es pieza y vale 30, la mejor de las otras dos es el 7, entonces tengo 37. Ese es el envido mas alto posible.

Ejemplo tengo 6 de Basto, 5 de Basto y 11 de Copa, sin piezas. Son 20 + 6 + 5 = 31.

Ejemplo tengo 12 de Espada, 10 de Oro y 11 de Copa, sin piezas y los tres palos distintos. Tengo 0.

### Cuanto se juega

| Toque | Puntos |
| --- | --- |
| Envido | 2 |
| Real Envido | 3 |
| Falta Envido | Los puntos que le faltan para terminar el partido al equipo que va primero |

Se puede revirar. Sobre un envido se puede decir envido de nuevo, real envido o falta envido, y asi hasta que alguien diga quiero o no quiero. El que no quiere entrega los puntos del ultimo canto que se habia querido, y si no se habia querido ninguno entrega 1.

| Cantos | Resultado |
| --- | --- |
| Envido, Quiero | 2 al que tiene mas puntos |
| Envido, No quiero | 1 al que toco |
| Envido, Envido, Quiero | 4 al que tiene mas puntos |
| Envido, Envido, No quiero | 2 al que toco el segundo envido |
| Real Envido, Quiero | 3 al que tiene mas puntos |
| Envido, Real Envido, Quiero | 5 al que tiene mas puntos |
| Falta Envido, Quiero | La falta al que tiene mas puntos |

El envido va primero. Si se grito truco y todavia no se jugo el envido, se puede tocar envido igual. Primero se resuelve el envido, se suman los puntos, y despues se contesta el truco.

---

## El canto de flor. Que es, como contarla y como jugarla.

La flor son 3 puntos y hay que cantarla en el primer turno, antes de tirar la carta a la mesa en la primera ronda. Si tenes flor y no la cantas a tiempo, la perdiste.

La flor invalida el envido. Si alguien canta flor no se puede tocar envido, y si alguien ya toco envido pero el otro equipo tiene flor, la flor lo anula.

### Que es flor

- Tres cartas del mismo palo.
- Una pieza + dos cartas del mismo palo entre ellas.
- Dos piezas + cualquier carta.
- Tres piezas.

En todas estas formas, y tambien en el envido, el 12 que actua de espejo cuenta como la pieza que copia. Si la muestra es un 2 de Oro, un 12 de Oro cuenta como un 2 de Oro tambien para armar flor: un 12 de Oro espejo mas otra pieza ya es flor de dos piezas.

### Como se cuenta

- Tres del mismo palo: 20 + la suma de las tres.
- Una pieza + dos del mismo palo: los puntos de la pieza + la suma de las otras dos.
- Dos piezas + una carta: los puntos de la pieza mas alta + las unidades de la segunda pieza + los puntos de la tercera carta.
- Tres piezas: los puntos de la pieza mas alta + las unidades de las otras dos.

Cuando una mano entra en mas de una de estas reglas, se cuenta por la que tiene mas piezas: primero tres piezas, despues dos piezas, despues una pieza mas dos del mismo palo, y por ultimo tres del mismo palo. Como una pieza siempre aporta su valor entero, esa es ademas la cuenta que da mas tantos. Ejemplo, con muestra 3 de Oro y 2, 6 y 7 de Oro se cuenta como una pieza mas dos del mismo palo (30 + 6 + 7 = 43), no como tres del mismo palo (35).

Las unidades son el ultimo digito del valor de la pieza. El 2 de la muestra vale 30 y sus unidades son 0, el 4 vale 29 y sus unidades son 9, el 5 vale 28 y sus unidades son 8, el 11 y el 10 valen 27 y sus unidades son 7.

La flor mas debil es 20 y la mas fuerte es 47.

Ejemplo la muestra es 3 de Copa y tengo 2 de Copa, 4 de Copa y 5 de Copa. Son tres piezas. La mas alta es el 2 que vale 30, y le sumo las unidades del 4 que son 9 y las del 5 que son 8. Tengo 47.

Ejemplo tengo 7, 6 y 12 de Espada sin piezas. Son 20 + 7 + 6 + 0 = 33.

Ejemplo la muestra es 3 de Oro y tengo 5 de Oro, 7 de Basto y 6 de Basto. Es una pieza + dos del mismo palo. El 5 de Oro vale 28 y le sumo 7 + 6. Tengo 41.

### Como se juega

Si mas de un jugador tiene flor, todos la tienen que cantar enseguida de que se canto la primera, sin esperar su turno. Cuando dos del mismo equipo tienen flor se le dice collera, y cuando son tres se le dice trillera.

Si las flores son todas de un mismo equipo, ese equipo se lleva 3 puntos por cada flor.

Si hay flores en los dos equipos se enfrentan.

| Canto | Como se resuelve |
| --- | --- |
| La mia flor | Se cuentan al final de la mano, 3 tantos para la flor mas alta |
| Con flor envido | Se cuentan en el momento, 5 tantos para la flor mas alta |
| Contra flor al resto | Se muestran las flores, la mas alta se lleva la falta del que va ganando mas los puntos de las flores en juego |

---

## El grite de Truco/Retruco/ValeCuatro. Que es, como cuenta y como jugarlo.

La mano por defecto vale 1 punto. En cualquier momento de la mano, cuando es tu turno, podes gritar truco y la subis a 2. El otro equipo dice quiero o no quiero. Si no quiere, el que grito se lleva el punto de la mano y se termina ahi.

Se puede seguir revirando, pero siempre revira el equipo que quiso el canto anterior. Nunca podes revirar tu propio canto.

| Canto | Vale si se quiere | Si no se quiere |
| --- | --- | --- |
| Nada | 1 | — |
| Truco | 2 | 1 al que grito |
| Retruco | 3 | 2 al que grito |
| Vale Cuatro | 4 | 3 al que grito |

Ejemplo grito truco y el otro equipo quiere, entonces la mano vale 2. Despues ese mismo equipo grita retruco y yo no quiero, entonces se llevan 2 puntos, que es lo que valia el truco que ya estaba querido.

---

## Irse al mazo

Irse al mazo es rendirse en la mano. El que se va entrega los puntos que estaban en juego en ese momento, o sea 1 si no se grito nada, o lo que valga el ultimo canto de truco querido.

No te podes ir al mazo dejando cosas sin resolver. Si hay un envido o una flor cantados y sin cerrar, primero se resuelven esos puntos y despues te vas.

---

## Partidas de a 6

En las partidas de a 6 la primera parte del partido alterna dos tipos de mano. Primero se juega una mano de tres contra tres, que se le dice redondilla, y despues tres manos de uno contra uno, que se le dice pico a pico.

En el pico a pico el Falta Envido vale 6 puntos y la Contra Flor al Resto vale 12, que son los 6 mas las dos flores.

Cuando un equipo llega a la mitad de los puntos del partido, de ahi en adelante se juegan solo redondillas.

---

## Señas

Existe un codigo de gestos para avisarle al compañero que cartas tenes sin que te escuche el rival. Las señas son parte del juego real y van a hacer falta para el modo de a 4 y de a 6, pero todavia no estan documentadas aca. Hay que armar la tabla completa de seña por carta antes de implementarlas.

---

## Cosas a confirmar antes de implementar

Estos puntos varian segun con quien juegues y conviene fijarlos antes de escribir el codigo.

- Hasta que momento exacto se puede tocar el envido. Toda la primera ronda, o hasta que el primer jugador tira su carta.
- Si la flor es obligatoria cantarla o si podes esconderla.
- Cuantos puntos entrega el que no quiere una Contra Flor al Resto.
- Si la Falta Envido se cuenta contra el final del partido o contra el final de las malas cuando el que va primero todavia esta en malas.
- Que pasa si el que es mano se va al mazo antes de tirar la primera carta.

---

## Ideas para la implementacion posterior.

Que el sistema de barajado de cartas y repartir sea literalmente como en un mazo de cartas. Si se pudiera animar las cartas siendo barajadas y que se vean seria genial. Y darle realismo, por ejemplo si un elige no barajar y da a cortar y el otro no corta, que la primer carta sea las que se juntaron. Complica la implementacion pero da realidad aumentada y satisfacion a la jugabilidad.

Tambien estaria bueno implementar trampas, como bichar las señas o ojear si se distraen.