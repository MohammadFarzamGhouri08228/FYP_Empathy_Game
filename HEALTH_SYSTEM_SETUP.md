# Health System Setup Guide - Using Sliced Tilemap Tiles

This guide will walk you through setting up the health system with heart animations using your sliced tilemap tiles.

## Step 1: Prepare Your Heart Tile Sprites

### Option A: If you have a single image with all heart states:
1. **Import your heart tilesheet image** into Unity:
   - Drag your heart tilesheet image (PNG) into the `Assets` folder
   - Select the imported image in the Project window

2. **Configure Sprite Import Settings**:
   - In the Inspector, set **Texture Type** to `Sprite (2D and UI)`
   - Set **Sprite Mode** to `Multiple` (if you have multiple hearts in one image)
   - Set **Pixels Per Unit** to match your game (typically `16`, `32`, or `100`)
   - Click **Apply**

3. **Slice the Spritesheet**:
   - Click **Sprite Editor** button
   - In Sprite Editor window:
     - Click **Slice** dropdown → Choose **Grid By Cell Count** or **Grid By Cell Size**
     - If using Grid By Cell Count: Set **Column & Row** to match your layout (e.g., 3 columns for 3 heart states)
     - If using Grid By Cell Size: Set the pixel dimensions of each tile
   - Click **Apply** and close the Sprite Editor

4. **Verify Sliced Sprites**:
   - In Project window, expand the image asset
   - You should see individual sprites: `heart_full`, `heart_half`, `heart_empty` (or similar names)
   - If names are generic (like `heart_0`, `heart_1`, `heart_2`), rename them for clarity

### Option B: If you have separate images for each heart state:
1. Import each heart image separately:
   - `heart_full.png`
   - `heart_half.png`
   - `heart_empty.png`

2. For each image:
   - Set **Texture Type** to `Sprite (2D and UI)`
   - Set **Sprite Mode** to `Single`
   - Set **Pixels Per Unit** to match your game
   - Click **Apply**

## Step 2: Organize Your Heart Sprites

1. **Create a folder** (optional but recommended):
   - Right-click in Project window → Create → Folder
   - Name it `UI/Hearts` or `Sprites/Hearts`
   - Move your heart sprites into this folder

## Step 3: Set Up the Health System in Your Scene

### 3.1 Add HealthSystem to Player:
1. Select your **Player GameObject** in the Hierarchy
2. In Inspector, click **Add Component**
3. Search for and add `HealthSystem`
4. Set **Max Health** to `3` (or your desired starting health)

### 3.2 Create HealthUI GameObject:
1. In Hierarchy, right-click → **Create Empty**
2. Name it `HealthUI`
3. Add Component → `HealthUI`

### 3.3 Configure HealthUI Component:
1. Select the **HealthUI** GameObject
2. In Inspector, find the **HealthUI** component
3. **Assign Heart Sprites**:
   - Drag your **Full Heart Sprite** into `Full Heart Sprite` field
   - Drag your **Half Heart Sprite** into `Half Heart Sprite` field (optional, for future use)
   - Drag your **Empty Heart Sprite** into `Empty Heart Sprite` field

4. **Adjust Display Settings** (optional):
   - **Heart Spacing**: Distance between hearts (default: X=50, Y=0)
   - **Heart Size**: Size of each heart UI element (default: 50x50)
   - **Start Position**: Where hearts appear on screen (default: top-left at X=50, Y=-50)

5. **Animation Settings** (optional):
   - **Animation Duration**: How long the animation takes (default: 0.3 seconds)
   - **Scale Curve**: Edit the animation curve for custom animation feel

## Step 4: Set Up Obstacles

1. Select each **obstacle tilemap object** in your scene
2. Add Component → `ObstacleInteraction`
3. Configure settings:
   - **Damage Amount**: How much health to lose (default: 1)
   - **Destroy On Contact**: Whether obstacle disappears after hit (optional)
   - **Invincibility Duration**: Time before player can take damage again (default: 1 second)

4. **Ensure Colliders are Set Up**:
   - Obstacles need a **Collider2D** component
   - For trigger-based detection: Enable **Is Trigger** on the collider
   - For collision-based detection: Keep **Is Trigger** disabled

## Step 5: Tag Your Player (Important!)

1. Select your **Player GameObject**
2. In Inspector, find the **Tag** dropdown (top of Inspector)
3. If "Player" tag doesn't exist:
   - Click **Add Tag...**
   - Click **+** to add new tag
   - Name it `Player`
   - Click **Save**
4. Select **Player** tag from the dropdown

## Step 6: Test the System

1. **Enter Play Mode**
2. You should see **3 hearts** in the top-left corner
3. Move player into an obstacle
4. Watch hearts decrease with animation:
   - Full heart → Empty heart (with scale animation)
   - Hearts animate when health changes

## Troubleshooting

### Hearts don't appear:
- Check that heart sprites are assigned in HealthUI component
- Verify Canvas exists (HealthUI creates one automatically)
- Check Console for error messages

### Hearts don't animate:
- Verify Animation Duration is > 0
- Check that heart sprites are properly assigned

### Health doesn't decrease:
- Ensure Player has `HealthSystem` component
- Ensure obstacles have `ObstacleInteraction` component
- Check that colliders are set up correctly
- Verify Player has "Player" tag or `PlayerController2` component

### Hearts look wrong:
- Adjust **Heart Size** in HealthUI component
- Adjust **Pixels Per Unit** on your sprite import settings
- Check that sprites are imported as `Sprite (2D and UI)` type

## Advanced: Custom Heart Layout

If you want hearts in a different position:
1. Select **HealthUI** GameObject
2. In Inspector, adjust:
   - **Start Position**: X and Y coordinates
   - **Heart Spacing**: Horizontal (X) and Vertical (Y) spacing
   - **Heart Size**: Width and Height of each heart

## Notes

- The system automatically creates a Canvas if one doesn't exist
- Hearts are positioned using UI anchors (top-left by default)
- Animation uses scale and optional fade effects
- Half-heart support is built-in for future fractional health systems

