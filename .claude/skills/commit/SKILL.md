---
name: commit
description: Arma un commit del estado actual siguiendo el formato del proyecto.
disable-model-invocation: true
allowed-tools: Bash(git status *) Bash(git diff *) Bash(git add *) Bash(git commit *) Bash(dotnet test *)
---

Antes de commitear, corré `dotnet test`. Si algo falla, mostrá qué falló y no
commitees.

Después mostrá `git status` y `git diff --stat`, y confirmá qué archivos entran.

Formato del mensaje, en español, imperativo, sin punto final:

```
<tipo>(<ámbito>): <qué cambió>

<por qué, si no es evidente>
<regla de RULES_Afinadas.md afectada, si aplica>
```

Tipos: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`.
Ámbitos: `core`, `tests`, `docs`, `reglas`.

Ejemplos:

```
feat(core): implementar el 12 espejo del palo de la muestra

El 12 copia a la muestra solo cuando la muestra es pieza.
Sección "Jerarquia completa de las Cartas".
```

```
fix(core): corregir tantos del 10 de la muestra

Valía 26 y vale 27, igual que el 11. Fuerza y tantos son
funciones distintas.
```

No agregues coautoría ni firmas al mensaje. Nunca hagas `push` sin que te lo
pidan explícitamente.
