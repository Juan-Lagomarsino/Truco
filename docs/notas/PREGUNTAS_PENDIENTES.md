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

## P2–P16. Decisiones de negocio/arquitectura de los planes de Unity, servidor y publicación

**Estado:** ABIERTAS

No son huecos de reglas del truco, son decisiones tuyas de producto/arquitectura que los
planes escritos esta noche dejan explícitamente pendientes (cada documento tiene su propia
sección "Decisiones que necesitan mi OK" con el detalle completo; acá solo el resumen para
que las tengas todas juntas).

**De `docs/notas/PLAN_Unity.md`:**
- Versión exacta de Unity 6 (build number) y si hace falta retargetear `core.csproj` para
  que Unity pueda consumirlo tal cual.
- Método de link de `/core` a Unity: DLL precompilada vs. link de fuente por asmdef.
- URP vs Built-in Render Pipeline.
- UI Toolkit vs uGUI para el HUD de cantos.
- Arte placeholder vs arte final desde el arranque, y de qué fuente (licencias).
- Si las señas deberían viajar como mensaje de red en `/server` a futuro (para
  reconexión), aunque sigan sin ser una `Accion` del reductor.

**De `docs/notas/PLAN_Server.md`:**
- Dónde vive el filtrado de vista por jugador: DTO en `/server` vs. función pura nueva en
  `Domain` (cambiaría la superficie pública de `/core`).
- Autenticación de jugadores al unirse/reconectar a una sala.
- Persistencia: todo en memoria vs. guardar `EstadoPartida` para sobrevivir un restart.
- Modelo de hosting: un solo proceso vs. varias instancias detrás de balanceador.
- Límites de tiempo para reconexión y qué pasa con los puntos en juego si nadie reconecta.
- Si señas entra en el alcance de la primera versión del servidor, dado que en `/core`
  todavía no existe la función de "seña de la mano completa" (ver P1).

**De `docs/notas/PLAN_Publicacion.md`:**
- Orden real de publicación (el plan sugiere Android → Steam → iOS).
- Presupuesto y timing para las cuentas de desarrollador (~25 USD Google, 100 USD Steam,
  99 USD/año Apple).
- Recurso para build de iOS: acceso a una Mac (propia, prestada, o en la nube) — sin esto
  iOS queda bloqueado.
- Modelo de monetización (gratis / pago único / ads / IAP).
- Nombre/marca del juego (no hay ninguno decidido todavía).
- Si la v1 sale solo local/vs bot o si el multijugador online es requisito desde el día 1.

**De `docs/notas/DISENO_Grabacion.md`**: resuelta — ver DECISIONES_NOCTURNAS.md D4
(el codec de texto de una `Grabacion` quedó en `/core`, por ser transformación de datos
pura sin IO, igual criterio que "EstadoPartida es serializable").

---
