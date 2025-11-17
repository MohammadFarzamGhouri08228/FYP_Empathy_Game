# Fog of War - Quick Start Guide

## 🚀 Quick Setup (5 Minutes)

### 1. Verify URP Setup (1 min)
- **Edit → Project Settings → Graphics**: Ensure `UniversalRP.asset` is assigned
- **Edit → Project Settings → Quality**: Ensure `UniversalRP.asset` is assigned to all quality levels

### 2. Set Camera for 2D Lights (1 min)
- Select **Main Camera**
- Add component: **Universal Additional Camera Data**
- Set **Renderer** = `Renderer2D`

### 3. Make Sprites Lit (2 min)
- Select all maze sprites (walls, floor)
- In **SpriteRenderer → Material**: Change to **Sprite-Lit-Default** material
  - Click circle icon → Search "Sprite-Lit" → Select

### 4. Add Lights (1 min)
- **Global Light 2D**: Right-click Hierarchy → **Light → 2D → Global Light 2D**
  - Set **Intensity** = **0** (complete darkness!)
  - Set **Color** = **Black (0, 0, 0)**
- **Disable Ambient Light**: Window → Rendering → Lighting → Set **Ambient Intensity** = 0, Colors = Black
- **Camera Background**: Select Main Camera → Set **Background** = Black
- **Point Light 2D on Player**: Select Player → **Add Component → Light 2D (URP)**
  - Set **Light Type** = Point
  - Set **Point Light Outer Radius** = 5
  - Set **Intensity** = 1.5
  - Check **Target Sorting Layers** (your sprite layers)

### 5. Add Script (30 sec)
- Select Player → **Add Component → PlayerLightController**
- Drag the **Light 2D** component into **Player Light** field (or leave null)

**Done!** Enter Play Mode to test.

---

## 📋 Component Checklist

### On Player GameObject:
- [ ] **PlayerController2** (existing)
- [ ] **Light 2D (URP)** - Point Light
- [ ] **PlayerLightController** script

### In Scene:
- [ ] **Global Light 2D** (darkens scene)
- [ ] **Main Camera** with **Universal Additional Camera Data**

### On Maze Sprites:
- [ ] **SpriteRenderer** with **Sprite-Lit-Default** material

---

## 🎮 Using PlayerLightController

### In Inspector:
- **Min Radius**: Smallest light size (tight mode)
- **Max Radius**: Largest light size (exploration mode)
- **Current Radius**: Target radius (smoothly transitions to this)
- **Adjust Speed**: How fast radius changes

### From Code:
```csharp
PlayerLightController lightController = GetComponent<PlayerLightController>();

// Make light bigger (exploration mode)
lightController.SetExplorationMode();

// Make light smaller (tight mode)
lightController.SetTightMode();

// Set custom radius
lightController.SetRadius(6f);
```

---

## ⚙️ Common Settings

| Setting | Recommended Value | What It Does |
|---------|------------------|--------------|
| Global Light Intensity | **0** | Complete darkness (must be 0 for pitch black) |
| Global Light Color | **Black (0,0,0)** | Pure black for complete darkness |
| Ambient Light Intensity | **0** | Disable ambient light for pitch darkness |
| Player Light Intensity | 1.0 - 2.0 | Brightness of flashlight |
| Player Light Outer Radius | 3 - 8 | Size of lit area |
| Falloff Intensity | 1.0 | Smooth edges (lower = sharper) |

---

## 🐛 Troubleshooting

**Lights not working?**
→ Check sprites use **Sprite-Lit-Default** material

**Scene too dark/bright?**
→ Adjust **Global Light Intensity** (0.1-0.3) and **Player Light Intensity** (1.0-2.0)

**Light not following player?**
→ Ensure **Light 2D** is a **child** of Player GameObject

**Wrong sprites lit?**
→ Check **Light 2D → Target Sorting Layers** matches sprite sorting layers

---

For detailed instructions, see `FOG_OF_WAR_SETUP_GUIDE.md`

