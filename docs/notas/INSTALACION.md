# Cómo instalar este paquete

Descomprimí el zip en la raíz del repo. Queda así:

```
tu-repo/
├── CLAUDE.md                          # contexto permanente
├── .claude/
│   └── skills/
│       ├── truco-uruguayo/SKILL.md    # dominio y trampas de las reglas
│       ├── core-dominio/SKILL.md      # arquitectura de /core
│       ├── tests-xunit/SKILL.md       # convenciones de test
│       ├── flujo-trabajo/SKILL.md     # alcance, ritmo, límites
│       └── commit/SKILL.md            # /commit, solo manual
├── docs/
│   ├── RULES_Afinadas.md              # el tuyo, no lo toques
│   └── PREGUNTAS_ABIERTAS.md          # 15 casos a decidir
└── PROMPT_INICIAL.md                  # el prompt, para copiar y pegar
```

No hay nada que "instalar": Claude Code descubre `.claude/skills/` solo al
arrancar en el repo. Verificalo con `/skills` o preguntándole
"¿qué skills tenés disponibles?".

`PROMPT_INICIAL.md` es para vos, no para el repo. Podés borrarlo después de
usarlo, o dejarlo en `/docs/notas`.

## Orden sugerido

1. Copiar los archivos.
2. Abrir Claude Code en el repo y correr `/skills` para confirmar que ve las cinco.
3. Leer `docs/PREGUNTAS_ABIERTAS.md` y decidir al menos A1, A2 y B1 — bloquean
   casi todo lo demás.
4. Pegar el contenido de `PROMPT_INICIAL.md` (lo que va debajo de la línea).
5. Fase 0. No dejarlo escribir código todavía.

## Ajustes que quizás necesites

- Si `RULES_Afinadas.md` no está en `docs/`, corregí la ruta en `CLAUDE.md`,
  en la skill `truco-uruguayo` y en `PROMPT_INICIAL.md`.
- Si tus proyectos no se llaman `/core` y `/tests`, lo mismo.
