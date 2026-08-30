# Plan nocturno — trabajo autónomo

Instrucciones para una corrida desatendida de Claude Code. Lanzar parado en la raíz del repo con
`claude --permission-mode bypassPermissions` y pegar todo lo que sigue (o decir: "seguí este archivo").

---

Voy a dormir. Trabajás SOLO toda la noche, sin esperarme nunca, hasta terminar todo lo que puedas
o quedarte sin contexto/uso.

Antes de nada, leé: CLAUDE.md, docs/RULES_Afinadas.md completo, docs/PREGUNTAS_ABIERTAS.md,
y todo /core y /tests. Respetá las reglas duras de /core (sin Random sin semilla, sin DateTime,
sin IO, sin async, sin UnityEngine, sin estático) y el estilo de commits (español imperativo,
sin punto final, sin coautoría).

REGLA DE ORO DE LA NOCHE:
- Hacé TODO lo que puedas hacer sin mí. No me esperes para nada.
- Si algo necesita una decisión mía o no lo podés resolver solo, NO frenes: anotalo en
  docs/notas/PREGUNTAS_PENDIENTES.md (contexto, qué necesito decidir, opciones, tu recomendación
  fundamentada) y SEGUÍ con el siguiente paso que sí puedas hacer. El orden es una guía, no una
  traba: si un paso queda pendiente por una pregunta, saltá al que siga y volvés después.
- Cosas que SÍ podés decidir vos solo (no son preguntas): arquitectura, andamiaje, estructura de
  proyectos, nombres. Decidilas y anotá el porqué en docs/notas/DECISIONES_NOCTURNAS.md.
- LÍNEA ROJA: NO inventes ni completes de memoria NINGUNA regla del truco que no esté escrita en
  RULES_Afinadas o PREGUNTAS_ABIERTAS. Si un paso depende de una regla ambigua, eso ES una
  pregunta pendiente: anotala y saltá. Nunca la resuelvas vos.

REGLAS DE SEGURIDAD:
- Trabajá en una rama nueva: `git switch -c noche/roadmap`. NO hagas push. NO borres archivos
  (nada de rm ni reset --hard). NO toques `main` ni `devIA`.
- Todo cambio de código: test-first (un test xUnit que falle antes y pase después), y dejá
  build+test en verde antes de commitear. Para compilar/testear:
  export DOTNET_ROOT=$HOME/.dotnet; export PATH=$HOME/.dotnet:$PATH; dotnet test
- Un commit chico por sub-paso, con qué cambió y qué test lo cubre.

PARALELISMO: usá subagentes para hacer EN PARALELO las tareas de análisis/propuesta que no tocan
código (guarda de reglas, revisión de señas, diseño de la grabación, y los planes de Unity/server/
publicación). El código real (/core, /bot, /cli) hacelo en SERIE para no pisarte. Los subagentes
devuelven un resumen; no dependas de que sigan vivos. Los pasos de PLANIFICAR (7 en adelante) son
sólo escribir, así que arrancalos en paralelo temprano y no bloquean nada.

LISTA (1 a 6 = HACER; 7 en adelante = sólo PLANIFICAR por escrito). Hacé todo lo que puedas de
cada uno; lo que no, va a PREGUNTAS_PENDIENTES y seguís:
1. Verificá la suite en verde: build + test, reportá cuántos tests hay y cuántos pasan. Si algo
   falla, diagnosticá y anotá en PREGUNTAS_PENDIENTES; no lo arregles a lo bruto.
2. Guarda automática de las reglas duras de /core: un test (o analizador) que falle si aparece
   Random sin semilla, DateTime, IO, async, estático o UnityEngine en /core. Alcance /tests.
3. Señas: SÓLO si docs/notas/SEÑAS.md está 100% cerrado y sin convenciones sin confirmar,
   implementalo en /core test-first. Si hay cualquier hueco, va a PREGUNTAS_PENDIENTES y NO
   implementás (no inventes gestos ni reglas).
4. Bot: librería nueva /bot que dependa de /core, función pura EstadoPartida a Acción de
   AccionesLegales, política simple y honesta (sólo lo que ese jugador ve). Test-first bot vs bot
   sin deadlock.
5. Consola jugable: proyecto /cli que referencia /core y /bot para jugar 1v1 humano-vs-bot en
   terminal. Console/IO SÓLO en /cli, jamás en /core. Dejalo corriendo con dotnet run.
6. Grabación/reproducción: modelo en /core (semilla + lista de Acciones), IO fuera de /core.
   Test-first: grabar una partida fuzz, reproducirla, assertear estado idéntico paso a paso.
7-9. Unity (setup en /game + referencia a /core, assets, render, input, loop 1v1 vs bot),
   10-13 server (/server ASP.NET Core + SignalR, sync autoritativa, salas, reconexión),
   14. publicación Android/iOS/Steam: NO scaffoldees ni ejecutes nada de esto (necesita mi OK y
   Unity abierto). Para CADA uno escribí un plan fundamentado en docs/notas/PLAN_<tema>.md:
   enfoque, cómo se linkea /core, archivos a crear, decisiones que necesitan mi OK (esas también
   a PREGUNTAS_PENDIENTES), y riesgos.

Seguí ciclando por la lista mientras haya algo que puedas avanzar; no pares sólo porque juntaste
preguntas. Al terminar todo lo posible o antes de quedarte sin contexto, escribí
docs/notas/REPORTE_NOCHE.md: qué hiciste, lista de commits, decisiones (link a
DECISIONES_NOCTURNAS.md), todas las preguntas pendientes juntas, y el próximo paso exacto para
retomar. Después pará.

ALCANCE Y DEPENDENCIAS (no te vayas del proyecto, no instales nada):
- Trabajá SÓLO dentro de este repo. No crees ni edites archivos fuera de la carpeta del proyecto.
- NO instales nada: ni paquetes NuGet nuevos (`dotnet add package`), ni tools globales
  (`dotnet tool`), ni apt/snap/pip/npm/etc. Usá sólo lo que ya está en el repo y el SDK instalado.
- Si algún paso necesita una dependencia o herramienta nueva: NO la instales — anotá en
  docs/notas/PREGUNTAS_PENDIENTES.md qué necesitás y por qué, y seguí con otro paso.
- Referencias entre proyectos del repo (`dotnet add reference` de /cli o /bot hacia /core) sí están
  permitidas; eso no es una dependencia externa.
