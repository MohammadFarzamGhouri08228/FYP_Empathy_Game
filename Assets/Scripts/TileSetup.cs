using UnityEngine;

/// <summary>
/// Script to set up a tile game object to use the tile.svg sprite
/// Attach this to a game object and assign the tile sprite in the inspector
/// </summary>
public class TileSetup : MonoBehaviour
{
    [Header("Tile Sprite Configuration")]
    [Tooltip("The sprite from tile.svg to use for this tile")]
    public Sprite tileSprite;
    
    [Tooltip("Scale of the tile (default: 1 unit = 1 meter)")]
    public Vector3 tileScale = Vector3.one;
    
    [Tooltip("Rotation of the tile (for 3D, typically rotate 90 degrees on X axis to lay flat)")]
    public Vector3 tileRotation = new Vector3(90, 0, 0);

    void Start()
    {
        SetupTile();
    }

    /// <summary>
    /// Sets up the tile game object with the sprite
    /// </summary>
    public void SetupTile()
    {
        // Try to get or add MeshRenderer (for 3D Quad)
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            // If no MeshRenderer, try SpriteRenderer (for 2D)
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            if (tileSprite != null)
            {
                spriteRenderer.sprite = tileSprite;
            }
        }
        else
        {
            // For 3D Quad, create a material with the sprite texture
            if (tileSprite != null)
            {
                Material tileMaterial = new Material(Shader.Find("Standard"));
                tileMaterial.mainTexture = tileSprite.texture;
                meshRenderer.material = tileMaterial;
            }
        }

        // Apply scale and rotation
        transform.localScale = tileScale;
        transform.localRotation = Quaternion.Euler(tileRotation);
    }

    /// <summary>
    /// Loads the tile sprite from the tile.svg file
    /// Note: Unity may need the SVG to be converted to PNG first
    /// </summary>
    [ContextMenu("Load Tile Sprite from Resources")]
    public void LoadTileSprite()
    {
        // Try to load from Resources folder
        Sprite loadedSprite = Resources.Load<Sprite>("tile");
        if (loadedSprite != null)
        {
            tileSprite = loadedSprite;
            SetupTile();
        }
        else
        {
            // Try to load directly from the asset path
            Object[] sprites = Resources.LoadAll("Scenes/Maze2/tile");
            if (sprites.Length > 0 && sprites[0] is Sprite)
            {
                tileSprite = sprites[0] as Sprite;
                SetupTile();
            }
            else
            {
                Debug.LogWarning("Could not load tile sprite. Make sure tile.svg is imported as a sprite in Unity, or convert it to PNG format.");
            }
        }
    }
}



