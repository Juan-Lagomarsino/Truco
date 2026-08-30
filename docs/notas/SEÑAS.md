---
estado: CERRADA y VOLCADA a RULES_Afinadas.md §Señas. Nada va a /core todavía.
tags: [señas, reglas, a-confirmar]
fuente: RULES_Afinadas.md §Señas + reglamento uruguayo (ver §Fuentes) + correcciones del autor
---

# Señas — tabla completa (propuesta)

> [!warning] Estado
> Borrador de documentación, no regla cerrada. `RULES_Afinadas.md §Señas` pide "armar la tabla
> completa de seña por carta **antes** de implementarlas". **Nada toca `/core` hasta que esto
> esté confirmado acá y volcado a `RULES_Afinadas.md`.**

> [!danger] Ojo: esto es truco URUGUAYO
> En la primera pasada me confundí con las señas argentinas (1 de espada = morder el labio, etc.).
> **Están mal para este juego.** El uruguayo señea distinto y, sobre todo, **le da seña a las
> piezas**. Esta versión ya está corregida con fuente uruguaya y con tus indicaciones.

## Para qué sirven y dónde se usan

Gestos para avisarle al compañero qué cartas tenés sin que el rival te escuche. Sólo existen con
compañero: **modo de a 4 (2v2) y de a 6 (3v3)**; en 1v1 no hay. Son **información**, no una
acción del reductor: no cambian el estado legal del juego (ver §"Nota para la implementación").

---

## Convenciones de notación (provenance)

| Marca | Significado |
| --- | --- |
| 🟢 | **Confirmado por vos** en el chat. Es regla. |
| 🔵 | Viene de un **reglamento uruguayo** (ver §Fuentes). Coherente con lo que confirmaste, pero **falta tu OK final**. |
| 🟡 | **Convención mía / hueco.** Sin respaldo. A confirmar o reemplazar. |

---

## Vocabulario de gestos (propuesto)

Base: reglamento uruguayo, ajustado a tus correcciones. Cada gesto = una carta o rol.

### Piezas (las 5 cartas del palo de la muestra)

| Rol (carta del palo de la muestra) | Fuerza / valor | Gesto | Marca |
| --- | --- | --- | --- |
| **2** de la muestra (pieza mayor) | nivel 1 · 30 | Levantar las cejas | 🟢 |
| **4** de la muestra | nivel 2 · 29 | Tirar un beso | 🟢 |
| **5** de la muestra | nivel 3 · 28 | Arrugar la nariz | 🟢 |
| **11 / Caballo** de la muestra | nivel 4 · 27 | **Guiño ojo derecho** | 🟢 |
| **10 / Sota** de la muestra | nivel 5 · 27 | **Guiño ojo izquierdo** | 🟢 |

> Vos confirmaste que los guiños son para el 11 y el 10 de la muestra. El reglamento asigna
> derecho→11, izquierdo→10; lo dejo así salvo que quieras invertir los ojos (D2).

### Matas (identidad fija, no dependen de la muestra)

| Carta | Fuerza | Gesto | Marca |
| --- | --- | --- | --- |
| **1 de Espada** | nivel 6 | Mueca hacia la **derecha** | 🟢 |
| **1 de Basto** | nivel 7 | Mueca hacia la **derecha** | 🟢 |
| **7 de Espada** | nivel 8 | Mueca hacia la **izquierda** | 🟢 |
| **7 de Oro** | nivel 9 | Mueca hacia la **izquierda** | 🟢 |

> [!check] Las matas comparten seña de a pares (D3 — confirmado)
> El 1 de Espada y el 1 de Basto usan **la misma** seña (mueca derecha), e igual el 7 de Espada
> y el 7 de Oro (mueca izquierda). O sea: avisás "tengo una de las dos matas altas" vs "una de las
> dos bajas", sin decir cuál. **Se juega así.**
>
> Variante conocida (anotada, **no** la usamos): algunos agregan sacar la lengua hacia el lado de
> la mueca, y arriba/abajo para distinguir la alta de la baja. Queda documentado por si más
> adelante se quiere sumar, pero la regla del juego es **sólo mueca derecha/izquierda**.

### Chicas, falsos y débiles

| Carta(s) | Rol | Gesto | Marca |
| --- | --- | --- | --- |
| **3** (los cuatro palos) | chica | Morder el labio inferior | 🟢 |
| **2** que **no** es pieza (palo ≠ muestra) | chica | Boca levemente abierta | 🟢 |
| **1 de Oro** y **1 de Copa** (falsos) | falso | Sacar la punta de la lengua | 🟢 |
| Blancas y negras (sin valor) | nada | *sin seña propia* → ver §"Cómo se señea la mano" | 🟢 |

### Auxiliares (no son "por carta")

| Aviso | Gesto | Marca |
| --- | --- | --- |
| **Flor** | **Inflar los cachetes** | 🟢 |
| **Envido** (fuerte) | **Sacar / mostrar los dientes de abajo** | 🟢 |
| **Nada** (las tres cartas malas) | **Cerrar ambos ojos** | 🟢 |
| Quiero / no quiero | — (normalmente se dice, no se señea) | 🟡 abierto (D5) |

> "Cerrar ambos ojos" es una seña **de mano**, no de una carta suelta: significa "mis tres cartas
> son malas". Ver §"Cómo se señea la mano".

---

## Cómo se señea la mano (D1 + D7 — confirmado)

Dos reglas que definen *qué* se señea, no sólo el gesto:

1. **Una carta, una seña (D1).** Cada carta tiene exactamente **una** seña. Si por su naturaleza
   pudiera entrar en dos categorías, se toma la del **valor más alto**, igual que en toda la
   jerarquía del juego. (Ej. clásico: el 12 espejo podría verse como negra o como la pieza que
   copia → gana la pieza, que vale más.)

2. **Se señean las buenas, no las malas (D7).** Hacés la seña de **cada carta buena** que tengas
   —pueden ser dos—; las malas no se avisan. Ejemplo: con dos buenas y una mala, hacés **las dos**
   señas buenas y listo; **no** cerrás los ojos por la tercera.
   - Hacer una seña ya implica "tengo esta carta **y** el resto son malas". Si hacés dos, tenés
     dos buenas.
   - **"Cerrar ambos ojos" es sólo para las tres malas.** Es la seña de la mano entera cuando no
     tenés ninguna carta señable.

Qué cuenta como **buena** (tiene seña propia): piezas, matas, el 3, el 2 común y los falsos
(1 de Oro / 1 de Copa). Qué cuenta como **mala** (sin seña propia, entra en "ojos cerrados" si son
las tres): las **blancas** y las **negras**.

---

## Sobre la pregunta 1 que no se entendió (reformulada)

Antes te pregunté "¿seña por carta física o por rol de fuerza?". Lo digo con un ejemplo concreto:

Tu **4 de Oro**. Cuando la muestra **no** es de Oro, es una carta basura (una blanca). Pero
cuando la muestra **es** de Oro, ese mismo 4 de Oro se convierte en la **2ª carta más fuerte del
mazo** (pieza, vale 29). La pregunta era: ¿ese 4 de Oro hace **siempre el mismo gesto**, o hace
el gesto de "nada" cuando es basura y el gesto de pieza ("beso") cuando la muestra es de Oro?

**La seña es por ROL, no por la cara de la carta (confirmado).** Una misma carta física puede
tener **dos señas** según la muestra, y toma siempre la del **valor más alto** (D1). Eso es lo
natural: al compañero le importa cuán fuerte es lo que tenés, no qué dibujo. Qué gesto corresponde
lo calcula **el juego** con la tabla de fuerza que ya existe en las reglas; el jugador sólo ve
"tengo pieza" o "tengo nada". Ver §"Cómo se señea la mano" arriba.

---

## Tabla completa por carta (las 40)

Palos: **B**asto, **O**ro, **E**spada, **C**opa. `y` = palo de la muestra. La **Seña** es la que
hacés **si esa es tu mejor carta**.

### Cartas de identidad fija (la seña NO depende de la muestra) — 16 cartas

| Carta | Nivel | Rol | Seña |
| --- | --- | --- | --- |
| 1 de Espada | 6 | mata | Mueca derecha 🟢 |
| 1 de Basto | 7 | mata | Mueca derecha 🟢 |
| 7 de Espada | 8 | mata | Mueca izquierda 🟢 |
| 7 de Oro | 9 | mata | Mueca izquierda 🟢 |
| 3 de Basto | 10 | chica | Morder labio inferior 🟢 |
| 3 de Oro | 10 | chica | Morder labio inferior 🟢 |
| 3 de Espada | 10 | chica | Morder labio inferior 🟢 |
| 3 de Copa | 10 | chica | Morder labio inferior 🟢 |
| 1 de Oro | 12 | falso | Punta de la lengua 🟢 |
| 1 de Copa | 12 | falso | Punta de la lengua 🟢 |
| 7 de Basto | 16 | blanca | *mala, sin seña propia* 🟢 |
| 7 de Copa | 16 | blanca | *mala, sin seña propia* 🟢 |
| 6 de Basto | 17 | blanca | *mala, sin seña propia* 🟢 |
| 6 de Oro | 17 | blanca | *mala, sin seña propia* 🟢 |
| 6 de Espada | 17 | blanca | *mala, sin seña propia* 🟢 |
| 6 de Copa | 17 | blanca | *mala, sin seña propia* 🟢 |

### Cartas muestra-dependientes (la seña cambia según `y`) — 24 cartas (6 números × 4 palos)

| Número | Si su palo **es** la muestra (`palo = y`) | Si su palo **NO** es la muestra |
| --- | --- | --- |
| **2** | pieza mayor → **Levantar las cejas** 🟢 | chica "un 2" → **Boca levemente abierta** 🟢 |
| **4** | pieza (29) → **Beso** 🟢 | blanca → *mala, sin seña propia* 🟢 |
| **5** | pieza (28) → **Arrugar la nariz** 🟢 | blanca → *mala, sin seña propia* 🟢 |
| **11** | pieza (27) → **Guiño derecho** 🟢 | negra → *mala, sin seña propia* 🟢 |
| **10** | pieza (27) → **Guiño izquierdo** 🟢 | negra → *mala, sin seña propia* 🟢 |
| **12** | **espejo** (si la muestra es pieza) → la seña de la pieza que copia 🟢; si la muestra no es pieza → negra → *mala, sin seña propia* 🟢 | negra → *mala, sin seña propia* 🟢 |

16 fijas + 24 muestra-dependientes = **40 cartas**. ✔

> [!check] El 12 espejo (D6 — confirmado)
> Si la muestra es pieza (2,4,5,11,10) y tenés el 12 de ese palo, tu 12 "copia" esa pieza
> (ver `PREGUNTAS_ABIERTAS` C1) y **hace la seña de la pieza copiada** (ej.: muestra 2 de Oro +
> tu 12 de Oro → seña de "2 de muestra" = levantar cejas). Si la muestra no es pieza, el 12 es una
> negra común → ambos ojos cerrados.

---

## Preguntas para cerrar (checklist)

- [x] **D2** — Guiños: **derecho = 11 / izquierdo = 10**. ✅
- [x] **D3** — Matas de a pares (1E/1B mueca derecha; 7E/7O mueca izquierda). Sólo mueca, sin variante de lengua. ✅
- [x] **D5** — Envido = **sacar los dientes de abajo**. Flor = **inflar los cachetes**. ✅
- [x] **D6** — 12 espejo hace la seña de la pieza que copia (cuando la muestra es pieza). ✅
- [x] **D1** — Una carta = una seña; si entra en dos, la del **valor más alto**. ✅
- [x] **D4** — Confirmados los gestos de piezas (2 = cejas / 4 = beso / 5 = arrugar nariz) y de chicas (3 = morder labio, 2 común = boca abierta, falsos 1O/1C = punta de lengua). ✅
- [x] **D7** — Se señean **todas las buenas** (pueden ser dos); "ojos cerrados" **sólo** con las tres malas. ✅
- [x] **D8** — Trampas ("bichar las señas"): anotado para después. ✅

> [!success] Tabla cerrada y volcada
> Todas las decisiones (D1–D8) están confirmadas y la tabla ya está en `RULES_Afinadas.md §Señas`
> (fuente de verdad). Esta nota queda como memoria de trabajo (decisiones, provenance, fuentes).

> [!info] Implementado parcialmente en /core (corrida nocturna)
> `Domain.Señas.DeCarta(Carta, Muestra)` en `core/Señas.cs` ya mapea una carta suelta a su
> seña (o `null` si es mala), con tests en `tests/SeñasTests.cs`. Lo que falta es la función
> de "seña de la mano completa" (incluye "cerrar ambos ojos"): encontré un caso real que
> ninguno de los dos documentos resuelve — qué pasa si las tres cartas de la mano son
> "buenas" a la vez, dado que el texto dice "hasta dos". Ver `PREGUNTAS_PENDIENTES.md` P1.

---

## Fuentes

- Correcciones tuyas en el chat (11/10 de la muestra = guiños; flor = inflar cachetes). — 🟢
- Reglamento uruguayo: [servicioti.com.uy — Truco Uruguayo Reglamento](https://www.servicioti.com.uy/2026/04/truco-uruguayo-reglamento.html) (tabla de señas por pieza/mata/chica/flor/envido). — 🔵
- Contexto de señas uruguayas: [truqui.app — Las señas del truco](https://www.truqui.app/senas-del-truco) (ojo: usa asignación al estilo argentino para las matas; **no** la seguí).
- `RULES_Afinadas.md §Señas`, §Jerarquía, §Muestra; `PREGUNTAS_ABIERTAS.md` C1 (12 espejo).

---

## Nota para la futura implementación (NO implementar aún)

- Las señas **no son `Accion`es del reductor**: son un canal de comunicación, no cambian el estado
  legal ni la resolución. Meterlas en `AccionesLegales`/`Aplicar` mezclaría info con reglas.
- El mapeo carta→seña **sí** es lógica pura y determinista: podría vivir en `/core` como
  `Seña DeCarta(Carta c, Muestra m)`, reusando la tabla de fuerza existente. Se planifica
  **después** de cerrar esta tabla. Ver `[[HANDOFF_PICO_A_PICO]]` como formato de handoff.
