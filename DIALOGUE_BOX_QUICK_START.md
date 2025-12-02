# Dialogue Box System - Quick Start Guide

## 🚀 Quick Setup (10 Minutes)

### Step 1: Prepare Your Sprite Sheet (3 min)

1. **Select your sprite sheet** in Project window:
   - `Assets/Scenes/Maze2/speech-bubble-vector-halftone-style-set.png`

2. **Configure Import Settings**:
   - Inspector → **Texture Type**: `Sprite (2D and UI)`
   - **Sprite Mode**: `Multiple`
   - **Pixels Per Unit**: `100`
   - Click **Apply**

3. **Slice the Sprites**:
   - Click **Sprite Editor** button
   - Click **Slice** → Choose slicing method (Grid By Cell Count/Size or Manual)
   - **Rename each sprite** descriptively (e.g., `speech_bubble_round`, `speech_bubble_spiky`)
   - Set **Pivot** to `Bottom` or `Bottom Left` for each sprite
   - Click **Apply**

### Step 2: Set Up Dialogue System in Scene (3 min)

1. **Create Canvas** (if not exists):
   - Right-click Hierarchy → **UI → Canvas**
   - Set **Render Mode** to `Screen Space - Overlay` (or `World Space` for 3D)

2. **Create Dialogue Manager**:
   - Right-click Canvas → **Create Empty**
   - Name it `DialogueBoxManager`
   - Add Component → `DialogueBoxManager`

3. **Configure DialogueBoxManager**:
   - **Dialogue Box Sprites**: Drag all sliced sprites from sprite sheet into this array
   - **Default Sprite Index**: `0`
   - **Text Font Size**: `24`
   - **Text Color**: `Black`
   - **Padding**: `X: 20, Y: 20`

### Step 3: Test It! (2 min)

1. **Add Example Script** (optional):
   - Select any GameObject
   - Add Component → `DialogueBoxExample`
   - Drag `DialogueBoxManager` into the **Dialogue Manager** field

2. **Enter Play Mode**:
   - Press **Space** to show next dialogue
   - Press **H** to hide dialogue
   - Press **1-9** to show dialogue with specific sprite index

---

## 📋 Implementation Steps Summary

### For Basic Usage:

```csharp
// Get reference to dialogue manager
DialogueBoxManager dialogueManager = FindFirstObjectByType<DialogueBoxManager>();

// Show dialogue with default sprite
dialogueManager.ShowDialogue("Hello!");

// Show dialogue with specific sprite (index 2)
dialogueManager.ShowDialogue("Watch out!", 2);

// Hide dialogue
dialogueManager.HideDialogue();
```

### For Following a GameObject:

```csharp
// Show dialogue above an NPC
dialogueManager.ShowDialogueFollowing(
    "Hello player!",
    npcTransform,
    new Vector3(0, 1.5f, 0) // Offset above NPC
);
```

### For World Position:

```csharp
// Show dialogue at specific world position
dialogueManager.ShowDialogueAtPosition(
    "Message here!",
    new Vector3(0, 2, 0)
);
```

---

## 🎯 Key Features

✅ **Multiple Sprite Support**: Use different dialogue box designs from your sprite sheet  
✅ **Dynamic Text**: Display any text in dialogue boxes  
✅ **Positioning Options**: Screen space, world space, or follow GameObjects  
✅ **Animations**: Optional fade-in/fade-out animations  
✅ **Auto-Hide**: Optional auto-hide after duration  
✅ **Easy Integration**: Simple API for showing/hiding dialogue  

---

## 📝 Quick Reference

### DialogueBoxManager Methods:

- `ShowDialogue(string text)` - Show with default sprite
- `ShowDialogue(string text, int spriteIndex)` - Show with specific sprite
- `ShowDialogueAtPosition(string text, Vector3 position)` - Show at world position
- `ShowDialogueFollowing(string text, Transform target, Vector3 offset)` - Follow GameObject
- `HideDialogue()` - Hide current dialogue
- `SetSprite(int index)` - Change sprite
- `SetText(string text)` - Change text

### Inspector Settings:

- **Dialogue Box Sprites**: Array of sprites from sprite sheet
- **Default Sprite Index**: Default sprite to use (0-based)
- **Text Font**: Font for text (optional)
- **Text Font Size**: Size of text
- **Text Color**: Color of text
- **Padding**: Padding around text (X, Y)
- **Auto Hide Duration**: Auto-hide after seconds (0 = disabled)

---

## 🔧 Troubleshooting

**Dialogue doesn't appear?**
- Check Canvas exists and is active
- Check DialogueBoxManager has sprites assigned
- Check script is attached and configured

**Wrong sprite shown?**
- Verify sprite index is correct (0-based)
- Check sprites are in correct order in array

**Text not visible?**
- Check text color is not transparent
- Adjust padding if text is outside visible area
- Check font is assigned

---

## 📚 Full Documentation

See `DIALOGUE_BOX_SETUP_GUIDE.md` for complete documentation with:
- Detailed setup instructions
- Advanced configuration
- Integration examples
- Troubleshooting guide

---

## 🎮 Example Use Cases

1. **NPC Dialogue**: Show dialogue when player approaches NPC
2. **Tutorial Messages**: Display instructions at specific locations
3. **Story Text**: Show narrative text during gameplay
4. **Warning Messages**: Use spiky bubbles for warnings
5. **Thought Bubbles**: Use cloud bubbles for character thoughts

---

**Ready to use!** Follow the steps above and you'll have dialogue boxes working in minutes! 🎉

