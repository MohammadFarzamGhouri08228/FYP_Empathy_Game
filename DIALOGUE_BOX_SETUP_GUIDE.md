# Dialogue Box System Setup Guide

This guide will walk you through setting up dialogue boxes using your sprite sheet with multiple speech bubble designs.

## Overview

You have a sprite sheet (`speech-bubble-vector-halftone-style-set.png`) containing multiple dialogue box designs. This system allows you to:
- Select different dialogue box sprites from the sheet
- Display text in the dialogue boxes
- Show/hide dialogue boxes dynamically
- Position dialogue boxes in world space or screen space

---

## Step 1: Prepare Your Sprite Sheet

### 1.1 Import and Configure the Sprite Sheet

1. **Locate your sprite sheet**:
   - File: `Assets/Scenes/Maze2/speech-bubble-vector-halftone-style-set.png`
   - If not already imported, drag it into Unity's `Assets` folder

2. **Select the sprite sheet** in the Project window

3. **Configure Import Settings**:
   - In Inspector, set **Texture Type** to `Sprite (2D and UI)`
   - Set **Sprite Mode** to `Multiple` (since you have multiple dialogue boxes)
   - Set **Pixels Per Unit** to match your game (typically `100` for UI elements)
   - Set **Filter Mode** to `Bilinear` (for smooth scaling)
   - Click **Apply**

### 1.2 Slice the Sprite Sheet

1. **Open Sprite Editor**:
   - Click the **Sprite Editor** button in the Inspector
   - A new window will open showing your sprite sheet

2. **Slice the Sprites**:
   - Click **Slice** dropdown → Choose **Grid By Cell Count** or **Grid By Cell Size**
   - **Option A - Grid By Cell Count**: If you know the grid layout (e.g., 3 columns × 3 rows)
     - Set **Column & Row** to match your layout
   - **Option B - Grid By Cell Size**: If you know pixel dimensions
     - Set **Pixel Size** to match each dialogue box size
   - **Option C - Manual**: Click and drag to select each dialogue box individually
     - This gives you the most control

3. **Name Your Sprites** (Important!):
   - After slicing, click on each sprite in the Sprite Editor
   - Rename them descriptively:
     - `speech_bubble_round`
     - `speech_bubble_rectangular`
     - `speech_bubble_spiky`
     - `thought_bubble_cloud`
     - `speech_bubble_explosion`
     - etc.

4. **Set Pivot Points**:
   - For each sprite, set **Pivot** to `Bottom` or `Bottom Left` (typical for dialogue boxes)
   - This ensures the tail/pointer aligns correctly

5. **Click Apply** and close the Sprite Editor

### 1.3 Verify Sliced Sprites

1. In Project window, **expand the sprite sheet asset**
2. You should see individual sprites listed
3. Click each sprite to verify it's correctly sliced

---

## Step 2: Set Up the Dialogue Box System

### 2.1 Create the Dialogue Box Manager Script

1. The `DialogueBoxManager.cs` script has been created for you
2. This script manages sprite selection and dialogue display

### 2.2 Create the Dialogue Box UI Script

1. The `DialogueBoxUI.cs` script has been created for you
2. This script handles individual dialogue box instances

### 2.3 Set Up in Your Scene

#### Option A: World Space Dialogue (Follows Game Objects)

1. **Create Dialogue Box GameObject**:
   - Right-click in Hierarchy → **Create Empty**
   - Name it `DialogueBoxManager`
   - Add Component → `DialogueBoxManager`

2. **Configure DialogueBoxManager**:
   - **Dialogue Box Sprites**: Drag all sliced sprites from your sprite sheet into this array
   - **Default Sprite Index**: Set to `0` (first sprite)
   - **Text Font**: Assign a font (or leave null to use default)
   - **Text Font Size**: Set to `24` or your preferred size
   - **Text Color**: Set to `Black` or your preferred color
   - **Padding**: Set X and Y padding for text (e.g., `20, 20`)

3. **Create Canvas** (if not exists):
   - Right-click Hierarchy → **UI → Canvas**
   - Set **Render Mode** to `World Space` (for world space dialogue)
   - Or `Screen Space - Overlay` (for screen space dialogue)

#### Option B: Screen Space Dialogue (Fixed on Screen)

1. **Create Canvas** (if not exists):
   - Right-click Hierarchy → **UI → Canvas**
   - Set **Render Mode** to `Screen Space - Overlay`

2. **Create Dialogue Box Container**:
   - Right-click Canvas → **Create Empty**
   - Name it `DialogueBoxContainer`
   - Add Component → `DialogueBoxManager`

3. **Configure DialogueBoxManager** (same as Option A)

---

## Step 3: Using the Dialogue System

### 3.1 Basic Usage in Code

```csharp
using UnityEngine;

public class ExampleDialogueUsage : MonoBehaviour
{
    private DialogueBoxManager dialogueManager;
    
    void Start()
    {
        // Find the dialogue manager
        dialogueManager = FindFirstObjectByType<DialogueBoxManager>();
        
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueBoxManager not found!");
            return;
        }
    }
    
    void ShowDialogue()
    {
        // Show dialogue with default sprite
        dialogueManager.ShowDialogue("Hello! This is a test message.");
        
        // Show dialogue with specific sprite index
        dialogueManager.ShowDialogue("This uses sprite index 2!", 2);
        
        // Show dialogue at specific position (world space)
        Vector3 worldPos = new Vector3(0, 2, 0);
        dialogueManager.ShowDialogueAtPosition("Positioned dialogue!", worldPos);
        
        // Show dialogue following a GameObject
        GameObject npc = GameObject.Find("NPC");
        dialogueManager.ShowDialogueFollowing("NPC says hello!", npc.transform, new Vector3(0, 1, 0));
    }
    
    void HideDialogue()
    {
        dialogueManager.HideDialogue();
    }
}
```

### 3.2 Using with NPCs or Game Objects

```csharp
public class NPCInteraction : MonoBehaviour
{
    public DialogueBoxManager dialogueManager;
    public string[] dialogueLines = {
        "Welcome to the maze!",
        "Watch out for bombs!",
        "Good luck!"
    };
    
    private int currentDialogueIndex = 0;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowNextDialogue();
        }
    }
    
    void ShowNextDialogue()
    {
        if (dialogueManager != null && currentDialogueIndex < dialogueLines.Length)
        {
            // Show dialogue above this NPC
            dialogueManager.ShowDialogueFollowing(
                dialogueLines[currentDialogueIndex],
                transform,
                new Vector3(0, 1.5f, 0) // Offset above NPC
            );
            
            currentDialogueIndex++;
        }
    }
}
```

### 3.3 Using Different Sprites for Different Situations

```csharp
public class DialogueVariety : MonoBehaviour
{
    public DialogueBoxManager dialogueManager;
    
    void ShowHappyDialogue()
    {
        // Use round, friendly speech bubble (index 0)
        dialogueManager.ShowDialogue("Great job!", 0);
    }
    
    void ShowAngryDialogue()
    {
        // Use spiky, explosion speech bubble (index 7)
        dialogueManager.ShowDialogue("Watch out!", 7);
    }
    
    void ShowThoughtDialogue()
    {
        // Use cloud thought bubble (index 2)
        dialogueManager.ShowDialogue("Hmm, I wonder...", 2);
    }
}
```

---

## Step 4: Advanced Configuration

### 4.1 Customizing Dialogue Appearance

In the `DialogueBoxManager` component:

- **Text Font**: Change the font style
- **Text Font Size**: Adjust text size
- **Text Color**: Change text color
- **Padding**: Adjust spacing between text and bubble edges
- **Dialogue Box Scale**: Scale the entire dialogue box
- **Auto Hide Duration**: Set to `0` to disable auto-hide, or set a duration in seconds

### 4.2 Positioning Options

- **World Space**: Dialogue follows game objects in 3D space
- **Screen Space**: Dialogue stays fixed on screen
- **Following Offset**: Adjust offset when following a GameObject

### 4.3 Animation Options

The system includes optional fade-in/fade-out animations. You can customize:
- **Fade In Duration**: How long fade-in takes
- **Fade Out Duration**: How long fade-out takes
- Enable/disable animations in the component

---

## Step 5: Testing

1. **Enter Play Mode**
2. **Test Basic Display**:
   - Call `ShowDialogue("Test message")` from a script
   - Verify dialogue box appears with text

3. **Test Sprite Selection**:
   - Try different sprite indices
   - Verify correct sprites are displayed

4. **Test Positioning**:
   - Test world space positioning
   - Test following a GameObject
   - Verify dialogue appears in correct location

5. **Test Hide/Show**:
   - Call `HideDialogue()` to hide
   - Call `ShowDialogue()` again to show

---

## Troubleshooting

### Dialogue Box Doesn't Appear

- **Check Canvas**: Ensure Canvas exists and is active
- **Check Render Mode**: Ensure Canvas render mode matches your needs
- **Check Script**: Verify DialogueBoxManager is attached and configured
- **Check Sprites**: Ensure sprites are assigned in the array

### Text Doesn't Show

- **Check Font**: Ensure font is assigned or default font is available
- **Check Text Color**: Ensure text color is visible (not transparent)
- **Check Padding**: Text might be outside visible area - adjust padding

### Wrong Sprite Displayed

- **Check Sprite Index**: Verify sprite index matches your sprite array
- **Check Sprite Array**: Ensure sprites are in correct order in array
- **Check Sprite Names**: Verify sprite names match your expectations

### Dialogue Box Position Wrong

- **Check Canvas Render Mode**: World Space vs Screen Space affects positioning
- **Check Offset**: Verify offset values are correct
- **Check Transform**: Ensure parent transform is correct

---

## Next Steps

- Create prefabs for reusable dialogue boxes
- Add typing animation for text
- Add sound effects for dialogue appearance
- Create dialogue system with branching conversations
- Integrate with your game's event system

---

## Quick Reference

### Key Methods

- `ShowDialogue(string text)` - Show dialogue with default sprite
- `ShowDialogue(string text, int spriteIndex)` - Show dialogue with specific sprite
- `ShowDialogueAtPosition(string text, Vector3 position)` - Show at world position
- `ShowDialogueFollowing(string text, Transform target, Vector3 offset)` - Follow GameObject
- `HideDialogue()` - Hide current dialogue
- `SetSprite(int index)` - Change sprite without changing text
- `SetText(string text)` - Change text without changing sprite

### Inspector Fields

- **Dialogue Box Sprites**: Array of sprites from sprite sheet
- **Default Sprite Index**: Default sprite to use
- **Text Font**: Font for dialogue text
- **Text Font Size**: Size of text
- **Text Color**: Color of text
- **Padding**: Padding around text
- **Auto Hide Duration**: Auto-hide after seconds (0 = disabled)

