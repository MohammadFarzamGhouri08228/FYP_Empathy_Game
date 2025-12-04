# Checkpoint System Implementation Plan

## Overview
Implement a checkpoint system that remembers player positions when they pass through checkpoints, and teleports the player back to the most recent checkpoint when they lose 2 lives.

## Design Pattern (Similar to Heart System)
- **HeartManager**: Uses `SpriteRenderer[] hearts` array
- **CheckpointManager**: Will use `Vector3[] checkpointPositions` array to store positions

## Components

### 1. CheckpointManager Script
**Purpose**: Main manager that stores checkpoint positions and handles player respawn

**Key Features**:
- Array of `Vector3` positions (similar to hearts array in HeartManager)
- Tracks the most recent checkpoint index
- Listens to health changes to detect when 2 lives are lost
- Teleports player back to most recent checkpoint when triggered
- Singleton pattern for easy access

**Inspector Fields**:
- `Transform[] checkpointTransforms` - Array of checkpoint transforms in the scene (optional, for visual reference)
- `Vector3[] checkpointPositions` - Array of stored checkpoint positions (auto-populated)
- `int mostRecentCheckpointIndex` - Index of the most recently passed checkpoint
- `GameObject player` - Reference to player GameObject (auto-found if not assigned)

**Methods**:
- `RegisterCheckpoint(Vector3 position)` - Adds/updates checkpoint position
- `RespawnAtCheckpoint()` - Teleports player to most recent checkpoint
- `OnHealthChanged(int currentHealth, int previousHealth)` - Detects when 2 lives are lost

### 2. Checkpoint Script
**Purpose**: Trigger zone that registers a checkpoint when player passes through

**Key Features**:
- Collider2D with IsTrigger enabled
- OnTriggerEnter2D detects player
- Calls CheckpointManager to register position
- Visual indicator (optional sprite/particle effect)

**Inspector Fields**:
- `bool isActive` - Whether this checkpoint is active
- `SpriteRenderer checkpointVisual` - Optional visual indicator
- `int checkpointID` - Optional ID for debugging

**Methods**:
- `OnTriggerEnter2D(Collider2D other)` - Detects player and registers checkpoint

## Integration with Health System

### Health Loss Detection
- Subscribe to `HealthSystem.OnHealthChanged` event
- Track previous health value
- When `previousHealth - currentHealth >= 2`, trigger respawn
- OR when `currentHealth <= maxHealth - 2` (if we want to track total lives lost)

### Respawn Logic
1. Check if there's a valid checkpoint (mostRecentCheckpointIndex >= 0)
2. Get player's Transform component
3. Set player position to checkpoint position
4. Reset player velocity (if using Rigidbody2D)
5. Optionally reset health to max (or keep current health)
6. Play respawn effect/audio (optional)

## Unity Configuration Steps

### Step 1: Create CheckpointManager GameObject
1. Create empty GameObject in scene
2. Name it "CheckpointManager"
3. Add `CheckpointManager` component
4. Configure inspector settings

### Step 2: Create Checkpoint Zones
1. For each checkpoint location:
   - Create empty GameObject
   - Name it "Checkpoint_1", "Checkpoint_2", etc.
   - Add `Checkpoint` component
   - Add `BoxCollider2D` or `CircleCollider2D`
   - Set collider to IsTrigger = true
   - Position at desired checkpoint location
   - Optionally add visual sprite/particle effect

### Step 3: Connect to Health System
1. Ensure player has `HealthSystem` component
2. CheckpointManager will automatically find and subscribe to it
3. Test by losing 2 lives

## Implementation Details

### Array Management (Similar to Hearts)
```csharp
// HeartManager pattern:
public SpriteRenderer[] hearts; // size = 2 in inspector

// CheckpointManager pattern:
public Vector3[] checkpointPositions; // Dynamic size, grows as checkpoints are passed
private int mostRecentCheckpointIndex = -1; // -1 means no checkpoint yet
```

### Position Storage
- Store player's position when passing checkpoint
- Use `transform.position` from player GameObject
- Store as Vector3 (works for 2D and 3D)

### Respawn Mechanics
- Instant teleport (no animation needed initially)
- Reset Rigidbody2D velocity to zero
- Ensure player is on valid ground (may need ground check)

## Testing Checklist
- [ ] Player passes through checkpoint → position is registered
- [ ] Multiple checkpoints → most recent is tracked correctly
- [ ] Player loses 2 lives → teleports to most recent checkpoint
- [ ] Player loses 1 life → no teleport (only 2 lives triggers it)
- [ ] No checkpoint passed yet → respawn at starting position or show error
- [ ] Health resets or maintains current value after respawn

## Future Enhancements (Optional)
- Visual checkpoint indicator (flag, light, etc.)
- Particle effect when checkpoint is activated
- Sound effect for checkpoint activation and respawn
- Animation for respawn (fade in/out)
- Save checkpoint data between scenes
- Checkpoint counter UI

