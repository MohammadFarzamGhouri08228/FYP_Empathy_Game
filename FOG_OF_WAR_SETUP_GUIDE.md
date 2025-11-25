# Fog of War / Flashlight Effect Setup Guide

This guide will help you implement a flashlight/fog-of-war effect for your top-down maze game using URP 2D lights.

---

## Part 1: Verify URP 2D Setup

Your project already has URP configured! Let's verify and complete the setup:

### Step 1.1: Verify URP Package
1. Open **Window → Package Manager**
2. Make sure **Universal RP** package is installed (should show in "In Project")
3. If not installed: Click **+ → Add package by name** → Enter `com.unity.render-pipelines.universal`

### Step 1.2: Verify 2D Renderer Asset
1. Check that `Assets/Settings/Renderer2D.asset` exists (✅ it does)
2. Check that `Assets/Settings/UniversalRP.asset` exists (✅ it does)

### Step 1.3: Assign Renderer to Graphics Settings
1. Go to **Edit → Project Settings → Graphics**
2. Under **Scriptable Render Pipeline Settings**, drag `Assets/Settings/UniversalRP.asset` into the field
3. Go to **Edit → Project Settings → Quality**
4. For each quality level, ensure **Rendering → Render Pipeline Asset** is set to `UniversalRP.asset`

### Step 1.4: Configure Main Camera for 2D Lights
1. Select your **Main Camera** in the scene
2. In the Inspector, find the **Camera** component
3. Set **Render Type** to **Overlay** (if using multiple cameras) OR keep as **Base**
4. Ensure **Projection** is set to **Orthographic** (for 2D)
5. Add a **Universal Additional Camera Data** component if not present:
   - Click **Add Component → Rendering → Universal Additional Camera Data**

6. **Finding the Renderer Setting:**
   - In **Universal Additional Camera Data**, look for a **"Renderer"** dropdown field
   - If you don't see it, try:
     - **Expand the "Rendering" section** in the Camera component (click the arrow next to "Rendering")
     - Look for **"Renderer"** or **"Renderer Index"** dropdown
     - The dropdown should show options like "0" or "Renderer2D"
   - **Alternative:** If the URP asset has Renderer2D set as the default (which yours does), the camera will use it automatically. You can skip this step if lights work without it.

**Note:** In some URP versions, the renderer is set automatically from the URP asset's default renderer. If you can't find the field, proceed to the next steps - it may work automatically.

---

## Part 2: Set Up Sprite Materials for Lighting

**CRITICAL:** Sprites must use a **lit material** to be affected by 2D lights!

### Step 2.1: Check Current Sprite Materials
1. Select a few sprites in your maze (walls, floor, player)
2. Check their **SpriteRenderer** component
3. Look at the **Material** field

### Step 2.2: Assign Lit Material to Sprites
You have two options:

**Option A: Use Default URP 2D Lit Material**
1. In Project window, go to `Assets/Settings/`
2. The Renderer2D asset has default materials configured
3. For each sprite that should be lit:
   - Select the sprite GameObject
   - In **SpriteRenderer → Material**, click the circle icon
   - Search for "Sprite-Lit-Default" or "2D Sprite-Lit"
   - Select it

**Option B: Create Custom Lit Material**
1. Right-click in Project → **Create → Material**
2. Name it "MazeLitMaterial"
3. In the material Inspector:
   - **Shader** = `Universal Render Pipeline/2D/Sprite-Lit-Default`
4. Assign this material to all maze sprites (walls, floor, etc.)

### Step 2.3: Set Sorting Layers (Important!)
1. Select your **maze sprites** (walls, floor)
2. In **SpriteRenderer**, set:
   - **Sorting Layer** = Create/select a layer like "Maze" or "Default"
   - **Order in Layer** = 0 (or appropriate value)

3. Select your **player sprite**
4. Set:
   - **Sorting Layer** = Same as maze (or a layer above)
   - **Order in Layer** = 1 (so player renders above floor)

---

## Part 3: Create the Lighting Setup

### Step 3.1: Add Global Light 2D (Complete Darkness)
1. In Hierarchy, right-click → **Light → 2D → Global Light 2D**
2. Name it "GlobalDarkLight"
3. In Inspector, configure:
   - **Light Type** = Global
   - **Light Color** = **Pure Black** (RGB: 0, 0, 0) - This is critical for pitch darkness!
   - **Intensity** = **0** (or 0.01 for absolute minimum) - This makes everything completely dark
   - **Light Order** = 0 (renders first, darkens everything)

**Important:** For complete pitch darkness, set Intensity to 0 and Color to pure black (0, 0, 0).

### Step 3.2: Add Point Light 2D to Player (Flashlight)
1. Select your **Player GameObject** in Hierarchy
2. Right-click on Player → **Light → 2D → Point Light 2D**
   - OR: Click **Add Component → Rendering → Light 2D (URP)**
3. Name the light "PlayerFlashlight" (or it will be a child of player)
4. In Inspector, configure the **Light 2D** component:
   - **Light Type** = Point
   - **Light Color** = White or warm white (e.g., RGB: 255, 240, 200)
   - **Intensity** = 1.0 to 2.0 (brightness of the light)
   - **Falloff Intensity** = 1.0 (smooth falloff)
   - **Point Light Inner Radius** = 0 (sharp center)
   - **Point Light Outer Radius** = 5 (adjustable - visible area)
   - **Light Order** = 1 (renders after global light, brightens area)
   - **Target Sorting Layers** = Check the layers your maze sprites are on (e.g., "Default", "Maze")

### Step 3.3: Disable Ambient Light (Critical for Pitch Darkness)
1. Go to **Window → Rendering → Lighting** (or **Edit → Render Pipeline → Lighting Settings**)
2. In the **Environment** section:
   - Set **Ambient Mode** = **Trilight** or **Flat**
   - Set **Ambient Sky Color** = **Black** (RGB: 0, 0, 0)
   - Set **Ambient Equator Color** = **Black** (RGB: 0, 0, 0)
   - Set **Ambient Ground Color** = **Black** (RGB: 0, 0, 0)
   - Set **Ambient Intensity** = **0**
3. **Alternative:** Go to **Edit → Project Settings → Graphics**
   - Find **Lighting** section
   - Set ambient colors to black and intensity to 0

### Step 3.4: Set Camera Background to Black
1. Select **Main Camera**
2. In **Camera** component, find **Background Type**
3. Set it to **Solid Color**
4. Set **Background** color to **Black** (RGB: 0, 0, 0)

### Step 3.5: Configure Light Layers (Optional but Recommended)
1. In **Light 2D** component, find **Light Layers**
2. Create/assign layers:
   - **Maze Layer**: For walls, floor
   - **Player Layer**: For player sprite
3. Set the light's **Light Layers** to affect "Maze Layer"
4. Set your sprite **SpriteRenderer → Rendering Layer Mask** to match

---

## Part 4: Add PlayerLightController Script

### Step 4.1: Attach Script to Player
1. Select your **Player GameObject**
2. Click **Add Component**
3. Search for "PlayerLightController"
4. Add it

### Step 4.2: Configure Script in Inspector
1. With Player selected, find **PlayerLightController** component
2. Set fields:
   - **Min Radius** = 3 (tight mode radius)
   - **Max Radius** = 8 (exploration mode radius)
   - **Current Radius** = 5 (starting radius)
   - **Adjust Speed** = 2 (how fast radius changes)
   - **Player Light** = Drag the **Point Light 2D** component here (or leave null to auto-detect)

### Step 4.3: Test the Script
1. Enter Play Mode
2. The light should follow the player
3. You can call `SetExplorationMode()` or `SetTightMode()` from other scripts:
   ```csharp
   GetComponent<PlayerLightController>().SetExplorationMode();
   ```

---

## Part 5: Fine-Tuning

### Adjust Light Intensity
- **Global Light**: Set to **0** for complete darkness, or **0.01** for absolute minimum
- **Global Light Color**: Must be **pure black (0, 0, 0)** for pitch darkness
- **Player Light**: Higher intensity = brighter flashlight (try 1.0-2.0)
- **Ambient Light**: Must be **disabled** (intensity 0, colors black) for complete darkness

### Adjust Light Radius
- In **PlayerLightController**, adjust **Min Radius** and **Max Radius**
- Or directly in **Light 2D → Point Light Outer Radius**

### Adjust Falloff
- **Falloff Intensity** = 1.0 gives smooth edges
- Lower values = sharper edges

### Performance Tips
- Limit the number of 2D lights in scene
- Use **Light Layers** to control which sprites are affected
- Consider using **Light Culling** if you have many lights

---

## Common Pitfalls & Solutions

### ❌ Problem: Sprites are not lit / lights have no effect
**Solution:**
- Sprites must use a **lit material** (Sprite-Lit-Default shader)
- Check **SpriteRenderer → Material** is set to a lit material
- Verify **Light 2D → Target Sorting Layers** includes your sprite's sorting layer

### ❌ Problem: Light is too bright/dark
**Solution:**
- Adjust **Global Light → Intensity** (set to 0 for complete darkness)
- Adjust **Global Light → Color** (must be pure black: 0, 0, 0)
- **Disable Ambient Light** (set intensity to 0, colors to black)
- Adjust **Player Light → Intensity** (higher = brighter)
- Adjust **Player Light → Point Light Outer Radius** (larger = bigger area)

### ❌ Problem: Walls still visible in darkness (not pitch black)
**Solution:**
- **Global Light 2D → Intensity** must be **0** (not 0.1 or 0.2)
- **Global Light 2D → Color** must be **pure black (0, 0, 0)**
- **Disable Ambient Light**: Window → Rendering → Lighting → Set Ambient Intensity to 0
- **Camera Background** should be black
- Ensure sprites use **Sprite-Lit-Default** material (unlit materials won't respond to lights properly)

### ❌ Problem: Light doesn't follow player
**Solution:**
- Ensure **Point Light 2D** is a **child** of the Player GameObject
- OR ensure the light's Transform position matches player position

### ❌ Problem: Wrong sprites are lit
**Solution:**
- Check **Light 2D → Target Sorting Layers** matches sprite sorting layers
- Use **Light Layers** to control which sprites are affected

### ❌ Problem: Performance issues
**Solution:**
- Reduce number of lights
- Use **Light Culling** in Light 2D component
- Limit **Max Light Render Texture Count** in Renderer2D asset

---

## Alternative: Sprite Mask Solution (If URP Not Available)

If you cannot use URP 2D lights, here's an alternative:

### Setup Steps:
1. Create a **Canvas** (UI → Canvas) or use a world-space sprite
2. Create a **full-screen dark sprite** (solid color, covers entire screen)
3. Add **Sprite Mask** component to player
4. Create a **circular sprite** for the mask
5. Set mask to cut a hole in the dark overlay

**Note:** This is more complex and less flexible than URP lights. The `FogOfWarSpriteMask.cs` script provides a helper, but URP lights are strongly recommended.

---

## Quick Reference Checklist

- [ ] URP package installed
- [ ] UniversalRP.asset assigned in Graphics Settings
- [ ] Camera has Universal Additional Camera Data with Renderer2D
- [ ] Sprites use lit materials (Sprite-Lit-Default)
- [ ] Global Light 2D added (low intensity, darkens scene)
- [ ] Point Light 2D added as child of Player
- [ ] Light 2D Target Sorting Layers configured
- [ ] PlayerLightController script attached and configured
- [ ] Tested in Play Mode

---

## Testing

1. Enter **Play Mode**
2. Move the player around
3. Verify:
   - Scene is mostly dark
   - Area around player is lit
   - Light follows player smoothly
   - Light radius can be adjusted via script

---

**Need Help?** Check Unity's official documentation:
- [URP 2D Lights](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/2D-lights.html)
- [Light 2D Component](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/Light-2D.html)

