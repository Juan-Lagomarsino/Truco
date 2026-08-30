# Plan de publicación — Android / iOS / Steam

> **Esto es SOLO un documento de investigación y planificación.** No se crea
> ninguna cuenta, no se scaffoldea ningún proyecto de build, no se instala
> nada, no se toca `/game` ni `/server`. Nada de lo que sigue se ejecuta sin
> autorización expresa mía (el autor) y sin tener Unity abierto y el juego en
> un estado jugable. Este archivo es insumo para decidir, no una lista de
> tareas en curso.

Contexto: desarrollo en solitario del Truco Uruguayo en Unity 6, para
Android, iOS y Steam. `/game` y `/server` están marcados **NO TOCAR** hasta
fases posteriores (ver `CLAUDE.md`). Este plan asume que ese trabajo de
motor/servidor ya está resuelto cuando llegue el momento de publicar; acá se
mapean los requisitos de cada plataforma y las decisiones de negocio que
hacen falta, no el trabajo técnico de Unity en sí.

---

## 1. Requisitos por plataforma

### 1.1 Android — Google Play Console

- **Cuenta de desarrollador**: registro único de Google Play Console, costo
  **~25 USD una sola vez** (no es suscripción). Requiere identidad verificada
  (puede pedir DNI/pasaporte y, según el país, un período de espera de
  verificación de cuenta nueva antes de poder publicar públicamente).
- **Firma del build**: Android exige firmar el paquete. Google Play usa **Play
  App Signing** (Google gestiona la clave de firma final; el desarrollador
  sube una clave de subida/upload key). Formato de entrega: **AAB (Android
  App Bundle)**, no APK — Google dejó de aceptar APK como formato de subida
  estándar hace años.
- **Target API level vigente**: Google exige apuntar a una API level mínima
  que se actualiza aproximadamente una vez por año (política "target API
  level requirements"). Esto es **una fecha móvil**: hay que revisar el
  requisito vigente en el momento de la subida, no asumir un número fijo hoy.
  Unity 6 debe soportar esa API level al momento del build; si el proyecto
  usa una versión vieja de Unity esto puede bloquear la publicación.
- **Políticas de contenido — punto delicado para un juego de cartas**:
  - El Truco Uruguayo en este proyecto usa **tantos** (puntos de partida),
    no dinero real, no hay apuestas reales, no hay retiro de valor. Esto
    **no encuadra como real-money gambling** y no debería requerir las
    licencias de juego de azar real.
  - Sin embargo, Google tiene una política específica de "Juegos de azar,
    apuestas y casino" (Gambling) que aplica según *cómo se presenta* el
    juego, no solo según si hay dinero real. Cartas + apuestas simbólicas +
    mecánicas de "envite" pueden disparar revisión manual aunque no haya
    dinero de por medio. Hay que declarar explícitamente en el formulario de
    contenido de Play Console que **no** es un juego de azar con dinero real,
    y revisar la clasificación de contenido (IARC) con cuidado en las
    preguntas sobre "juegos de azar simulados".
  - Esto es un **riesgo de política que cambia con frecuencia** — revisar la
    política vigente de Google en el momento de la submission, no confiar en
    lo que dice este documento a futuro.

### 1.2 iOS — Apple Developer Program

- **Cuenta**: Apple Developer Program, **99 USD/año** (recurrente, no único).
  Sin cuenta activa el juego se cae de la App Store si no se renueva.
- **Requiere macOS**: Xcode solo corre en macOS. El build final para
  submission a App Store (archivo `.ipa`, firma, notarización) necesita pasar
  por Xcode en una Mac en algún punto del pipeline, aunque el desarrollo en
  Unity se haga en otro sistema operativo. Esto es un **requisito de
  infraestructura**, no solo de cuenta: si no hay una Mac disponible (propia,
  prestada, alquilada — Mac en la nube tipo MacinCloud/GitHub Actions macOS
  runners, etc.) no se puede publicar en iOS. **Anotado como decisión/recurso
  pendiente más abajo.**
- **App Store Review Guidelines relevantes**:
  - Sección de "Gambling, Contests, and Sweepstakes": igual que en Google,
    aplica más por la presentación (cartas, envites, lenguaje de apuesta) que
    por si hay dinero real. Apple es históricamente más estricto y con
    revisión humana; conviene ser explícito en las notas de reviewer
    aclarando que los tantos no son moneda de curso ni canjeable.
  - Guideline de metadata: capturas de pantalla deben reflejar el
    funcionamiento real de la app (no assets de marketing engañosos).
  - Tiempo de revisión humana (ver riesgos, sección 6).

### 1.3 Steam — Steamworks

- **Cuenta Steamworks**: **100 USD por juego** (no por cuenta), reembolsable
  una vez que el juego alcanza cierto umbral de ventas (~1000 USD según
  términos vigentes de Valve — confirmar cifra exacta al momento de aplicar).
- **Steamworks SDK**: integración opcional pero recomendada (achievements,
  cloud saves, overlay). Para un juego de cartas simple, el SDK es
  **opcional en la v1** — Steam no exige integrarlo para publicar, aunque sin
  overlay/achievements el juego pierde visibilidad en algunas búsquedas y
  funciones sociales.
- **Depots y build**: Steam organiza el contenido en "depots" (paquetes de
  archivos por plataforma/arquitectura). Para un juego simple de Unity
  alcanza con un depot por SO soportado (Windows/Mac/Linux, según a qué se
  apunte).
- **Curación/review de Valve**: Steam no tiene curación de contenido tan
  estricta como Apple, pero sí un proceso de revisión antes de aprobar la
  página de la tienda y el build (más orientado a que no sea fraude/spam/
  malware que a contenido). Tiempos de revisión históricamente más cortos y
  predecibles que Apple.

---

## 2. Qué es específico de este juego

- **Nombre/marca**: **no decidido todavía** — ver sección de decisiones. Sin
  nombre no se puede reservar el listing en ninguna de las tres tiendas
  (Google Play, App Store y Steam piden un nombre de producto desde el
  registro inicial del listing).
- **Arte necesario** (mínimo para v1 jugable):
  - Mazo español de 40 cartas (cuatro palos: oro, copa, espada, basto),
    ilustradas o con un estilo gráfico propio.
  - Tapete/mesa de juego.
  - Indicadores visuales de **muestra** y **piezas** (conceptos propios del
    Truco Uruguayo, no del argentino — el arte tiene que distinguirlos
    claramente de una carta común).
  - Representación visual de **señas** (gestos) si se implementan en `/core`
    antes de la v1 visual — hoy documentadas en `docs/notas/SEÑAS.md` pero
    pendientes de implementación en el dominio (ver memoria del proyecto).
  - Iconografía de UI: botones de canto (Envido, Truco, Flor, etc.),
    marcador de tantos.
- **Localización**:
  - Español rioplatense/uruguayo como idioma principal y público objetivo
    primario (el vocabulario del dominio — Envido, Flor, Muestra, Pieza,
    etc. — es intraducible sin perder identidad; no tiene sentido
    "traducir" los nombres de los cantos).
  - Inglés como expansión posterior, con los términos del dominio mantenidos
    en español (como ya se hace en el código) y explicados en un glosario/
    tutorial in-game, no traducidos literalmente.
- **Modo de juego en v1**: depende de si `/server` (SignalR, NO TOCAR por
  ahora) está listo. Recomendación de este plan: **v1 solo local / vs bot**
  (usando `/bot`, que according al plan nocturno se está armando en paralelo
  esta misma noche), sin multijugador online. El multijugador online agrega
  una superficie enorme de trabajo (servidor autoritativo, salas,
  reconexión, matchmaking) y de riesgo de plataforma (revisión de Apple es
  más estricta con apps que tienen componente online/cuentas de usuario).
  Publicar primero "vs bot" reduce alcance y acelera la primera publicación.

---

## 3. Orden sugerido de publicación

**Recomendado: Android primero → Steam segundo → iOS tercero.**

Razones:

1. **Android tiene la barrera de entrada más baja**: costo único bajo
   (~25 USD), no requiere hardware especial (se compila desde cualquier SO
   donde corra Unity), y el ciclo de iteración con Google es rápido
   (actualizaciones casi inmediatas, revisión automatizada mayormente). Es la
   mejor plataforma para **validar** que el juego funciona end-to-end en un
   dispositivo real y recibir feedback real de jugadores rápido, con el
   menor costo de "aprender publicando".
2. **Steam segundo**: costo moderado (100 USD, reembolsable), no depende de
   hardware Apple, buen público para un juego de cartas de mesa/estrategia
   con perfil "juego de reglas ricas" (el Truco Uruguayo con muestra/piezas/
   flor es más profundo que el truco genérico, lo cual encaja bien con el
   público de Steam que valora profundidad de reglas). Permite reusar
   aprendizajes de la build de Android (mismo motor, distinto empaquetado).
3. **iOS al final**: es la plataforma con mayor barrera estructural para un
   desarrollador solo sin Mac (costo recurrente de 99 USD/año + necesidad de
   Xcode/macOS + revisión humana más lenta y más estricta con temas de
   "gambling-adjacent"). Conviene encararla una vez que el juego ya está
   validado en otras plataformas y se justifica el costo recurrente y la
   inversión en infraestructura Mac.

Este orden es una recomendación fundamentada, no una decisión tomada — la
decisión final de por dónde arrancar es del autor (ver sección final).

---

## 4. Checklist pre-submission por plataforma

### 4.1 Google Play
- [ ] Nombre de la app y descripción corta/larga (localizadas es-UY como
      mínimo).
- [ ] Ícono de la app (512x512 según spec vigente de Play Console).
- [ ] Gráfico de feature (1024x500).
- [ ] Al menos 2 capturas de pantalla por tipo de dispositivo soportado
      (teléfono; tablet si se declara soporte).
- [ ] Clasificación de contenido vía cuestionario IARC (prestar atención
      especial a las preguntas sobre juegos de azar simulados/apuestas).
- [ ] Política de privacidad (URL pública) — obligatoria aunque no se
      recolecten datos personales.
- [ ] Declaración de datos recolectados (Data Safety section).
- [ ] AAB firmado con Play App Signing configurado.
- [ ] Target API level vigente verificado al momento de la subida.

### 4.2 App Store (iOS)
- [ ] Nombre, subtítulo, descripción, palabras clave (localizados).
- [ ] Ícono de app (1024x1024, sin canal alfa).
- [ ] Capturas de pantalla por tamaño de dispositivo requerido (iPhone y,
      si se soporta, iPad) — Apple exige tamaños específicos por generación
      de dispositivo.
- [ ] Rating de contenido (Age Rating) vía cuestionario de App Store
      Connect — mismo cuidado que en Android con las preguntas de
      "simulated gambling".
- [ ] Política de privacidad (URL pública, obligatoria).
- [ ] App Privacy details (qué datos se recolectan, aunque sean ninguno).
- [ ] Build firmado y notarizado desde Xcode en macOS.
- [ ] Notas para el reviewer aclarando la mecánica de tantos (no es dinero
      real) si el reviewer humano lo cuestiona.

### 4.3 Steam
- [ ] Nombre del juego y página de tienda (capturas, video opcional,
      descripción corta/larga).
- [ ] Ícono/capsule art en los tamaños que pide Steamworks (varias
      resoluciones: header, capsule pequeña, capsule vertical, etc.).
- [ ] Al menos 5 capturas de pantalla recomendadas por Valve.
- [ ] Clasificación de contenido (Steam tiene su propio formulario de
      content survey, distinto de IARC/rating de Apple/Google).
- [ ] Depot configurado por plataforma de build a distribuir.
- [ ] Decisión sobre achievements/cloud saves (opcional en v1).
- [ ] Precio y región de disponibilidad.

---

## 5. Riesgos

- **Tiempo de revisión de Apple**: históricamente entre 24 horas y varios
  días, pero puede extenderse si el reviewer humano tiene dudas sobre la
  mecánica de apuestas/tantos — un rechazo agrega otro ciclo completo de
  revisión. Es el mayor riesgo de cronograma de las tres plataformas.
- **Requisito de macOS para iOS**: sin una Mac (propia o alquilada), iOS
  queda directamente bloqueado. Esto es una dependencia dura, no solo de
  costo sino de disponibilidad de hardware — hay que resolverla antes de
  poder siquiera generar el build final, independientemente de si el
  desarrollo de Unity se hace en otro SO.
- **Cambios frecuentes de política sobre juegos de cartas/azar**: tanto
  Google como Apple actualizan sus políticas de "simulated gambling" con
  cierta frecuencia y las aplican con criterio subjetivo caso por caso. Un
  juego de cartas con cantos de apuesta simbólica (Envido, Truco, Flor) es
  exactamente el tipo de app que puede quedar atrapada en una re-
  interpretación de política aunque no cambie ni una línea del propio juego.
  Mitigación: revisar la política vigente de cada tienda **inmediatamente
  antes** de cada submission, no confiar en lo escrito acá como verdad
  permanente.
- **Target API level de Android como blanco móvil**: si el proyecto tarda en
  llegar a publicación, el requisito vigente al momento de escribir este
  documento puede haber quedado obsoleto. Repetir la verificación al momento
  de compilar el build final.
- **Costo recurrente de iOS (99 USD/año)**: a diferencia de Android (pago
  único) y Steam (pago único reembolsable), Apple cobra todos los años
  mientras la app siga publicada. Esto afecta la decisión de cuándo entrar a
  esa plataforma si el juego no genera ingresos que lo cubran.
- **Riesgo de alcance por multijugador online**: si se decide que la v1 debe
  incluir multijugador (dependiente de `/server`, hoy NO TOCAR), el
  cronograma de publicación se extiende considerablemente y agrega
  superficie de revisión adicional en las tres tiendas (manejo de cuentas,
  posible chat entre jugadores, moderación).

---

## Decisiones que necesitan mi OK

Estas son decisiones de negocio/producto que este plan **no inventa** y que
quedan explícitamente pendientes de mi autorización:

1. **Orden real de publicación**: ¿confirmo Android → Steam → iOS como
   sugiere este plan, o prefiero otro orden (por ejemplo Steam primero si el
   público objetivo es más PC-céntrico)?
2. **Presupuesto para cuentas de desarrollador**: ¿autorizo el gasto de
   ~25 USD (Google Play, único) + 100 USD (Steam, reembolsable) + 99 USD/año
   (Apple, recurrente)? ¿En qué momento se hace cada gasto — todos juntos o
   escalonados según el orden de publicación?
3. **Recurso para build de iOS**: ¿tengo o consigo acceso a una Mac (propia,
   prestada, o servicio en la nube tipo MacinCloud/runner de CI) para poder
   generar el build de Xcode? Sin esto, iOS queda bloqueado
   independientemente del resto del plan.
4. **Monetización**: ¿el juego es gratis, de pago único, con anuncios, o con
   compras dentro de la app (IAP)? Esto es puramente decisión de negocio mía
   y afecta directamente qué formularios y políticas de cada tienda aplican
   (por ejemplo, apps con anuncios o IAP tienen requisitos adicionales de
   privacidad y, en el caso de mecánicas tipo "loot box", escrutinio
   adicional que un juego de cartas con apuestas simbólicas de tantos
   podría rozar si no se aclara bien).
5. **Nombre/marca del juego**: no hay un nombre decidido todavía. Sin nombre
   no se puede reservar el listing en ninguna tienda. ¿Ya tengo un nombre en
   mente o hace falta definirlo antes de avanzar con cualquier registro?
6. **Alcance de la v1 respecto a multijugador**: ¿confirmo que la v1 sale
   solo local/vs bot (recomendación de este plan) y el multijugador online
   queda para una v2 una vez que `/server` esté listo, o el multijugador es
   un requisito desde el día uno?
