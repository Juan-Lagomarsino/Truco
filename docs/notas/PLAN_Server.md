# Plan — `/server`

Estado: plan solamente. `/server` sigue "NO TOCAR" según `CLAUDE.md`; este
documento no crea código, solo lo prepara para cuando se autorice.

Fuentes leídas: `CLAUDE.md`, `docs/RULES_Afinadas.md` completo,
`docs/PREGUNTAS_ABIERTAS.md`, y el `/core` real (`EstadoPartida.cs`,
`Accion.cs`, `Partido.cs`, `JugadorId.cs`, `EquipoId.cs`, `Contador.cs`,
`Carta.cs`, `Muestra.cs`, `Jugada.cs`, `Cobro.cs`). El estado actual de
`/core` ya tiene cantos, envido, flor, irse al mazo y el modo de a 6
(redondilla + pico a pico); las señas todavía no están implementadas ahí
(ver `senas-pendiente.md`).

---

## 1. Enfoque general

Servidor autoritativo, sin excepciones. El único lugar donde existe la
verdad de una partida es el `EstadoPartida` que vive en memoria del
servidor, dentro de la `Sala` correspondiente.

Flujo:

1. Un cliente (Unity, o el futuro `/cli` si se conecta en red) manda una
   `Accion` de `Domain` por SignalR.
2. El servidor identifica de qué `JugadorId` viene esa conexión (ver §3),
   llama a `Partido.AccionesLegales(estado, jugador)` y chequea que la
   acción recibida esté en esa lista.
3. Si es legal, llama a `Partido.Aplicar(estado, accion)`, que devuelve el
   `EstadoPartida` siguiente (o lanza si en el medio hay una inconsistencia
   que el chequeo anterior no cubrió — se trata como bug, no como input
   inválido de un cliente honesto).
4. El servidor guarda el nuevo estado como el estado vigente de la sala y
   difunde una vista filtrada (§4) a cada conexión de la sala.

El cliente nunca decide el estado; solo lo dibuja y manda intenciones. Esto
es literalmente la forma de la API que ya pide `core-dominio`: Unity dibuja
`EstadoPartida` y manda `Accion`, y acá el servidor hace exactamente lo
mismo que haría Unity del lado de la validación, salvo que su decisión es
la que cuenta.

Un detalle real del `Accion` actual que hay que tener presente: cada
`Accion` (`TirarCarta`, `CantarTruco`, `Quiero`, etc.) lleva un `JugadorId`
como dato puesto por quien la construye — es decir, por el cliente. El
servidor **no puede confiar en ese campo**: tiene que verificar que la
conexión que mandó el mensaje está autorizada a actuar como ese `JugadorId`
en esa sala (§3 y §6), antes incluso de mirar `AccionesLegales`. Si no, un
cliente modificado podría mandar `TirarCarta(JugadorId(0), carta)` estando
conectado como el jugador 1.

## 2. Cómo se linkea `/core`

Todo en el mismo repo .NET, sin DLL ni paquete intermedio:

- `/server` es un proyecto ASP.NET Core (Web API mínima + SignalR) que
  agrega una referencia de proyecto a `core/Core.csproj`
  (`dotnet add server/Server.csproj reference core/Core.csproj`, cuando se
  autorice). Esto es una referencia entre proyectos del propio repo, no una
  dependencia externa — coherente con lo que ya se permitió para `/cli` y
  `/bot` en el plan nocturno.
- El namespace de dominio es `Domain` (ver `RootNamespace` en
  `core/Core.csproj`); `/server` lo consume tal cual, sin wrappers.
- Serialización: `System.Text.Json` sobre los `record` / `readonly record
  struct` de `Domain`. La mayoría sale gratis (records con propiedades
  `init` inmutables), pero hay dos asteriscos concretos:
  - **`Contador`** no es un record: es una `class` con un `int[] _puntos`
    privado y sin setters públicos, solo `Puntos(EquipoId)`. `System.Text.Json`
    no lo puede reconstruir sin ayuda. Hace falta un
    `JsonConverter<Contador>` en `/server` (no en `/core`) que lo
    serialice como `{ Largo, PuntosEquipo0, PuntosEquipo1 }` y lo
    reconstruya sumando con `Contador.Sumar` desde `new Contador(largo)`.
    Esto no toca `/core`; es infraestructura del lado servidor.
  - **`JugadorId` / `EquipoId`** son `readonly record struct` con
    constructor validado y una sola propiedad `Valor`. `System.Text.Json`
    debería poder mapear `{"valor": N}` al constructor por nombre de
    parámetro, pero conviene un test de ida y vuelta (serializar → deserializar
    → comparar) antes de asumirlo, porque el nombre del parámetro (`valor`,
    minúscula) tiene que calzar con la política de nombres configurada.
- `EstadoPartida` entero es serializable sin trucos raros más allá de
  `Contador`: todo lo demás son `IReadOnlyList<T>`, `record`, `enum` o
  tipos primitivos.

## 3. Salas

Modelo mínimo, todo en memoria (ver §8 sobre persistencia):

- **`Sala`**: contiene un `EstadoPartida` (el vigente), la cantidad de
  jugadores/equipos configurada al crearla, y un mapeo `JugadorId ↔
  ConnectionId` de SignalR.
- El mapeo vive en `/server`, nunca en `/core`. `/core` no sabe que existen
  conexiones; solo conoce `JugadorId`. Esto es justamente lo que permite
  que mañana el mismo `Partido.Aplicar` sirva para Unity local, para un bot,
  y para la red sin duplicar una línea de reglas.
- Estructura propuesta para el mapeo dentro de una `Sala`:
  ```csharp
  // vive en /server, no en /core
  sealed class Sala
  {
      public EstadoPartida Estado { get; private set; }
      private readonly Dictionary<JugadorId, string> _conexionDeJugador = new();
      private readonly Dictionary<string, JugadorId> _jugadorDeConexion = new();
      // + un lock/semáforo: ver §9 (riesgos, concurrencia)
  }
  ```
- Identificación de sala: un código corto (join code) generado al crear la
  sala, no un GUID visible al usuario. Cómo se autentica *a la persona*
  detrás de un `JugadorId` (login, invitado, nombre) es una decisión
  pendiente — ver §8.
- Cuando una conexión nueva se une a una sala con un `JugadorId` libre, el
  servidor la registra en ambos diccionarios. Cuando se cae (ver §5), el
  servidor no borra el mapeo lógico `JugadorId → identidad del jugador`,
  solo invalida el `ConnectionId` viejo hasta que haya reconexión.

## 4. Visibilidad de información (vista filtrada)

`/core` es puro y no tiene ni idea de "quién está mirando". El filtrado por
jugador tiene que vivir fuera de `EstadoPartida`, como una proyección de
solo lectura. Dos formas de resolverlo, con una recomendación:

**Opción A — filtrar en `/server`.** Un mapper
`VistaEstadoPartida ProyectarPara(EstadoPartida e, JugadorId observador)`
que vive en `/server`, arma un DTO propio (no un `EstadoPartida`) y lo
serializa. `/core` no se toca para nada.

**Opción B — función pura en `/core`.** Agregar a `Domain` un tipo
`VistaJugador` y una función `Vistas.Para(EstadoPartida, JugadorId)` que
sigue siendo determinista y sin red (no rompe ninguna regla dura de
`/core`: no usa `Random`, ni IO, ni conceptos de conexión). La ventaja es
que Unity también la puede usar tal cual para dibujar "lo que ve este
jugador" sin duplicar la lógica de ocultamiento en dos lenguajes/proyectos.

**Recomiendo B**, pero es un cambio de superficie de `/core` (agrega un
tipo y una función pública nueva a `Domain`) y las reglas del proyecto piden
mi OK antes de tocar la forma de la API del dominio — queda anotado en §8,
no lo implementen sin decisión.

Independientemente de dónde viva, la regla de filtrado sale directo de
`EstadoPartida`:

| Campo de `EstadoPartida` | Visible para |
| --- | --- |
| `Manos[j]` (mano restante) | Solo para `j` mismo. Para el resto, reemplazar por la cantidad de cartas (para dibujar el dorso), nunca el valor. |
| `ManosIniciales[j]` | Solo para `j` mismo, mientras no se resolvió flor/envido de forma pública. Una vez que un canto se resuelve, lo que se hace público son los **tantos declarados**, no las cartas — cantar "28" no revela qué cartas son. |
| `JugadasBaza`, `BazasGanadas` | Públicas para todos: son cartas ya puestas en la mesa. |
| `Muestra`, `Contador`, `Turno`, `Truco`, `TrucoPendiente`, `EquipoResponde`, `EnvidoPendiente` (el canto, no las manos), `FlorPendiente`, `Cierre` | Públicas para todos: son hechos de la mesa, no cartas en mano. |
| `DenunciasPendientes` | Público que existe la ventana; no expone cartas. |

Un detalle que ya está resuelto en las reglas y hay que respetar en la
vista: `RULES_Afinadas.md` dice que esconder la flor es válido y que el
rival la puede "denunciar" si se da cuenta por indicios de juego (jugar dos
piezas, por ejemplo) — el servidor **no debe filtrar esos indicios**
(las cartas jugadas siempre son públicas), pero tampoco debe regalar el
dato de si alguien tiene flor sin cantarla. La vista filtrada ya cumple
esto solo con no exponer `ManosIniciales` de nadie más.

## 5. Reconexión

Gracias a D3 (`EstadoPartida` es serializable y determinista a partir de
semilla + número de mano + acciones aplicadas), la resincronización no
necesita replay de eventos: alcanza con volver a mandarle al que reconecta
la vista filtrada del estado *actual*.

Flujo propuesto:

1. El cliente se cae (SignalR dispara `OnDisconnectedAsync`). El servidor
   **no** togla el `JugadorId` como libre ni lo saca de la partida — solo
   marca esa entrada del mapeo como "desconectado", y opcionalmente arranca
   un timeout de gracia (ver §8: cuánto tiempo).
2. El equipo rival y el compañero deberían enterarse de que alguien se cayó
   (para que la UI lo muestre), pero la partida no tiene por qué pausarse
   por regla — eso es una decisión de producto, no de dominio, y queda
   para el diseño de Unity, no de `/core`.
3. Al reconectar (mismo join code + credencial de jugador, ver §8), el
   servidor:
   - Verifica que ese `JugadorId` sigue "desconectado" en la sala.
   - Actualiza el mapeo con el `ConnectionId` nuevo.
   - Le manda un mensaje de "estado completo" con
     `ProyectarPara(estadoVigente, jugador)` — la misma proyección que se
     manda después de cada `Aplicar`, no un mensaje especial.
4. No hace falta ningún log de acciones para reconstruir nada: el
   `EstadoPartida` vigente ya es la única verdad, y es justo lo que D3
   garantiza. Si más adelante se agrega grabación/reproducción (ítem 6 del
   plan nocturno, semilla + lista de acciones), ese log es para depurar y
   auditar partidas, no un requisito para que la reconexión funcione.

## 6. Sync autoritativa y anti-trampa

El punto no negociable: **el servidor nunca aplica una `Accion` de un
cliente sin revalidarla contra `AccionesLegales` sobre su propio estado**.
No importa qué haya mandado el cliente como "acabo de hacer esto" — la
única lista de acciones válidas es la que devuelve `Partido.AccionesLegales`
para ese `JugadorId` en el estado que el servidor tiene guardado, nunca el
que el cliente cree tener.

Dos capas de chequeo antes de `Aplicar`, en este orden:

1. **Identidad**: la conexión que mandó el mensaje, ¿está autorizada a
   actuar como el `JugadorId` que trae la `Accion`? (mapeo de §3). Si no,
   se rechaza antes de tocar el dominio — esto no es una regla del juego,
   es control de acceso.
2. **Legalidad**: ¿la `Accion` recibida está en
   `Partido.AccionesLegales(estadoVigente, jugador)`? Comparación por
   igualdad estructural (son `record`, así que `Equals` ya compara por
   valor). Si no está, se rechaza y se le informa al cliente (para que
   resincronice su copia local, que puede estar desactualizada por latencia).

Recién ahí se llama `Partido.Aplicar`. Cualquier excepción que tire
`Aplicar` en este punto es indicio de un bug del servidor (una acción que
pasó el chequeo de "legal" pero que igual explota), no una situación
esperable de cliente malicioso — vale la pena loguearla fuerte porque
significa que el chequeo de arriba y `AccionesLegales` divergieron.

**Señas** (`RULES_Afinadas.md` §Señas): son información entre compañeros
que no cambia el estado legal de la mano — no son un `Accion` de `Domain`
y no deberían transformarse en una. Además todavía no están implementadas
en `/core` (`senas-pendiente.md`), así que hoy no hay nada que retransmitir
del lado del dominio.

Cuando se implementen, la seña **no debería pasar por
`AccionesLegales`/`Aplicar` en absoluto** — es un canal aparte, más parecido
a un chat de gestos que a una jugada:

- El servidor recibe un mensaje tipo "hice esta seña" con el gesto (no con
  la carta — la seña señala una *fuerza*, no la carta física, según la
  tabla de RULES).
- Lo retransmite únicamente a las conexiones cuyo `JugadorId` esté en el
  mismo `EquipoId` que quien la hizo — `EstadoPartida.EquipoDe(jugador)` ya
  da esa respuesta sin agregar nada a `/core`.
- Nunca se guarda en `EstadoPartida` ni afecta `AccionesLegales`: si el
  servidor cayera y reconectara, una seña perdida no rompe nada porque no
  es estado del juego, es un efecto de video/audio entre vivos.
- Esto también resuelve solo la idea de "bichar señas" que menciona RULES
  al final como trabajo futuro: si algún día se quiere permitir que el
  rival "vea" una seña, es una decisión de qué conexiones reciben ese
  mensaje, no un cambio de reglas del juego.

## 7. Estructura de archivos a crear en `/server` (cuando se autorice)

```
server/
  Server.csproj                     # ASP.NET Core, ProjectReference a core/Core.csproj
  Program.cs                        # arranca Kestrel + mapea el hub de SignalR
  Hubs/
    MesaHub.cs                      # recibe Accion, contesta con la vista filtrada
  Salas/
    Sala.cs                         # EstadoPartida vigente + mapeo JugadorId<->ConnectionId + lock
    SalaRegistry.cs                 # diccionario en memoria de salas activas por join code
    JugadorConexion.cs              # tipo chico para el mapeo, sin filtrar a /core
  Vistas/
    VistaEstadoPartida.cs           # DTO de salida (si se elige la Opción A de §4)
    ProyectorDeVista.cs             # EstadoPartida + JugadorId -> VistaEstadoPartida
  Serializacion/
    ContadorJsonConverter.cs        # necesario por lo descrito en §2
  Senas/
    SenaMensaje.cs                  # DTO del canal de señas, fuera de Domain
```

Si en cambio se elige la Opción B de §4 (`Vistas.Para` dentro de `/core`),
`Vistas/ProyectorDeVista.cs` desaparece de `/server` y el DTO de salida
pasa a envolver el `VistaJugador` que devuelva `/core` — pero eso requiere
el OK que ya quedó anotado.

## 8. Decisiones que necesitan mi OK

- **Opción A vs B de la vista filtrada (§4)**: ¿el filtrado por jugador
  vive como DTO en `/server`, o como función pura nueva en `Domain`
  (`Vistas.Para`)? Lo segundo es más reusable pero cambia la superficie
  pública de `/core`.
- **Autenticación de jugadores**: ¿cómo se identifica *a la persona* detrás
  de un `JugadorId` al unirse o reconectar a una sala? Opciones: nada (solo
  join code + elegir asiento, sin persistencia de identidad — vale para
  jugar entre amigos pero cualquiera con el código puede "robar" un
  asiento vacío), un token simple por sesión de sala, o cuenta real. Esto
  también decide qué tan estricta puede ser la reconexión.
- **Persistencia**: ¿todo en memoria (una `Sala` se pierde si el proceso
  del servidor se reinicia) o se necesita guardar el `EstadoPartida`
  vigente en algún lado (archivo/DB) para sobrevivir un restart del
  servidor? Para jugar entre amigos en una sesión, memoria alcanza; para
  algo más serio, no.
- **Modelo de hosting**: ¿un solo proceso long-running (más simple, y
  suficiente mientras haya una sola instancia), o pensar desde ahora en
  múltiples instancias detrás de un balanceador (necesitaría sticky
  sessions o un backplane de SignalR para que todos los clientes de una
  sala terminen en el mismo proceso)? Recomiendo arrancar con un solo
  proceso y no resolver esto hasta que haga falta.
- **Límites de reconexión**: ¿cuánto tiempo se espera a que alguien
  reconecte antes de dar la mano/partida por abandonada? ¿Qué pasa con los
  puntos en juego si nadie reconecta (RULES ya cubre "irse al mazo", pero
  una desconexión no es lo mismo que irse al mazo a propósito)?
- **Señas por red**: aceptado el diseño de canal aparte (§6), falta decidir
  si señas es parte del alcance de la primera versión del servidor o si el
  server-side de señas espera a que `/core` las implemente primero
  (`senas-pendiente.md` todavía está sin cerrar del lado del dominio).

## 9. Riesgos

- **Concurrencia por sala**: si dos mensajes de la misma sala llegan casi
  al mismo tiempo (dos jugadores mandando acciones en simultáneo, o un
  reintento de red), hace falta serializar el acceso al `EstadoPartida` de
  esa `Sala` (un lock o una cola por sala) para no aplicar dos acciones
  sobre la misma versión del estado y perder una. Esto es puramente de
  `/server`; `/core` no necesita saber nada de esto porque es puro y no
  comparte estado entre llamadas.
- **`Contador` no es trivialmente serializable**: si se olvida el
  converter de §2, la primera serialización de un `EstadoPartida` real
  falla o produce un objeto vacío silenciosamente. Vale la pena un test de
  ida y vuelta apenas se arranque este proyecto.
- **Confiar en el `JugadorId` del propio mensaje**: es el error más fácil
  de cometer y el más grave — si el servidor arma el `Accion` a partir de
  lo que dice el cliente sin cruzarlo contra el mapeo de conexión, cualquier
  cliente modificado puede jugar por otro jugador. Tiene que ser imposible
  estructuralmente, no solo "validado en la mayoría de los casos".
- **Filtrado de vista con bugs = trampa gratis**: si `ProyectarPara` (o
  `Vistas.Para`) tiene un descuido y expone `ManosIniciales` ajenas o
  cartas de la mano de otro jugador, el resultado es indistinguible de un
  hack — el rival vería las cartas directamente en el JSON de red aunque
  la UI no las dibuje. Este código necesita tests explícitos por cada
  campo de `EstadoPartida`, no solo un test feliz de "el jugador ve sus
  cartas".
- **Divergencia entre el chequeo de legalidad del servidor y
  `AccionesLegales`**: si en algún punto el servidor arma su propio filtro
  adicional (por ejemplo, para UX, ocultar opciones que technically son
  legales pero no tienen sentido mostrar), hay que tener cuidado de no
  terminar con dos definiciones de "legal" que se puedan desincronizar.
  La regla dura tiene que seguir siendo: la única fuente de verdad sobre
  legalidad es `AccionesLegales`.
- **Acoplar de más el diseño de salas al 1v1**: `EstadoPartida` ya modela
  N jugadores (`CantidadJugadores`, `Activos` para el pico a pico de a 6),
  así que el mapeo de sala tiene que soportar desde el diseño 2, 4 y 6
  jugadores, no asumir 1v1 y tener que reescribirlo después.
