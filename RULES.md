# Reglas del Truco

## Explicacion de la Logica del Juego

El juego consiste en dos equipos. Si se juega de a 2 es 1vs1, si se juega de a 4 es 2vs2, y si se juega de a 6 es 3vs3. 
La gracia del juego es sumar puntos. El juego se puede jugar a los puntos que quieras, pero siempre se va a partir en dos etapas. Buenas y malas. Si jugas a 40 es 20 y 20, y asi con todo. 
Como ganas puntos con las mecanicas:

- **Mecanica 1: Tocar** : Tocar refiere a la accion de el envido. Existen 3 tipos de Toques:
     1) Envido (Normal, se dice solo Envido), este envido vale 2 puntos si se quiere jugar, es decir Jugador 1 dice envido, si Jugador 2 dice que si quiere el que gane el envido gana 2 puntos, si dice que no vale 1 punto para el que toco (En este caso Jugador 1). 
     2) Real Envido (Al agregarle el real antes estoy dandole mas valor), ahora en ves de valer 2 vale 3.
     3) Falta Envido (Al agregarle el falta antes, estoy jugando hasta la falta), ahora en ves de valer 2 o 3 vale la falta. Que es la falta, la falta es lo que le falta a el otro equipo para salir de su parcial. Si esta en malas (La primer mitad del juego ) lo que le falta al otro equipo para salir de malas, si esta en buenas (La segunda mitad del juego) lo que le falta para ganar. 
     
     Ademas importante saber que se puede revirar los toques, es decir si yo juego un envido, el rival puede responder, quiero o no quiero, pero tambien puede responder envido. Y subir asi el valor del mismo. Va escalando con el valor del toque. Si Jugador 1 dice envido y Jugador 2 dice envido, indirectamente dijo quiero y envido de vuelta (No es literalmente como si hubiera dicho eso, pero en el sentido de los puntos si) y ahi depende de que quiera el Jugador 1, si dice que quiere se juega el envido por 4 puntos (2 + 2), y si no quiere el Jugador 2 se queda con 3 puntos (2 + 1)

     **Como se juega**

     
- **Mecanica 2: Gritar** :  Gritar refiere a la accion de el Truco. Existen 3 tipos de Gritos:
     1) Truco: 
     2) Real Envido (Al agregarle el real antes estoy dandole mas valor), ahora en ves de valer 2 vale 3.
     3) Falta Envido (Al agregarle el falta antes, estoy jugando hasta la falta), ahora en ves de valer 2 o 3 vale la falta. Que es la falta, la falta es lo que le falta a el otro equipo para salir de su parcial. Si esta en malas (La primer mitad del juego ) lo que le falta al otro equipo para salir de malas, si esta en buenas (La segunda mitad del juego) lo que le falta para ganar. 
- **Mecanica 3: Cantar**

Como se juega cada ronda. Arranca tirando el jugador que es mano (El que esta seguido a el jugador que repartio). 

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

- 12 de Cualquier palo menos la muestra (El 12 de la muestra, es una carta especial. Si la muestra es una pieza, es decir 2,4,5,11,10 el 12 actua de espejo, copiando asi la carta que es la muestra. Ejemplo si la muestra es un 2 de Oro y tengo un 12 de Oro, en realidad tengo un 2 de Oro. Si la muestra es un 1 de Oro es decir una muestra que no es Pieza, entonces mi 12 de Oro es un 12 normal.)
- 11 de Cualquier palo menos la muestra
- 10 de Cualquier palo menos la muestra

Blancas (Se le llama blancas)

- 7 de Cualquier palo menos Espada y Oro
- 6 de Cualquier palo
- 5 de Cualquier palo menos la muestra
- 4 de Cualquier palo menos la muestra

---

## Jerarquia en formato programacion

**El orden de las cartas va de mejor a peor.**

### Carta = (Numero, Palo)

> Numero: Refiere al numero que tiene esa carta. Este numero pertenece a N = [1,2,3,4,5,6,7,10,11,12]

> Palo: Refiere al palo que tiene esa carta. Este palo pertenece a P = [Basto, Oro, Espada, Copa]

Sea la muestra (x,y) para la especificacion.

- (2, y) 
- (4, y)
- (5, y)
- (11, y)
- (10, y)
- (1, Espada)
- (1, Basto)
- (7, Espada)
- (7, Oro)
- (3, ∀ p ∈ P)
- (2, ∀ p ∈ P / p != y)
- (1, ∀ p ∈ P / p != Espada && p != Basto)
- (12, ∀ p ∈ P / !(x ∈ [2,4,5,11,10] && p = y))
- (11, ∀ p ∈ P / p != y)
- (10, ∀ p ∈ P / p != y)
- (7, ∀ p ∈ P / p != Espada && p != Oro)
- (6, ∀ p ∈ P)
- (5, ∀ p ∈ P / p != y)
- (4, ∀ p ∈ P / p != y)

---

## El toque de envido. Que es, como contarlo y como jugarlo.

---

## El canto de flor. Que es, como contarla y como jugarla.

---

## El grite de Truco/Retruco/ValeCuatro. Que es, como cuenta y como jugarlo.

---

## Ideas para la implementacion posterior.

Que el sistema de barajado de cartas y repartir sea literalmente como en un mazo de cartas. Si se pudiera animar las cartas siendo barajadas y que se vean seria genial. Y darle realismo, por ejemplo si un elige no barajar y da a cortar y el otro no corta, que la primer carta sea las que se juntaron. Complica la implementacion pero da realidad aumentada y satisfacion a la jugabilidad. 
Tambien estaria bueno implementar trampas, como bichar las senas o ojear si se distraen. 