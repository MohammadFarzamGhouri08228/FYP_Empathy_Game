# Far Distance Dialogue Setup Guide

This guide explains how to set up a dialogue box that appears at a far distance from the player and remains visible as the camera moves. The dialogue changes when you press 'P'.

## Quick Setup Steps

### Step 1: Add the Script to Your Scene

1. **Open your Maze2 scene** in Unity
2. **Create an empty GameObject**:
   - Right-click in Hierarchy → **Create Empty**
   - Name it `FarDistanceDialogueController`
3. **Add the Component**:
   - Select the GameObject
   - Click **Add Component** in Inspector
   - Search for and add `FarDistanceDialogueController`

### Step 2: Set Up Dialogue Box Manager (if not already done)

1. **Check if DialogueBoxManager exists** in your scene
   - If it doesn't exist, create one:
     - Right-click Hierarchy → **Create Empty**
     - Name it `DialogueBoxManager`
     - Add Component → `DialogueBoxManager`
2. **Configure DialogueBoxManager**:
   - **Dialogue Box Sprites**: Drag your sliced sprites from `speech-bubble-vector-halftone-style-set.png`
   - **Default Sprite Index**: `0`
   - **Text Font Size**: `24` (or your preferred size)
   - **Text Color**: `Black` (or your preferred color)
   - **Padding**: `X: 20, Y: 20`

### Step 3: Configure FarDistanceDialogueController

1. **Select the FarDistanceDialogueController GameObject**
2. **In the Inspector, configure these settings**:

   **Dialogue Settings:**
   - **Dialogue Messages**: Add your dialogue messages (default has 4 example messages)
     - Click the `+` button to add more messages
     - Edit the text in each field
   
   **Position Settings:**
   - **Position Mode**: Choose how the dialogue is positioned:
     - **Fixed**: Dialogue stays at a fixed world position (may go out of view if player moves far)
     - **Follow Camera**: Dialogue follows the camera with an offset (always visible, appears far away)
     - **Follow Player**: Dialogue follows the player with an offset (always visible, appears far away)
     - **Recommended**: Use **Follow Camera** to ensure dialogue is always visible
   - **Dialogue World Position**: Set to a far position (e.g., `X: 50, Y: 10, Z: 0`)
     - Only used if Position Mode is **Fixed**
   - **Follow Offset**: Offset from camera/player position (e.g., `X: 30, Y: 5, Z: 0`)
     - Used when Position Mode is **Follow Camera** or **Follow Player**
     - This makes the dialogue appear "far away" while staying visible
   - **Player Transform**: (Optional) Drag your Player GameObject here (auto-finds if not set)
   - **Camera Transform**: (Optional) Drag your Camera GameObject here (auto-finds Main Camera if not set)
   - **Use Relative Position**: Check this if you want position relative to player's starting position (only for Fixed mode)
   - **Relative Offset**: If using relative position, set offset (e.g., `X: 30, Y: 5, Z: 0`)
   
   **Dialogue Box Manager:**
   - **Dialogue Manager**: Drag your `DialogueBoxManager` GameObject here
     - If left empty, it will try to find one automatically
   
   **Input Settings:**
   - **Change Dialogue Key**: `P` (default)
   
   **Display Settings:**
   - **Dialogue Sprite Index**: `0` (which sprite from your array to use)
   - **Show On Start**: Check this to show dialogue automatically when scene starts

### Step 4: Test It!

1. **Enter Play Mode**
2. **The dialogue should appear** at the far distance position
3. **Press 'P'** to cycle through the dialogue messages
4. **Move the player** - the dialogue should remain visible at its world position as the camera follows the player

## How It Works

- The script creates a **World Space Canvas** for the dialogue
- **Position Modes:**
  - **Fixed**: Dialogue stays at a fixed world position (may go out of view if player moves far)
  - **Follow Camera**: Dialogue follows the camera with an offset, ensuring it's always visible while appearing far away
  - **Follow Player**: Dialogue follows the player with an offset, ensuring it's always visible while appearing far away
- The dialogue position updates every frame when using Follow Camera or Follow Player modes
- Pressing 'P' cycles through the dialogue messages array
- **No additional camera/player scripts needed** - the dialogue controller handles everything automatically

## Customization

### Changing Dialogue Messages

You can modify the dialogue messages in the Inspector, or add them programmatically:

```csharp
FarDistanceDialogueController controller = FindFirstObjectByType<FarDistanceDialogueController>();
controller.AddDialogueMessage("New message here!");
```

### Changing Position at Runtime

```csharp
controller.SetDialoguePosition(new Vector3(100f, 20f, 0f));
```

### Changing to Specific Dialogue

```csharp
controller.ChangeToDialogue(2); // Change to dialogue at index 2
```

## Troubleshooting

**Dialogue doesn't appear?**
- Check that DialogueBoxManager has sprites assigned
- Check that DialogueBoxManager is assigned in FarDistanceDialogueController
- Check the Console for error messages

**Dialogue not visible when camera moves?**
- **Use Follow Camera mode** instead of Fixed mode to ensure dialogue is always visible
- Make sure the dialogue position is within the camera's view (if using Fixed mode)
- Check that the World Space Canvas was created (look in Hierarchy)
- Verify the canvas scale is appropriate (should be 0.01 for 2D)
- Check that Camera Transform is assigned (or Main Camera exists) if using Follow Camera mode

**'P' key doesn't work?**
- Check that the script is enabled
- Check the Console for any errors
- Verify the Change Dialogue Key is set to 'P' in the Inspector

**Dialogue appears too small/large?**
- Adjust the World Space Canvas scale (in the created canvas GameObject)
- Adjust the dialogue box scale in DialogueBoxManager settings

## Notes

- The dialogue uses a World Space Canvas, so it exists in world coordinates
- The dialogue position is fixed in world space, not relative to the player
- Multiple dialogue messages can be set up and cycled through with 'P'
- The dialogue will remain visible as long as it's within the camera's view frustum

