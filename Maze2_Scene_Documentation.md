# Maze 2 Scene - System Documentation

## Table of Contents
1. [Software Design](#software-design)
2. [Data Design](#data-design)
3. [Technical Details](#technical-details)

---

## Software Design

### Architecture Overview

The Maze 2 scene implements a 2D maze exploration game with procedurally generated mazes, player movement, health management, fog of war, and interactive objects. The system follows a component-based architecture using Unity's MonoBehaviour pattern.

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Maze 2 Scene System                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐      ┌──────────────┐      ┌─────────────┐│
│  │   Maze       │──────│     Cell     │      │  Player     ││
│  │  Generator   │      │   (Data)     │      │ Controller  ││
│  └──────────────┘      └──────────────┘      └─────────────┘│
│         │                      │                    │        │
│         │                      │                    │        │
│  ┌──────▼──────┐      ┌────────▼────────┐   ┌──────▼──────┐ │
│  │   Walls     │      │  Maze Grid      │   │  Movement   │ │
│  │  & Floor    │      │  [Cell[,]]      │   │ Controller  │ │
│  └─────────────┘      └─────────────────┘   └─────────────┘ │
│                                                               │
│  ┌──────────────┐      ┌──────────────┐      ┌─────────────┐│
│  │   Camera     │      │   Player     │      │   Health    ││
│  │   Follow     │      │    Light     │      │  Management ││
│  └──────────────┘      └──────────────┘      └─────────────┘│
│         │                      │                    │        │
│         │                      │                    │        │
│  ┌──────▼──────┐      ┌────────▼────────┐   ┌──────▼──────┐ │
│  │  Fog of War │      │  Light Radius   │   │  Heart UI    │ │
│  │  SpriteMask │      │  Controller     │   │  Display     │ │
│  └─────────────┘      └─────────────────┘   └─────────────┘ │
│                                                               │
│  ┌──────────────┐      ┌──────────────┐      ┌─────────────┐│
│  │    Bomb      │      │  Obstacle    │      │   Object    ││
│  │ Interaction  │      │ Interaction  │      │ Interaction ││
│  └──────────────┘      └──────────────┘      └─────────────┘│
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### UML Class Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        MazeGenerator                         │
├─────────────────────────────────────────────────────────────┤
│ - floorPrefab: GameObject                                    │
│ - wallPrefab: GameObject                                     │
│ - startPrefab: GameObject                                   │
│ - goalPrefab: GameObject                                    │
│ - floorMaterial: Material                                   │
│ - wallMaterial: Material                                    │
│ - startMaterial: Material                                   │
│ - goalMaterial: Material                                    │
│ - player: GameObject                                        │
│ - height: int                                               │
│ - width: int                                                │
│ - wallHeight: float                                         │
│ - wallSeparation: float                                     │
│ - maze: Cell[,]                                             │
│ - cellRecord: Stack<Vector2Int>                             │
│ - usedTiles: int                                            │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ - GenerateMaze(): void                                      │
│ - checkAvailable(position: Vector2Int): List<int>           │
│ - createMaze(): void                                        │
│ - AssignMaterial(obj: GameObject, material: Material): void │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ uses
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         Cell (nsMaze)                        │
├─────────────────────────────────────────────────────────────┤
│ + isUsed: bool                                              │
│ + connected: bool[4]  // [N, E, S, W]                      │
├─────────────────────────────────────────────────────────────┤
│ + Cell()                                                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    PlayerController2                         │
├─────────────────────────────────────────────────────────────┤
│ - moveSpeed: float                                          │
│ - rb: Rigidbody2D                                           │
│ - moveInput: Vector2                                        │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ + FixedUpdate(): void                                       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                  PlayerMovementController                    │
├─────────────────────────────────────────────────────────────┤
│ - moveSpeed: float                                          │
│ - jumpForce: float                                          │
│ - groundCheck: Transform                                    │
│ - groundCheckRadius: float                                  │
│ - groundLayer: LayerMask                                    │
│ - rb: Rigidbody2D                                           │
│ - isMovingRight: bool                                       │
│ - isMovingLeft: bool                                        │
│ - isJumping: bool                                           │
│ - isGrounded: bool                                          │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ + FixedUpdate(): void                                       │
│ + OnRightButtonDown(): void                                 │
│ + OnRightButtonUp(): void                                   │
│ + OnLeftButtonDown(): void                                  │
│ + OnLeftButtonUp(): void                                   │
│ + OnJumpButtonDown(): void                                  │
│ + OnJumpButtonUp(): void                                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    CameraFollowMaze2                         │
├─────────────────────────────────────────────────────────────┤
│ - target: Transform                                         │
│ - autoFindPlayer: bool                                      │
│ - smoothSpeed: float                                        │
│ - offset: Vector3                                           │
│ - useBounds: bool                                           │
│ - minX, maxX, minY, maxY: float                            │
│ - velocity: Vector3                                         │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + LateUpdate(): void                                        │
│ + SetTarget(newTarget: Transform): void                     │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    PlayerLightController                     │
├─────────────────────────────────────────────────────────────┤
│ - minRadius: float                                          │
│ - maxRadius: float                                          │
│ - currentRadius: float                                      │
│ - adjustSpeed: float                                        │
│ - playerLight: Light2D                                      │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ + SetExplorationMode(): void                                │
│ + SetTightMode(): void                                      │
│ + SetRadius(radius: float): void                           │
│ + GetCurrentRadius(): float                                 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                        HeartManager                          │
├─────────────────────────────────────────────────────────────┤
│ + maxHealth: int                                            │
│ + currentHealth: int                                        │
│ + fullHeart: Sprite                                          │
│ + halfHeart: Sprite                                         │
│ + emptyHeart: Sprite                                        │
│ + hearts: SpriteRenderer[]                                  │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ + TakeDamage(dmg: int): void                                │
│ - UpdateHearts(): void                                      │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                        HealthSystem                          │
├─────────────────────────────────────────────────────────────┤
│ - maxHealth: int                                            │
│ - currentHealth: int                                       │
│ + OnHealthChanged: Action<int, int>                         │
│ + OnHealthDepleted: Action                                 │
│ + CurrentHealth: int {get}                                 │
│ + MaxHealth: int {get}                                      │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + TakeDamage(damage: int): void                            │
│ + Heal(amount: int): void                                  │
│ + ResetHealth(): void                                       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                      BombInteraction                         │
├─────────────────────────────────────────────────────────────┤
│ - destroyBomb: bool                                         │
│ - damageAmount: int                                         │
│ - heartManager: HeartManager                                │
│ - lastHitTime: float                                        │
│ - invincibilityDuration: float                              │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + OnTriggerEnter2D(other: Collider2D): void                │
│ + OnCollisionEnter2D(collision: Collision2D): void         │
│ - HandleBombInteraction(): void                           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    ObstacleInteraction                       │
├─────────────────────────────────────────────────────────────┤
│ - damageAmount: int                                         │
│ - destroyOnContact: bool                                    │
│ - invincibilityDuration: float                              │
│ - playerHealthSystem: HealthSystem                          │
│ - lastHitTime: float                                        │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + OnTriggerEnter2D(other: Collider2D): void                │
│ + OnCollisionEnter2D(collision: Collision2D): void         │
│ - IsPlayer(obj: GameObject): bool                          │
│ - HandleObstacleContact(): void                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    FogOfWarSpriteMask                        │
├─────────────────────────────────────────────────────────────┤
│ - visibleRadius: float                                      │
│ - fogColor: Color                                           │
│ - playerTransform: Transform                                │
│ - fogRenderer: SpriteRenderer                               │
│ - spriteMask: SpriteMask                                    │
│ - fogMaterial: Material                                     │
├─────────────────────────────────────────────────────────────┤
│ + Start(): void                                             │
│ + Update(): void                                            │
│ + SetVisibleRadius(radius: float): void                    │
│ - SetupFogOverlay(): void                                   │
└─────────────────────────────────────────────────────────────┘
```

### Class Descriptions

#### 1. MazeGenerator
**Purpose**: Generates procedurally created mazes using recursive backtracking algorithm.

**Attributes**:
- `floorPrefab`, `wallPrefab`, `startPrefab`, `goalPrefab`: GameObjects for maze construction
- `floorMaterial`, `wallMaterial`, `startMaterial`, `goalMaterial`: Materials for visual styling
- `height`, `width`: Maze dimensions
- `wallHeight`, `wallSeparation`: Physical properties of walls
- `maze`: 2D array of Cell objects representing the maze structure
- `cellRecord`: Stack tracking visited cells during generation
- `usedTiles`: Counter for processed cells

**Methods**:
- `Start()`: Initializes maze generation, validates prefabs, creates floor, and generates maze
- `GenerateMaze()`: Implements recursive backtracking algorithm to create maze structure
- `checkAvailable(Vector2Int)`: Returns list of available neighboring cells
- `createMaze()`: Instantiates wall and floor GameObjects based on maze structure
- `AssignMaterial(GameObject, Material)`: Helper method to assign materials to GameObjects

**Relationships**:
- Uses `Cell` class to represent maze cells
- Creates and positions player at start location
- Generates goal at random position

#### 2. Cell (nsMaze namespace)
**Purpose**: Data structure representing a single cell in the maze grid.

**Attributes**:
- `isUsed`: Boolean indicating if cell has been visited during generation
- `connected`: Boolean array of size 4 representing connections to North, East, South, West

**Methods**:
- `Cell()`: Constructor initializes cell as unused with no connections

**Relationships**:
- Used by `MazeGenerator` to build maze structure

#### 3. PlayerController2
**Purpose**: Handles player movement using keyboard input (WASD/Arrow keys).

**Attributes**:
- `moveSpeed`: Movement velocity
- `rb`: Reference to Rigidbody2D component
- `moveInput`: Current input vector

**Methods**:
- `Start()`: Initializes Rigidbody2D reference
- `Update()`: Reads keyboard input
- `FixedUpdate()`: Applies movement to Rigidbody2D

**Relationships**:
- Controls player GameObject movement
- Works with `CameraFollowMaze2` for camera tracking

#### 4. PlayerMovementController
**Purpose**: Alternative movement controller supporting button-based input and jumping.

**Attributes**:
- `moveSpeed`, `jumpForce`: Movement parameters
- `groundCheck`: Transform for ground detection
- `groundCheckRadius`, `groundLayer`: Ground detection parameters
- `isMovingRight`, `isMovingLeft`, `isJumping`, `isGrounded`: State flags

**Methods**:
- `Start()`: Initializes Rigidbody2D
- `Update()`: Checks ground state
- `FixedUpdate()`: Applies movement and jumping
- `OnRightButtonDown/Up()`, `OnLeftButtonDown/Up()`, `OnJumpButtonDown/Up()`: Button event handlers

**Relationships**:
- Can be used with UI buttons for mobile/touch input

#### 5. CameraFollowMaze2
**Purpose**: Smoothly follows the player with optional boundary constraints.

**Attributes**:
- `target`: Transform to follow
- `autoFindPlayer`: Auto-detect player if target not set
- `smoothSpeed`: Smoothing factor for camera movement
- `offset`: Camera offset from target
- `useBounds`, `minX`, `maxX`, `minY`, `maxY`: Boundary constraints
- `velocity`: Internal velocity for SmoothDamp

**Methods**:
- `Start()`: Finds player if auto-find enabled
- `LateUpdate()`: Smoothly moves camera to follow target
- `SetTarget(Transform)`: Manually set target at runtime

**Relationships**:
- Follows player GameObject
- Works with `PlayerController2` or `PlayerMovementController`

#### 6. PlayerLightController
**Purpose**: Controls player's flashlight light radius with smooth transitions.

**Attributes**:
- `minRadius`, `maxRadius`: Light radius bounds
- `currentRadius`: Target radius value
- `adjustSpeed`: Speed of radius transitions
- `playerLight`: Reference to Light2D component

**Methods**:
- `Start()`: Initializes Light2D reference and sets initial radius
- `Update()`: Smoothly interpolates light radius
- `SetExplorationMode()`: Sets radius to maximum
- `SetTightMode()`: Sets radius to minimum
- `SetRadius(float)`: Sets custom radius (clamped)
- `GetCurrentRadius()`: Returns current target radius

**Relationships**:
- Attached to player GameObject
- Works with Unity's Universal Render Pipeline Light2D

#### 7. HeartManager
**Purpose**: Manages player health display using heart sprites.

**Attributes**:
- `maxHealth`, `currentHealth`: Health values
- `fullHeart`, `halfHeart`, `emptyHeart`: Sprite references
- `hearts`: Array of SpriteRenderer components for display

**Methods**:
- `Start()`: Initializes health and validates sprites/renderers
- `Update()`: Temporary test input (X key for damage)
- `TakeDamage(int)`: Reduces health and updates display
- `UpdateHearts()`: Updates heart sprites based on current health

**Relationships**:
- Receives damage events from `BombInteraction`
- Displays health visually to player

#### 8. HealthSystem
**Purpose**: Core health management system with event notifications.

**Attributes**:
- `maxHealth`, `currentHealth`: Health values
- `OnHealthChanged`: Event fired when health changes
- `OnHealthDepleted`: Event fired when health reaches 0

**Methods**:
- `Start()`: Initializes health to maximum
- `TakeDamage(int)`: Reduces health, fires events
- `Heal(int)`: Restores health, fires events
- `ResetHealth()`: Resets to maximum health

**Relationships**:
- Used by `ObstacleInteraction` for damage
- Can be monitored by `HealthUI` for display updates

#### 9. BombInteraction
**Purpose**: Handles player interaction with bomb objects, causing damage.

**Attributes**:
- `destroyBomb`: Whether to destroy bomb on contact
- `damageAmount`: Damage dealt to player
- `heartManager`: Reference to HeartManager
- `lastHitTime`, `invincibilityDuration`: Invincibility frame management

**Methods**:
- `Start()`: Finds HeartManager in scene
- `OnTriggerEnter2D(Collider2D)`: Handles trigger collisions
- `OnCollisionEnter2D(Collision2D)`: Handles regular collisions
- `HandleBombInteraction()`: Processes bomb contact, applies damage

**Relationships**:
- Interacts with player GameObject
- Calls `HeartManager.TakeDamage()`

#### 10. ObstacleInteraction
**Purpose**: Handles player collision with obstacles, causing damage.

**Attributes**:
- `damageAmount`: Damage dealt
- `destroyOnContact`: Whether obstacle is destroyed
- `invincibilityDuration`: Invincibility frame duration
- `playerHealthSystem`: Reference to player's HealthSystem
- `lastHitTime`: Last damage time for invincibility

**Methods**:
- `Start()`: Finds player and HealthSystem
- `OnTriggerEnter2D(Collider2D)`: Handles trigger collisions
- `OnCollisionEnter2D(Collision2D)`: Handles regular collisions
- `IsPlayer(GameObject)`: Checks if GameObject is player
- `HandleObstacleContact()`: Processes obstacle contact

**Relationships**:
- Interacts with player GameObject
- Uses `HealthSystem.TakeDamage()`

#### 11. FogOfWarSpriteMask
**Purpose**: Implements fog of war effect using sprite masks to limit visibility.

**Attributes**:
- `visibleRadius`: Radius of visible area around player
- `fogColor`: Color of fog overlay
- `playerTransform`: Reference to player transform
- `fogRenderer`: SpriteRenderer for fog overlay
- `spriteMask`: SpriteMask component for visibility hole

**Methods**:
- `Start()`: Finds player and sets up fog overlay
- `Update()`: Updates mask position to follow player
- `SetVisibleRadius(float)`: Adjusts visible area radius
- `SetupFogOverlay()`: Configures fog rendering components

**Relationships**:
- Follows player GameObject position
- Creates visibility limitation effect

---

## Data Design

### Data Structure Overview

The Maze 2 scene primarily uses in-memory data structures during runtime. There is no persistent database, but the system maintains several key data structures:

### 1. Maze Grid Structure

**Entity**: Cell Grid
- **Type**: 2D Array `Cell[height, width]`
- **Purpose**: Represents the maze structure in memory
- **Attributes**:
  - `Cell.isUsed`: Boolean (visited during generation)
  - `Cell.connected[4]`: Boolean array [North, East, South, West]

**Relationships**:
- One-to-many: Each cell can connect to up to 4 neighbors
- Grid structure: Each cell has a position (x, y) in the grid

### 2. Player State

**Entity**: Player State
- **Type**: Runtime GameObject State
- **Attributes**:
  - Position: `Vector3` (x, y, z coordinates)
  - Health: `int` (currentHealth, maxHealth)
  - Light Radius: `float` (currentRadius, minRadius, maxRadius)
  - Movement State: `bool` flags (isMovingRight, isMovingLeft, isJumping, isGrounded)

**Relationships**:
- One-to-one with Player GameObject
- One-to-many with health events (OnHealthChanged)

### 3. Health System Data

**Entity**: Health Data
- **Type**: Runtime State
- **Attributes**:
  - `maxHealth`: int (default: 3-4)
  - `currentHealth`: int (0 to maxHealth)
  - Heart Sprites: `Sprite[]` (fullHeart, halfHeart, emptyHeart)

**Relationships**:
- One-to-one with Player
- One-to-many: Multiple damage sources can affect health

### 4. Maze Generation Stack

**Entity**: Cell Record Stack
- **Type**: `Stack<Vector2Int>`
- **Purpose**: Tracks visited cells during maze generation
- **Attributes**:
  - Each entry: `Vector2Int` (x, y position)

**Relationships**:
- Used during maze generation algorithm
- Temporary structure, cleared after generation

### Data Flow Diagram

```
┌─────────────┐
│   Start     │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ Initialize Maze │
│   Parameters    │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐      ┌──────────────┐
│ Generate Maze   │─────▶│  Cell Grid   │
│   Algorithm     │      │  [Cell[,]]   │
└──────┬──────────┘      └──────────────┘
       │
       ▼
┌─────────────────┐
│  Create Walls   │
│  & Floor        │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│  Initialize     │
│  Player State   │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐      ┌──────────────┐
│  Game Loop      │─────▶│  Update      │
│  (Runtime)      │      │  State       │
└─────────────────┘      └──────────────┘
       │
       ├──▶ Player Movement
       ├──▶ Health Updates
       ├──▶ Light Updates
       ├──▶ Camera Follow
       └──▶ Collision Detection
```

### Normalized Data Model (Conceptual)

While the system doesn't use a traditional database, the data relationships can be conceptualized as:

**Table: MazeCell**
- Primary Key: (row, column)
- isUsed: BOOLEAN
- connectedNorth: BOOLEAN
- connectedEast: BOOLEAN
- connectedSouth: BOOLEAN
- connectedWest: BOOLEAN

**Table: PlayerState**
- Primary Key: playerID (implicit, single player)
- positionX: FLOAT
- positionY: FLOAT
- positionZ: FLOAT
- currentHealth: INT
- maxHealth: INT
- lightRadius: FLOAT

**Table: GameObjects**
- Primary Key: objectID
- objectType: ENUM (Wall, Floor, Bomb, Obstacle, Goal, Checkpoint)
- positionX: FLOAT
- positionY: FLOAT
- positionZ: FLOAT
- isActive: BOOLEAN

**Relationships**:
- MazeCell (1) ── (many) GameObjects (Walls/Floor)
- PlayerState (1) ── (1) GameObjects (Player)
- GameObjects (many) ── (1) PlayerState (Collision interactions)

---

## Technical Details

### Algorithms

#### 1. Recursive Backtracking Maze Generation Algorithm

**Purpose**: Generates a perfect maze (one unique path between any two points) using recursive backtracking.

**Input**:
- `height`: Integer (maze height in cells)
- `width`: Integer (maze width in cells)
- Starting position: `Vector2Int(0, 0)` (top-left corner)

**Output**:
- `maze`: 2D array of `Cell` objects with connection information
- Physical maze structure with walls and paths

**Algorithm Steps**:

```
1. Initialize maze grid with unvisited cells
2. Initialize stack for backtracking
3. Set starting cell (0,0) as visited
4. Push starting cell to stack
5. WHILE (usedTiles < totalCells):
   a. Get current cell from stack top
   b. Find available unvisited neighbors
   c. IF neighbors exist:
      - Randomly select one neighbor
      - Create connection between current and neighbor
      - Mark neighbor as visited
      - Push neighbor to stack
      - Increment usedTiles
   d. ELSE:
      - Pop from stack (backtrack)
      - Update current position to new stack top
6. END WHILE
```

**Pseudocode**:

```csharp
function GenerateMaze():
    maze = new Cell[height, width]
    cellRecord = new Stack<Vector2Int>()
    position = Vector2Int(0, 0)
    usedTiles = 1
    maze[0, 0].isUsed = true
    cellRecord.Push(position)
    
    while usedTiles < height * width:
        availableDir = checkAvailable(cellRecord.Peek())
        
        if availableDir.Count > 0:
            nextDir = availableDir[Random.Range(0, availableDir.Count)]
            
            switch nextDir:
                case 0: // North
                    maze[position.x, position.y].connected[0] = true
                    maze[position.x - 1, position.y].connected[2] = true
                    position.x -= 1
                case 1: // East
                    maze[position.x, position.y].connected[1] = true
                    maze[position.x, position.y + 1].connected[3] = true
                    position.y += 1
                case 2: // South
                    maze[position.x, position.y].connected[2] = true
                    maze[position.x + 1, position.y].connected[0] = true
                    position.x += 1
                case 3: // West
                    maze[position.x, position.y].connected[3] = true
                    maze[position.x, position.y - 1].connected[1] = true
                    position.y -= 1
            
            maze[position.x, position.y].isUsed = true
            cellRecord.Push(position)
            usedTiles++
        else:
            if cellRecord.Count > 1:
                cellRecord.Pop()
                position = cellRecord.Peek()
```

**Time Complexity**: O(n) where n = height × width (each cell visited exactly once)
**Space Complexity**: O(n) for maze array + O(n) for stack (worst case)

**Techniques Used**:
- Stack-based backtracking
- Random selection for maze variety
- Bidirectional connection marking

#### 2. Wall Placement Algorithm

**Purpose**: Converts maze cell connections into physical wall GameObjects.

**Input**:
- `maze`: 2D array of Cell objects with connection data
- `wallPrefab`: GameObject prefab for walls
- `wallHeight`, `wallSeparation`: Physical dimensions

**Output**:
- Instantiated wall and floor GameObjects in scene

**Algorithm Steps**:

```
1. Create border walls (perimeter)
2. FOR each cell in maze:
   a. Create center wall at cell intersection
   b. IF no South connection:
      - Create South wall
   c. IF no East connection:
      - Create East wall
3. Create goal at random position
```

**Pseudocode**:

```csharp
function createMaze():
    wallScale = Vector3(wallSeparation, wallHeight, wallSeparation)
    
    // Create border walls
    for i = 0 to 2 * height:
        CreateWall(-wallSeparation, wallHeight/2, i * wallSeparation)
        CreateWall(2 * width * wallSeparation, wallHeight/2, i * wallSeparation)
    
    for i = 0 to 2 * width:
        CreateWall(i * wallSeparation, wallHeight/2, -wallSeparation)
        CreateWall(i * wallSeparation, wallHeight/2, 2 * height * wallSeparation)
    
    // Create interior walls
    for i = 0 to height:
        for j = 0 to width:
            // Center intersection wall
            CreateWall(wallSeparation * (j * 2 + 1), wallHeight/2, wallSeparation * (i * 2 + 1))
            
            // South wall (if no connection)
            if !maze[i, j].connected[2]:
                CreateWall(j * 2 * wallSeparation, wallHeight/2, wallSeparation * (i * 2 + 1))
            
            // East wall (if no connection)
            if !maze[i, j].connected[1]:
                CreateWall(wallSeparation * (j * 2 + 1), wallHeight/2, i * 2 * wallSeparation)
    
    // Create goal
    goalPos = Random position in maze
    CreateGoal(goalPos)
```

**Time Complexity**: O(n) where n = height × width
**Space Complexity**: O(n) for instantiated GameObjects

**Techniques Used**:
- Grid-based positioning
- Conditional wall placement based on connections
- Prefab instantiation

#### 3. Smooth Camera Following Algorithm

**Purpose**: Smoothly follows player with optional boundary constraints.

**Input**:
- `target`: Transform of player
- `offset`: Vector3 camera offset
- `smoothSpeed`: Float smoothing factor
- `useBounds`: Boolean for boundary constraints
- `minX`, `maxX`, `minY`, `maxY`: Boundary values

**Output**:
- Updated camera position each frame

**Algorithm**:

```csharp
function LateUpdate():
    if target == null: return
    
    desiredPosition = target.position + offset
    
    if useBounds:
        desiredPosition.x = Clamp(desiredPosition.x, minX, maxX)
        desiredPosition.y = Clamp(desiredPosition.y, minY, maxY)
    
    smoothedPosition = SmoothDamp(currentPosition, desiredPosition, velocity, smoothSpeed)
    transform.position = smoothedPosition
```

**Time Complexity**: O(1) per frame
**Techniques Used**:
- SmoothDamp for velocity-based interpolation
- LateUpdate for frame-independent smoothing
- Boundary clamping

#### 4. Light Radius Interpolation Algorithm

**Purpose**: Smoothly transitions player light radius between min and max values.

**Input**:
- `currentRadius`: Target radius
- `minRadius`, `maxRadius`: Bounds
- `adjustSpeed`: Transition speed
- `playerLight`: Light2D component

**Output**:
- Updated light radius each frame

**Algorithm**:

```csharp
function Update():
    targetRadius = Clamp(currentRadius, minRadius, maxRadius)
    currentLightRadius = playerLight.pointLightOuterRadius
    
    if Abs(currentLightRadius - targetRadius) > 0.01:
        newRadius = Lerp(currentLightRadius, targetRadius, adjustSpeed * deltaTime)
        playerLight.pointLightOuterRadius = newRadius
    else:
        playerLight.pointLightOuterRadius = targetRadius
```

**Time Complexity**: O(1) per frame
**Techniques Used**:
- Linear interpolation (Lerp)
- Frame-rate independent using Time.deltaTime
- Threshold check for performance

#### 5. Health Display Update Algorithm

**Purpose**: Updates heart sprites based on current health value.

**Input**:
- `currentHealth`: Integer (0 to maxHealth)
- `maxHealth`: Integer
- `hearts`: Array of SpriteRenderer components
- `fullHeart`, `halfHeart`, `emptyHeart`: Sprite references

**Output**:
- Updated heart sprites in UI

**Algorithm**:

```csharp
function UpdateHearts():
    health = currentHealth
    
    for i = 0 to hearts.Length - 1:
        if health >= 2:
            hearts[i].sprite = fullHeart
            health -= 2
        else if health == 1:
            hearts[i].sprite = halfHeart
            health -= 1
        else:
            hearts[i].sprite = emptyHeart
```

**Time Complexity**: O(n) where n = number of hearts
**Techniques Used**:
- Sequential sprite assignment
- Health value decomposition (each heart = 2 health points)

#### 6. Invincibility Frame Algorithm

**Purpose**: Prevents multiple damage instances in quick succession.

**Input**:
- `lastHitTime`: Float timestamp of last damage
- `invincibilityDuration`: Float duration of invincibility
- `Time.time`: Current game time

**Output**:
- Boolean: Whether damage should be applied

**Algorithm**:

```csharp
function HandleDamage():
    if Time.time - lastHitTime < invincibilityDuration:
        return // Still invincible
    
    ApplyDamage()
    lastHitTime = Time.time
```

**Time Complexity**: O(1)
**Techniques Used**:
- Time-based cooldown
- Timestamp comparison

### Tool Chain Integration

1. **Maze Generation** → **Wall Creation** → **Scene Population**
   - Algorithm generates maze structure
   - Structure converted to GameObjects
   - GameObjects placed in Unity scene

2. **Player Input** → **Movement Controller** → **Physics System** → **Camera Follow**
   - Input captured in Update()
   - Movement applied in FixedUpdate()
   - Rigidbody2D handles physics
   - Camera follows in LateUpdate()

3. **Collision Detection** → **Damage System** → **Health Update** → **UI Update**
   - Unity collision events trigger
   - Damage calculated and applied
   - Health system updates
   - HeartManager updates visual display

4. **Light System** → **Fog of War** → **Visibility**
   - PlayerLightController adjusts radius
   - FogOfWarSpriteMask follows player
   - Combined effect creates visibility limitation

### Performance Considerations

1. **Maze Generation**: Runs once at Start(), O(n) complexity acceptable
2. **Wall Instantiation**: Batch creation at initialization, no runtime overhead
3. **Camera Smoothing**: O(1) per frame, efficient
4. **Light Interpolation**: O(1) per frame, minimal overhead
5. **Collision Detection**: Unity's built-in system, optimized internally
6. **Health Updates**: O(n) where n = heart count (typically 2-4), negligible

---

## Summary

The Maze 2 scene implements a complete 2D maze exploration game with:
- Procedural maze generation using recursive backtracking
- Player movement with multiple control schemes
- Health management with visual feedback
- Fog of war and dynamic lighting
- Interactive objects (bombs, obstacles)
- Smooth camera following
- Collision detection and damage systems

The architecture is modular, component-based, and follows Unity best practices for maintainability and extensibility.

