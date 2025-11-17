# Fix: Complete Pitch Darkness (Walls Still Visible)

If your maze walls are still visible even though they're darkened, follow these steps for **complete pitch darkness**:

## Quick Fix (3 Steps)

### 1. Set Global Light 2D to Complete Darkness
1. Select **Global Light 2D** in Hierarchy
2. In Inspector, set:
   - **Intensity** = **0** (not 0.1 or 0.2 - must be 0!)
   - **Color** = **Black** (RGB: 0, 0, 0) - Click the color picker and set all values to 0

### 2. Disable Ambient Light
1. Go to **Window → Rendering → Lighting** (or **Edit → Render Pipeline → Lighting Settings**)
2. In **Environment** section:
   - **Ambient Mode** = Trilight or Flat
   - **Ambient Sky Color** = Black (0, 0, 0)
   - **Ambient Equator Color** = Black (0, 0, 0)
   - **Ambient Ground Color** = Black (0, 0, 0)
   - **Ambient Intensity** = **0**

**Alternative method:**
- **Edit → Project Settings → Graphics**
- Find **Lighting** section
- Set all ambient colors to black and intensity to 0

### 3. Set Camera Background to Black
1. Select **Main Camera**
2. In **Camera** component:
   - **Background Type** = Solid Color
   - **Background** = Black (0, 0, 0)

## Verify It's Working

After making these changes:
1. Enter **Play Mode**
2. Areas outside the player's light should be **completely black** (invisible)
3. Only the area within the player's light radius should be visible

## Common Mistakes

❌ **Wrong:** Global Light Intensity = 0.1 or 0.2  
✅ **Correct:** Global Light Intensity = 0

❌ **Wrong:** Global Light Color = Dark gray (20, 20, 20)  
✅ **Correct:** Global Light Color = Pure black (0, 0, 0)

❌ **Wrong:** Ambient Light Intensity = 1 (default)  
✅ **Correct:** Ambient Light Intensity = 0

## If Walls Are Still Visible

1. **Check sprite materials**: Sprites must use **Sprite-Lit-Default** material
   - Select wall sprites
   - In **SpriteRenderer → Material**, ensure it's set to a lit material
   - If using unlit material, lights won't affect them properly

2. **Check Light 2D Target Sorting Layers**:
   - Select **Player's Point Light 2D**
   - Ensure **Target Sorting Layers** includes the layer your walls are on

3. **Verify Global Light is active**:
   - Check that **Global Light 2D** GameObject is enabled (checkbox checked)

---

**Summary:** For complete pitch darkness, you need:
- Global Light 2D: Intensity = 0, Color = Black
- Ambient Light: Intensity = 0, Colors = Black  
- Camera Background: Black
- Sprites: Use lit materials

