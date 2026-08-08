---
name: flujo-trabajo
description: Cómo trabajar en este proyecto: alcance, límites, ritmo de cambios, y qué hacer ante una ambigüedad de reglas. Usá esta skill al empezar cualquier tarea de varios pasos, al planificar, o cuando estés por tocar archivos fuera de /core y /tests.
when_to_use: plan, planificar, empezar, alcance, refactor, siguiente paso, qué hago ahora, agregar dependencia, crear proyecto, Unity, servidor
---

# Flujo de trabajo

## Reparto de roles

El autor escribe la lógica de reglas a mano **a propósito**: es parte del
objetivo de aprendizaje del proyecto. Tu rol es revisar, corregir, cerrar
huecos, y armar el andamiaje alrededor (tipos, tests, estructura, refactors
propuestos).

No reescribas código que él escribió sin decirlo primero. Si su implementación
está mal, mostrá el caso que falla antes de proponer el arreglo — el objetivo es
que él vea el error, no que desaparezca.

## Leer antes de escribir

Antes de la primera edición de una sesión: `docs/RULES_Afinadas.md`,
`docs/PREGUNTAS_ABIERTAS.md`, y el código existente de `/core` y `/tests`.
El código ya empezado es una decisión de diseño, no un borrador a descartar.

## Un paso por vez

Trabajá en pasos chicos e independientes. Al cerrar cada uno, informá tres
cosas: qué cambió, por qué, y qué test lo cubre. Después parás y esperás.

No encadenes cinco pasos del plan en un turno. El autor está aprendiendo el
código junto con vos; un diff grande le saca eso.

## Ante una ambigüedad de reglas: pará

Si el documento no resuelve el caso que estás implementando:

1. Fijate si ya está en `docs/PREGUNTAS_ABIERTAS.md` con decisión tomada.
2. Si no está decidido, **no elijas un default**. Agregalo al archivo con las
   opciones y tu recomendación, y preguntá.

Un default silencioso acá no rompe la compilación ni los tests: aparece meses
después, jugando, y es carísimo de rastrear.

## Límites de alcance

**No toques `/game` ni `/server`.** No existen para vos todavía. Unity es Fase 4
y el servidor viene después. Si te parece que algo de `/core` "necesitaría" el
motor, es señal de que el diseño está mal — decilo, no lo construyas.

**No implementes señas.** El documento las nombra pero no las especifica.

**Preguntá antes de:** agregar paquetes NuGet, crear proyectos o soluciones
nuevas, refactorizar más de un archivo a la vez, cambiar la forma de la API del
dominio, o modificar `RULES_Afinadas.md`.

## Sobre modificar las reglas

`docs/RULES_Afinadas.md` lo escribe el autor. Vos podés proponer correcciones
—contradicciones internas, casos faltantes, ejemplos que no cierran— pero no lo
editás por tu cuenta. Las propuestas van como lista de cambios sugeridos.

## Definición de terminado

Un paso está terminado cuando: compila, `dotnet test` pasa entero, la regla
nueva tiene un test que fallaba antes, no quedaron `TODO` sin anotar, y el autor
sabe qué cambió.
