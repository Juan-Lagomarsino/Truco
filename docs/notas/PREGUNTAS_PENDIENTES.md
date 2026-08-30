---
estado: en progreso — corrida nocturna
tags: [nocturno, preguntas]
---

# Preguntas pendientes de la corrida nocturna

Casos que encontré trabajando solo y que no puedo resolver sin que decidas. No invento
ninguna regla del truco para cerrarlos: quedan anotados acá y seguí con el resto de la
lista. Formato: contexto, qué necesito que decidas, opciones, mi recomendación.

---

## P1. Señas: ¿qué pasa si las tres cartas de la mano son "buenas" a la vez?

**Estado:** ABIERTA

**Contexto:** `docs/notas/SEÑAS.md` está cerrado (D1–D8 confirmados) y volcado a
`RULES_Afinadas.md §Señas`, que dice: *"Se señean todas las cartas buenas que tengas, que
pueden ser hasta dos"*. Implementé `Domain.Señas.DeCarta(Carta, Muestra)` en `/core`
(mapeo de una carta suelta a su seña), que es 100% inambiguo y ya tiene test.

Lo que NO implementé es la función a nivel de mano completa (las 3 cartas de un jugador)
que decide qué señas hacer, incluida "cerrar ambos ojos" cuando las tres son malas. Los
casos de 0, 1 o 2 cartas buenas son directos (el texto ya los cubre con ejemplos). El
problema es el caso de **3 cartas buenas al mismo tiempo**: es posible (ejemplo real: 3 de
Oro + 3 de Copa + 1 de Oro, con muestra en cualquier otro palo — las tres son "buenas" por
la tabla: dos treses y un uno falso), y ni `RULES_Afinadas.md` ni `SEÑAS.md` dicen qué
pasa ahí. La frase "pueden ser hasta dos" no aclara si es una descripción de lo que suele
pasar o una regla dura ("mostrás como máximo dos, aunque tengas tres buenas").

**Qué necesito que decidas:**
1. ¿Con 3 buenas se señean las 3, o hay un tope de 2?
2. Si hay tope de 2: ¿cuáles dos se muestran? (¿las dos de mayor jerarquía? ¿un orden fijo
   por categoría —pieza > mata > chica/falso—?)

**Mi recomendación:** tope de 2, mostrando las dos de mayor jerarquía (usando
`Jerarquia.Fuerza`, que ya existe y ya resuelve empates de forma consistente con el resto
del juego). Es la lectura más literal de "pueden ser hasta dos" y reusa una función que ya
está testeada, pero es una recomendación, no algo que haya implementado.

**Mientras tanto:** no implementé ninguna función de "seña de la mano completa" en /core.
Sólo existe el mapeo carta→seña (`Señas.DeCarta`), que no depende de esta decisión.

---
