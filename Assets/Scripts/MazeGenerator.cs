using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nsMaze;

public class MazeGenerator : MonoBehaviour
{
    //Reference to the prefabs for the walls, start and end points.
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;

    //Reference to materials
    public Material floorMaterial;
    public Material wallMaterial;
    public Material startMaterial;
    public Material goalMaterial;

    //Reference to the player's FPSController to change its position.
    GameObject player;

    //Maze properties
    public int height, width;
    public float wallHeight, wallSeparation;


    //Maze state generation variables
    //Maze matrix to store all the cells.
    //The Cells store if they have been used and the connection with other cells.
    nsMaze.Cell[,] maze;
    //Stack to keep the track of the past positions for the backtracking.
    Stack<Vector2Int> cellRecord;
    //Counter for the total of used tiles.
    int usedTiles;

    // Helper method to assign material to a GameObject
    void AssignMaterial(GameObject obj, Material material)
    {
        if (obj == null || material == null) return;

        // Try MeshRenderer first (for 3D objects)
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = material;
            return;
        }

        // Try SpriteRenderer (for 2D objects)
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.material = material;
            return;
        }

        // If no renderer on the root, check children
        MeshRenderer[] childMeshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer childRenderer in childMeshRenderers)
        {
            childRenderer.material = material;
        }

        SpriteRenderer[] childSpriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer childRenderer in childSpriteRenderers)
        {
            childRenderer.material = material;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set default values if not set in Inspector
        if (height <= 0) height = 10;
        if (width <= 0) width = 10;
        if (wallHeight <= 0) wallHeight = 2f;
        if (wallSeparation <= 0) wallSeparation = 1f;

        //Validate prefabs
        if (wallPrefab == null)
        {
            Debug.LogError("Wall Prefab is not assigned in the Inspector!");
            return;
        }
        if (floorPrefab == null)
        {
            Debug.LogError("Floor Prefab is not assigned in the Inspector!");
            return;
        }
        if (startPrefab == null)
        {
            Debug.LogError("Start Prefab is not assigned in the Inspector!");
            return;
        }
        if (goalPrefab == null)
        {
            Debug.LogError("Goal Prefab is not assigned in the Inspector!");
            return;
        }

        //Prepare the floor prefab to cover all the space.
        Vector3 floorScale = new Vector3((height * wallSeparation) * 2.3f, 1, (width * wallSeparation) * 2.3f);
        GameObject floor = Instantiate(floorPrefab, new Vector3(height * wallSeparation, -0.5f, width * wallSeparation), Quaternion.identity);
        floor.transform.localScale = floorScale;
        if (floorMaterial != null)
        {
            AssignMaterial(floor, floorMaterial);
        }

        //Prepara the Player GameObject to assign the start position.
        player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("Player GameObject not found. Maze will still generate but player won't be positioned.");
        }

        //Run Maze generation recursive backtracker algorithm.
        GenerateMaze();

        //Create the maze using the wall prefab. 
        createMaze();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenerateMaze()
    {
        //Initialize the maze with new Cell objects.
        maze = new nsMaze.Cell[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                maze[i, j] = new nsMaze.Cell();
            }
        }

        //Initialize the stack of vector2ints for position.
        cellRecord = new Stack<Vector2Int>();

        //Initialize the position of the starting point. It can be on (0,0) or random.
        //Vector2Int position = new Vector2Int(Random.Range(0, height),Random.Range(0, width));
        Vector2Int position = new Vector2Int(0, 0);

        //Initialize the used tiles with 1 after the starting was used.
        usedTiles = 1;
        //Change that position to used.
        maze[position.x, position.y].isUsed = true;

        //Introduce the initial position in the stack.
        cellRecord.Push(position);

        //Instantiate the start prefab with the initial position. 
        GameObject startObj = Instantiate(startPrefab, new Vector3(position.x * 2 * wallSeparation, wallHeight / 2, position.y * 2 * wallSeparation), Quaternion.identity);
        if (startMaterial != null)
        {
            AssignMaterial(startObj, startMaterial);
        }

        //Move the player to the initial position.
        if (player != null)
        {
            player.transform.position = new Vector3(position.x * 2 * wallSeparation, wallHeight / 2, position.y * 2 * wallSeparation);
        }

        //Recursive backtracking starts.
        while (usedTiles < height * width)
        {
            //Check available next positions from the top of the stack. It returns a List of int with the possible movements:
            // 0 = N
            // 1 = E
            // 2 = S
            // 3 = W
            List<int> availableDir = checkAvailable(cellRecord.Peek());

            //Check if there is any possible next direction.
            //If not, pop the position from the stack and check the previous position (e.g. go back).
            if (availableDir.Count != 0)
            {
                //The next position is random between the total possible directions.
                //It takes an index between 0 and the total number of possible directions and use it as index for the List.
                int nextDir = availableDir[Random.Range(0, availableDir.Count)];

                //Evaluate the case of all the possible next directions.
                //NOTICE: The double connection is redundant and never used to create the maze!!
                //It could be reduced to only S and E connections as the maze is created checking said connections.
                switch (nextDir)
                {
                    case 0:
                        //Creates a conection between the current cell and next cell depending on the direction.
                        maze[position.x, position.y].connected[0] = true;
                        maze[position.x - 1, position.y].connected[2] = true;

                        //Moves the position to the next cell depending on the direction.
                        position.x = position.x - 1;

                        break;
                    case 1:
                        //Creates a conection between the current cell and next cell depending on the direction.
                        maze[position.x, position.y].connected[1] = true;
                        maze[position.x, position.y + 1].connected[3] = true;

                        //Moves the position to the next cell depending on the direction.
                        position.y = position.y + 1;

                        break;
                    case 2:
                        //Creates a conection between the current cell and next cell depending on the direction.
                        maze[position.x, position.y].connected[2] = true;
                        maze[position.x + 1, position.y].connected[0] = true;

                        //Moves the position to the next cell depending on the direction.
                        position.x = position.x + 1;

                        break;
                    case 3:
                        //Creates a conection between the current cell and next cell depending on the direction.
                        maze[position.x, position.y].connected[3] = true;
                        maze[position.x, position.y - 1].connected[1] = true;

                        //Moves the position to the next cell depending on the direction.
                        position.y = position.y - 1;

                        break;
                }

                //Change the next position to used.
                maze[position.x, position.y].isUsed = true;

                //Introduce the next position into the stack.
                cellRecord.Push(position);

                //Increment the used tiles counter.
                usedTiles++;
            }
            else
            {
                //If there are no possible next directions, pop from the stack to get the previous position.
                if (cellRecord.Count > 1)
                {
                    cellRecord.Pop();
                    //Update the current position to the new top of the stack.
                    position = cellRecord.Peek();
                }
                else
                {
                    //This shouldn't happen, but break to prevent infinite loop
                    Debug.LogWarning("Maze generation encountered an issue. Breaking loop.");
                    break;
                }
            }
        }
    }

    List<int> checkAvailable(Vector2Int position)
    {
        /*
            Function that checks which neightbour cells are available.
            The directions are:
            0 = N
            1 = E
            2 = S
            3 = W
        */

        //Create a list to store the possible neightbours.
        List<int> availableDir = new List<int>();

        //Create variables to shorten the name.
        int X = position.x;
        int Y = position.y;

        //Each condition checks if there is a neighbor in any direction and if it's used or not.
        if (X > 0 && !maze[X - 1, Y].isUsed)
        {
            availableDir.Add(0);
        }

        if (Y < width - 1 && !maze[X, Y + 1].isUsed)
        {
            availableDir.Add(1);
        }


        if (X < height - 1 && !maze[X + 1, Y].isUsed)
        {
            availableDir.Add(2);
        }


        if (Y > 0 && !maze[X, Y - 1].isUsed)
        {

            availableDir.Add(3);
        }

        return availableDir;
    }

    void createMaze()
    {
        /*
            This function creates the maze from the result of the algorithm and the wall prefabs.
            It applies the wallHeight and wallSeparation to adjust the size of the wall prefabs.
            The method is based on double sizing the maze for space and walls.
            Then checking if there are any South or East connection for each cell.
            If there is a connection there is no need to create a wall.
            The external walls and "diagonal" walls are always created independently from connections. 
       */

        Vector3 wallScale = new Vector3(wallSeparation, wallHeight, wallSeparation);

        //Creates the border of the maze.
        for (int i = 0; i < 2 * height; i++)
        {
            GameObject wall1 = Instantiate(wallPrefab, new Vector3(-wallSeparation, wallHeight / 2, i * wallSeparation), Quaternion.identity);
            wall1.transform.localScale = wallScale;
            if (wallMaterial != null)
            {
                AssignMaterial(wall1, wallMaterial);
            }

            GameObject wall2 = Instantiate(wallPrefab, new Vector3(2 * wallSeparation * width, wallHeight / 2, i * wallSeparation), Quaternion.identity);
            wall2.transform.localScale = wallScale;
            if (wallMaterial != null)
            {
                AssignMaterial(wall2, wallMaterial);
            }
        }

        for (int i = 0; i < 2 * width; i++)
        {
            GameObject wall1 = Instantiate(wallPrefab, new Vector3(i * wallSeparation, wallHeight / 2, -wallSeparation), Quaternion.identity);
            wall1.transform.localScale = wallScale;
            if (wallMaterial != null)
            {
                AssignMaterial(wall1, wallMaterial);
            }

            GameObject wall2 = Instantiate(wallPrefab, new Vector3(i * wallSeparation, wallHeight / 2, 2 * wallSeparation * height), Quaternion.identity);
            wall2.transform.localScale = wallScale;
            if (wallMaterial != null)
            {
                AssignMaterial(wall2, wallMaterial);
            }
        }


        //Creates the inside of the maze.
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject centerWall = Instantiate(wallPrefab, new Vector3(wallSeparation * (j * 2 + 1), wallHeight / 2, wallSeparation * (i * 2 + 1)), Quaternion.identity);
                centerWall.transform.localScale = wallScale;
                if (wallMaterial != null)
                {
                    AssignMaterial(centerWall, wallMaterial);
                }

                if (!maze[i, j].connected[2])
                {
                    GameObject southWall = Instantiate(wallPrefab, new Vector3(j * 2 * wallSeparation, wallHeight / 2, wallSeparation * (i * 2 + 1)), Quaternion.identity);
                    southWall.transform.localScale = wallScale;
                    if (wallMaterial != null)
                    {
                        AssignMaterial(southWall, wallMaterial);
                    }
                }
                if (!maze[i, j].connected[1])
                {
                    GameObject eastWall = Instantiate(wallPrefab, new Vector3(wallSeparation * (j * 2 + 1), wallHeight / 2, i * 2 * wallSeparation), Quaternion.identity);
                    eastWall.transform.localScale = wallScale;
                    if (wallMaterial != null)
                    {
                        AssignMaterial(eastWall, wallMaterial);
                    }
                }
            }
        }

        //Instantiates the goal in a random position inside the maze.
        GameObject goalObj = Instantiate(goalPrefab, new Vector3(Random.Range(0, height) * 2 * wallSeparation, wallHeight / 2, Random.Range(0, width) * 2 * wallSeparation), Quaternion.identity);
        if (goalMaterial != null)
        {
            AssignMaterial(goalObj, goalMaterial);
        }
    }
}
