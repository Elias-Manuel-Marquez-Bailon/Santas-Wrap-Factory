# CLAUDE.md — Santa's Wrap Factory (demo académica)

## 1. Qué es este proyecto
Juego 3D tipo **Overcooked**, pero en la **fábrica de juguetes de Santa Claus**.
El jugador es un elfo que recibe pedidos (tarjetas arriba de la pantalla), arma/envuelve regalos
y los entrega en el **costal de Santa** antes de que se acabe el tiempo.

- Proyecto **académico**, no comercial. Solo una demo.
- Estilo visual: **low poly**, colores planos, sin texturas complejas.
- Un solo mapa (la fábrica), 3 niveles que solo cambian parámetros.
- **Un jugador** (prioridad). Segundo jugador local en el mismo teclado es opcional y va al final.
- **Sin multijugador en red.** No proponerlo.

## 2. Stack
- Unity 2022.3 LTS o Unity 6 (usar la versión con la que se creó el proyecto), render pipeline **URP**.
- C#. Sistema de input: **Input System nuevo** si ya está instalado; si no, `Input.GetAxis` clásico para ir rápido.
- UI: **TextMeshPro** + Canvas de Unity.
- Sin paquetes externos innecesarios.

## 3. Reglas de trabajo para Claude Code
1. **Jugabilidad primero, arte después.** Todo debe funcionar con primitivas de Unity (cubos, esferas, cilindros) con materiales de colores. Los modelos finales reemplazan a las primitivas al final.
2. Código **simple, comentado en español**, un script por responsabilidad. Nada de arquitecturas complejas.
3. Usar **ScriptableObjects** para datos (tipos de juguete, recetas, niveles). Así agregar contenido no requiere tocar código.
4. Claude Code **no puede abrir el editor de Unity**. Para no armar todo a mano, crear **scripts de Editor** (`Assets/_Project/Scripts/Editor/`) con menús tipo `Tools/Santa/...` que generen escena, prefabs y ScriptableObjects automáticamente con primitivas.
5. Después de cada paso, indicarme **qué debo hacer manualmente en Unity** (si hay algo) y **cómo probarlo**.
6. No refactorizar ni agregar funciones que no pedí. Si algo es opcional, preguntar antes.
7. Mantener el proyecto compilando siempre. Sin errores en consola al terminar un paso.

## 4. Estructura de carpetas

```
Assets/
└── _Project/
    ├── Art/
    │   ├── Models/
    │   │   ├── Toys/            # oso, carrito, robot, pelota
    │   │   ├── Gifts/           # caja, moños, papel
    │   │   ├── Stations/        # dispensador, mesa, envolvedora, costal
    │   │   ├── Environment/     # paredes, piso, props navideños, nieve
    │   │   └── Characters/      # elfo
    │   ├── Materials/           # materiales planos de colores (low poly)
    │   ├── Textures/
    │   └── UI/                  # íconos de las tarjetas, fuentes
    ├── Audio/
    │   ├── Music/
    │   └── SFX/
    ├── Prefabs/
    │   ├── Player/
    │   ├── Items/
    │   ├── Stations/
    │   └── UI/
    ├── Scenes/
    │   ├── MainMenu.unity
    │   ├── Factory.unity        # único mapa de juego
    │   └── GameOver.unity       # o panel dentro de Factory
    ├── ScriptableObjects/
    │   ├── Toys/
    │   ├── Orders/
    │   └── Levels/
    └── Scripts/
        ├── Core/                # GameManager, LevelManager, ScoreManager, Timer
        ├── Player/              # PlayerMovement, PlayerInteraction, PlayerCarry
        ├── Items/               # Item, ItemType, Pickable
        ├── Stations/            # Station (base), Dispenser, AssemblyTable, WrappingMachine, DeliveryBag
        ├── Orders/              # Order, OrderManager, OrderData
        ├── UI/                  # OrderCardUI, HUD, MenuUI, EndScreenUI
        ├── Data/                # definiciones de ScriptableObjects
        └── Editor/              # generadores de escena/prefabs
```

## 5. Diseño del juego

### Flujo de un regalo
1. **Dispensador** → entrega materiales crudos: juguete (según tipo), caja, papel (rojo/verde/azul), moño (dorado/plateado).
2. **Mesa de ensamblado** → juguete + caja = caja con juguete (tarda ~2 s con barra de progreso).
3. **Máquina envolvedora** → caja con juguete + papel + moño = regalo terminado (barra de progreso ~4 s).
4. **Costal de Santa** → entregar el regalo. Si coincide con un pedido activo, suma puntos; si no, resta o no cuenta.

### Pedido (tarjeta)
Un pedido = `tipo de juguete` + `color de papel` + `tipo de moño` + `tiempo límite`.
Se muestra como tarjeta arriba (como Overcooked) con íconos y una barra de tiempo.
Si expira, resta puntos.

### Variedad con pocos assets
- 4 juguetes × 3 papeles × 2 moños = 24 combinaciones distintas con solo ~9 assets.
- Opcional (si sobra tiempo): pedidos de tipo "solo fabricar el juguete" sin envolver.

### Niveles (solo cambian datos, mismo mapa)
| Nivel | Tiempo | Tipos de pedido | Frecuencia de pedidos | Extras |
|---|---|---|---|---|
| 1 | 150 s | solo juguete + caja + papel | lenta | ninguno |
| 2 | 120 s | juguete + papel + moño | media | banda transportadora |
| 3 | 120 s | todas las combinaciones | rápida | elfos traviesos / suelo resbaloso |

### Puntaje
- Entrega correcta: +20 (bonus por rapidez).
- Pedido expirado: -10.
- Meta de puntos por nivel para pasar (1 a 3 estrellas).

## 6. Scripts principales (qué debe hacer cada uno)

- `PlayerMovement`: movimiento con WASD/flechas relativo a cámara, rotación hacia donde camina, con `CharacterController` o `Rigidbody`.
- `PlayerInteraction`: detecta con raycast/overlap la estación o item más cercano al frente. `E` / `Espacio` = recoger, soltar o usar estación. Resalta lo que se puede interactuar.
- `PlayerCarry`: punto de mano (Transform) donde se sostiene **un solo item** a la vez.
- `Item` + `ItemType` (enum o ScriptableObject): toy, box, paper, bow, boxWithToy, giftReady. Guarda los atributos (tipo de juguete, color, moño).
- `Station` (clase base): estado, `Interact(player)`, `CanAccept(item)`.
- `Dispenser`: entrega un item de un tipo configurable.
- `AssemblyTable` / `WrappingMachine`: reciben items, validan, corren un temporizador con barra de progreso, producen el resultado. Se pueden quedar esperando a que el jugador lo recoja.
- `DeliveryBag`: valida contra `OrderManager`, suma puntos, destruye el item.
- `OrderManager`: genera pedidos según el `LevelData`, mantiene la lista activa (máx. 4), expira pedidos.
- `OrderCardUI`: instancia de tarjeta con íconos y barra de tiempo.
- `GameManager`: estados (Menu, Playing, Paused, Ended), cronómetro del nivel, carga de nivel.
- `ScoreManager`, `HUD`, `EndScreenUI`: puntaje, tiempo restante, pantalla final con estrellas y botón reintentar / siguiente nivel.

## 7. Plan por fases (con meta para el avance de mañana)

### Fase 1 — Prototipo jugable (HOY, prioridad máxima)
- [ ] Escena `Factory` con piso, paredes y cámara cenital inclinada (estilo Overcooked) hecha con cubos.
- [ ] Jugador (cápsula) que se mueve.
- [ ] Recoger y soltar items (un item a la vez).
- [ ] Dispensadores funcionando.
- [ ] Mesa de ensamblado con barra de progreso.
- [ ] Máquina envolvedora con barra de progreso.
- [ ] Costal que recibe regalos.

### Fase 2 — Pedidos y reglas (HOY, si alcanza)
- [ ] Sistema de pedidos + tarjetas arriba.
- [ ] Cronómetro, puntaje, pantalla final.
- [ ] Menú principal simple.

### Fase 3 — Arte (HOY/MAÑANA temprano)
- [ ] Materiales de colores consistentes (paleta navideña).
- [ ] Reemplazar primitivas por modelos low poly (ver sección 8).
- [ ] Iluminación fría + luces cálidas, partículas de nieve.

### Fase 4 — Contenido (después del avance)
- [ ] Niveles 2 y 3 (con ScriptableObjects).
- [ ] Sonido, música, pulido.
- [ ] Opcional: segundo jugador local, obstáculos extra.

**Avance de mañana = Fase 1 + Fase 2 funcionando, aunque se vea con cubos de colores.**
Es mejor mostrar un juego que se pueda jugar con arte simple que uno bonito que no funcione.

## 8. Estrategia de assets (low poly, estilo consistente)

Prioridad de fuentes, de más fácil a más difícil:

1. **Primitivas de Unity + materiales planos.** Una caja es un cubo, un moño son 2 cubos achatados en cruz, el papel es un color. Este estilo ya es low poly y es 100% consistente. Úsalo para todo lo que sea regalo, caja, estaciones y mapa.
2. **Packs gratuitos low poly** (revisar licencia y citar créditos en la presentación):
   - Kenney.nl (CC0)
   - Quaternius (CC0)
   - Poly Pizza
   - itch.io (buscar "low poly christmas", "low poly winter", "low poly factory")
   - Unity Asset Store (filtrar por gratis)
3. **IA 3D (Meshy, Tripo, etc.)** solo para los 3-4 juguetes y quizá el elfo:
   - Usar siempre el mismo prompt base, ej.: `low poly stylized teddy bear, flat colors, simple shapes, game asset, no texture detail`.
   - Pedir poca cantidad de polígonos y colores planos.
   - Revisar en Blender: escala, origen en la base, eliminar geometría rara, aplicar materiales planos.
   - Exportar como `.fbx` o `.glb` y poner en `Art/Models/`.
4. **Blender a mano** solo para formas muy simples (costal, moños) si hace falta.

Convenciones de importación:
- Escala 1 unidad = 1 metro, origen en la base del modelo.
- Un material por color, guardado en `Art/Materials/`, reutilizado en todos los modelos.
- Nombres: `Toy_Bear`, `Station_Wrapper`, `Env_Wall`, `Mat_RedPaper`.

## 9. Paleta sugerida
Rojo navideño `#C62828`, verde `#2E7D32`, azul invierno `#1565C0`, dorado `#F9A825`, plateado `#B0BEC5`, nieve `#ECEFF1`, madera `#8D6E63`, fondo morado frío `#4A3F6B`.

## 10. Primera tarea para Claude Code
Lee este archivo completo y haz lo siguiente, en orden, avisándome al terminar cada punto:
1. Verifica la estructura de carpetas de la sección 4 y créala si falta.
2. Crea los scripts base de **Player** (`PlayerMovement`, `PlayerInteraction`, `PlayerCarry`) y de **Items** (`Item`, `ItemType`).
3. Crea un script de Editor con menú `Tools/Santa/Build Prototype Scene` que genere la escena `Factory` con piso, paredes, cámara, jugador y un dispensador de prueba usando primitivas.
4. Dime exactamente qué debo hacer en Unity para probarlo.

Después seguimos con las estaciones y los pedidos.

## 11. Estado actual (actualizado 2026-11-26)
- Fase 1 y Fase 2 completas y probadas: estaciones, pedidos con tarjetas, puntaje, cronómetro, pausa, pantalla final, menú principal, 3 niveles (LevelData) y 4 juguetes (osito, carrito, robot, pelota) con primitivas.
- UX: avisos en pantalla (correcto / no coincide / expirado), textos de ayuda sobre las estaciones, tarjetas con color de juguete y "Sin moño".
- Para regenerar todo: `Tools/Santa/Build Full Game` (requiere TMP Essential Resources importado). Luego abrir `MainMenu` y Play.
- Los prefabs de items existentes NO se sobrescriben al regenerar (para no pisar modelos reemplazados a mano). Para regenerar uno, borrarlo primero.

## 12. Assets sugeridos (low poly, mismo estilo, preferible un solo pack/autor)
Formato `.fbx` o `.glb`. Revisar licencia y anotar autor para los créditos.

**Prioridad alta**
| Asset | Uso | Notas |
|---|---|---|
| Elfo | Jugador | Ideal con animaciones caminar/idle |
| Osito, carrito, robot, pelota | Juguetes | Tamaño ~30–50 cm |
| Caja de regalo | Caja, caja con juguete, regalo terminado | Una sola caja; el color se pone por código |
| Moño | Moño dorado/plateado | Uno solo; el color se cambia por código |
| Costal de Santa | Entrega | |

**Prioridad media (estaciones)**: mesa de trabajo de madera (ensamblado), máquina industrial (envolvedora), repisas/cajones/contenedores (dispensadores), rollo de papel (opcional).

**Prioridad baja (ambiente)**: pinos, adornos, guirnaldas, chimenea, ventanas, montones de regalos, paredes/piso de madera.

**Fuentes**: Kenney.nl (*Holiday Kit*, CC0), Quaternius (CC0), Poly Pizza, itch.io ("low poly christmas").

## 13. Ideas de mecánicas para expandir (pendiente decidir)
- **Fáciles**: mesas libres para dejar items, bote de basura, combo/multiplicador por entregas seguidas, selector de nivel con estrellas guardadas.
- **Medianas**: banda transportadora (nivel 2), suelo de hielo (nivel 3), elfo travieso que roba items, pedido urgente de Santa (doble puntos, poco tiempo), máquina que se atasca (presionar E varias veces).
- **Grandes**: segundo jugador local, juguetes con pasos extra (pintar carrito, pilas al robot), mapa que cambia a mitad del nivel.
- **Recomendado primero**: mesas libres + bote de basura + banda transportadora + suelo de hielo.
