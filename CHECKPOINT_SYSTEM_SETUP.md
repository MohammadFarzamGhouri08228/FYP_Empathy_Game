# Checkpoint System Unity Setup Guide

## Overview
This guide will help you set up the checkpoint system in Unity. The system automatically teleports the player back to the most recent checkpoint when they lose 2 lives.

## Prerequisites
- Player GameObject tagged as "Player" (or has `PlayerController`/`PlayerController2` component)
- **IMPORTANT**: Player must have `HealthSystem` component attached (see Step 0 below)

## Step 0: Ensure Player Has HealthSystem Component

**This is required for the checkpoint system to work!**

1. **Select your Player GameObject** in the Hierarchy
2. **Check if it has `HealthSystem` component**:
   - Look in the Inspector
   - If you see a `HealthSystem` component, you're good! ✓
   - If not, continue to step 3
3. **Add HealthSystem component**:
   - Click **Add Component** in Inspector
   - Search for `HealthSystem`
   - Add it to the player
4. **Configure HealthSystem** (if needed):
   - Set **Max Health** to your desired value (e.g., 3 or 4)

**Note**: No checkpoint-related scripts need to be attached to the player. The `CheckpointManager` automatically finds the player and subscribes to its `HealthSystem` events.

## Step 1: Add CheckpointManager to Scene

1. **Create CheckpointManager GameObject**:
   - In Hierarchy, right-click → **Create Empty**
   - Name it `CheckpointManager`

2. **Add CheckpointManager Component**:
   - Select the `CheckpointManager` GameObject
   - In Inspector, click **Add Component**
   - Search for and add `CheckpointManager`

3. **Configure CheckpointManager** (optional):
   - **Player**: Leave empty (will auto-find) or drag player GameObject
   - **Reset Health On Respawn**: Check if you want health to reset to max on respawn
   - **Default Spawn Position**: Leave as (0,0,0) or set to player's starting position

## Step 2: Create Checkpoint Zones

For each location where you want a checkpoint:

1. **Create Checkpoint GameObject**:
   - In Hierarchy, right-click → **Create Empty**
   - Name it `Checkpoint_1`, `Checkpoint_2`, etc.

2. **Add Checkpoint Component**:
   - Select the checkpoint GameObject
   - In Inspector, click **Add Component**
   - Search for and add `Checkpoint`

3. **Add Collider2D**:
   - Click **Add Component** → **Physics 2D** → **Box Collider 2D** (or Circle Collider 2D)
   - In the collider component:
     - Check **Is Trigger** ✓
     - Adjust **Size** to cover the checkpoint area (recommended: 2-3 units wide/tall)

4. **Position the Checkpoint**:
   - Move the checkpoint GameObject to the desired location in the scene
   - This is where the player will respawn when they pass through it

5. **Configure Checkpoint** (optional):
   - **Checkpoint ID**: Set a unique number for debugging (0, 1, 2, etc.)
   - **Is Active**: Ensure it's checked ✓
   - **Allow Multiple Activations**: Uncheck if you only want it to activate once

6. **Add Visual Indicator** (optional but recommended):
   - Add **Sprite Renderer** component to checkpoint GameObject
   - Assign a sprite (flag, checkpoint icon, etc.)
   - The checkpoint will change color when activated (green = active, gray = inactive)
   - Or assign custom sprites in **Active Sprite** and **Inactive Sprite** fields

7. **Add Audio** (optional):
   - Add **Audio Source** component
   - Assign an audio clip to **Checkpoint Sound** field in Checkpoint component
   - Sound will play when checkpoint is activated

## Step 3: Verify Setup

1. **Check Player Setup**:
   - ✅ Player has `HealthSystem` component (REQUIRED - see Step 0)
   - ✅ Player is tagged as "Player" or has `PlayerController`/`PlayerController2` component
   - ✅ **No checkpoint scripts need to be attached to the player** - CheckpointManager handles everything automatically

2. **Test in Play Mode**:
   - Enter Play Mode
   - Move player through a checkpoint
   - Check Console for: `"Checkpoint X: Activated! Position registered: ..."`
   - Lose 2 lives (take damage twice)
   - Player should teleport back to the most recent checkpoint

## Step 4: Testing Checklist

- [ ] CheckpointManager GameObject exists in scene
- [ ] CheckpointManager component is added
- [ ] At least one Checkpoint GameObject exists
- [ ] Checkpoint has Collider2D with IsTrigger enabled
- [ ] Player has HealthSystem component
- [ ] Player passes through checkpoint → Console shows activation message
- [ ] Player loses 2 lives → Player teleports to checkpoint
- [ ] Player loses 1 life → No teleport (only 2 lives triggers respawn)

## Troubleshooting

### Checkpoint Not Activating
- **Issue**: Player passes through but nothing happens
- **Solution**: 
  - Check that Collider2D has **Is Trigger** enabled
  - Verify player has "Player" tag or PlayerController component
  - Check Console for error messages
  - Ensure CheckpointManager exists in scene

### Player Not Respawning
- **Issue**: Player loses 2 lives but doesn't teleport
- **Solution**:
  - Verify player has `HealthSystem` component
  - Check that at least one checkpoint has been activated
  - Check Console for error messages
  - Verify CheckpointManager found the player GameObject

### Player Respawning at Wrong Position
- **Issue**: Player respawns at (0,0,0) or wrong location
- **Solution**:
  - Ensure player passed through at least one checkpoint
  - Check that checkpoint was activated (see Console logs)
  - Verify checkpoint position in CheckpointManager inspector

### Multiple CheckpointManager Instances
- **Issue**: Console shows warning about multiple instances
- **Solution**: 
  - Only one CheckpointManager should exist per scene
  - Delete duplicate CheckpointManager GameObjects

## Advanced Configuration

### Custom Respawn Behavior
- Edit `CheckpointManager.cs` to modify respawn logic
- Change `resetHealthOnRespawn` to control health reset behavior
- Add animations/effects in `RespawnAtCheckpoint()` method

### Visual Feedback
- Add particle effects to checkpoint GameObject
- Use different sprites for active/inactive states
- Add animation component for checkpoint activation

### Multiple Checkpoints
- Create as many checkpoints as needed
- Each checkpoint stores the player's position when passed
- Only the most recent checkpoint is used for respawn

## Notes

- Checkpoints store the **player's position** when they pass through, not the checkpoint's position
- The system uses an array of `Vector3` positions (similar to how HeartManager uses `SpriteRenderer[]`)
- The most recent checkpoint index is tracked automatically
- If no checkpoint has been passed, player respawns at `defaultSpawnPosition`

