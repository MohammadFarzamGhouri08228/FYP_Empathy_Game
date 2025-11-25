# How to Set Camera Renderer to 2D

If you can't find the "Renderer" option in Universal Additional Camera Data, here are the exact steps:

## Method 1: Check Camera Component's Rendering Section

1. Select **Main Camera**
2. In the **Camera** component, find the **"Rendering"** section
3. **Click the arrow** to expand it (it might be collapsed)
4. Look for a **"Renderer"** or **"Renderer Index"** dropdown
5. Set it to **"0"** (which corresponds to Renderer2D if it's the first renderer in your URP asset)

## Method 2: Universal Additional Camera Data

1. Select **Main Camera**
2. Find **Universal Additional Camera Data** component
3. Look for a **"Renderer"** dropdown field
4. If you see a dropdown with numbers (0, 1, 2, etc.):
   - Select **"0"** (first renderer, which should be Renderer2D)
5. If you see a dropdown with renderer names:
   - Select **"Renderer2D"**

## Method 3: It May Work Automatically!

**Good news:** Your URP asset (`UniversalRP.asset`) already has Renderer2D configured as the default renderer (index 0). This means:

- **If the renderer field shows "-1" or "Default"**, it will automatically use Renderer2D
- **You might not need to change anything!**

## How to Verify It's Working

1. **Skip the renderer setting for now**
2. Proceed to add the lights (Global Light 2D and Point Light 2D)
3. Make sure sprites use **Sprite-Lit-Default** material
4. Enter Play Mode
5. **If lights work**, the renderer is already set correctly!

## If Lights Still Don't Work

If lights don't work after setting up materials and lights, then explicitly set the renderer:

1. Go to **Edit → Project Settings → Graphics**
2. Verify **Scriptable Render Pipeline Settings** = `UniversalRP.asset`
3. Select **UniversalRP.asset** in Project window
4. In Inspector, check **Renderer List** - Renderer2D should be at index 0
5. In **Main Camera → Universal Additional Camera Data**, set **Renderer Index** to **0**

---

**TL;DR:** Try the lights first. If they work, you're done! If not, set Renderer Index to 0 in Universal Additional Camera Data.

