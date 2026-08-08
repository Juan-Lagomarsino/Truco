---
name: tests-xunit
description: Convenciones de testing con xUnit para este proyecto. Los tests son la especificación ejecutable de las reglas del Truco. Usá esta skill siempre que escribas, modifiques o revises un test, o cuando implementes una regla nueva (porque toda regla nueva necesita su test).
when_to_use: test, tests, xunit, Fact, Theory, InlineData, assert, cobertura, TDD, /tests, verificar una regla
---

# Tests

## Para qué existen los tests acá

No para verificar que el código anda. Para **ser la especificación de las
reglas**. `RULES_Afinadas.md` está en prosa; `/tests` es la misma cosa en forma
ejecutable. Cuando el autor afine una regla del documento, el test que rompe le
dice exactamente qué código tocar.

De eso se desprende todo lo demás.

## Trazabilidad al documento

Cada test de reglas referencia la sección de `RULES_Afinadas.md` que verifica.
El nombre del test es la regla en castellano:

```csharp
[Fact]
public void El12DelPaloDeLaMuestra_EspejaLaMuestra_SoloSiLaMuestraEsPieza() { }

[Fact]
public void SiTodasLasBazasSonPardas_GanaElQueEsMano() { }

[Fact]
public void ConUnaPieza_ElEnvidoEsLaPiezaMasLaMejorDeLasOtrasDos() { }
```

Un lector que no conoce el código tiene que poder leer la lista de nombres de
test y reconstruir las reglas del juego. Si un nombre no logra eso, está mal
puesto.

## Estructura

- `[Theory]` con `[InlineData]` para todo lo que sea tabla de casos: la
  jerarquía de fuerza, el recuento de tantos, la resolución de bazas, los
  puntajes de cada canto. El documento ya viene en forma de tabla; el test
  también.
- `[Fact]` para casos únicos o secuencias de cantos.
- Un `Assert` conceptual por test. Si necesitás tres asserts sin relación, son
  tres tests.
- Sin mocks. `/core` es puro, así que todo test es: armar estado literal,
  aplicar acciones, verificar el estado resultante.

## Los ejemplos del documento son tests obligatorios

`RULES_Afinadas.md` trae ejemplos numéricos concretos (muestra 3 de Oro con 2,
7 de Oro y 5 de Copa da 37; tres piezas dan 47; 6 y 5 de Basto más 11 de Copa
dan 31; 7, 6 y 12 de Espada dan 33; 5 de Oro con 7 y 6 de Basto dan 41). Todos
tienen que existir como caso de test citando el ejemplo. Son la verificación más
barata de que se entendió el recuento.

Lo mismo con la tabla de resolución de manos: sus ocho filas son ocho casos.

## Invariantes

Además de los casos, testeá las propiedades que tienen que valer siempre. Estas
atrapan la clase de error que los casos puntuales no ven:

- Para las 40 muestras posibles: la tabla de fuerza cubre exactamente 40 cartas,
  sin huecos ni duplicados de nivel de pieza.
- Para cualquier mano de 3 cartas: el envido cae en [0, 37] y la flor, si la
  hay, en [20, 47].
- Toda partida simulada con acciones aleatorias (semilla fija, muchas semillas)
  termina, tiene exactamente un ganador, y ningún puntaje decrece.
- `AccionesLegales` nunca devuelve lista vacía para el jugador en turno.

Si conviene traer una librería de property-based testing (CsCheck, FsCheck),
proponelo primero — agregar dependencias requiere aprobación.

## Ritmo

Todo cambio de reglas viene con un test que **falla antes y pasa después**.
Escribí el test primero, mostrá que falla, después implementá. Si el test pasa
antes de tocar el código, el test está mal.

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~Jerarquia"
```
