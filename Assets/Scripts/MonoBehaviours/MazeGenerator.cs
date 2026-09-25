using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public int width = 15;
    public int height = 15;
    public float wallSize = 2f; // Ensure this matches your prefab's physical width

    private bool[,] visited;

    void Start()
    {
        visited = new bool[width, height];
        // Start digging the maze from the bottom-left corner
        GenerateMaze(0, 0); 
    }

    void GenerateMaze(int x, int y)
    {
        visited[x, y] = true;

        // Define the 4 possible directions (Up, Down, Right, Left)
        Vector2Int[] directions = { 
            new Vector2Int(0, 1), 
            new Vector2Int(0, -1), 
            new Vector2Int(1, 0), 
            new Vector2Int(-1, 0) 
        };

        // Shuffle directions to randomize the maze paths
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int temp = directions[i];
            int randomIndex = Random.Range(i, directions.Length);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }

        // Try to move in each direction
        foreach (Vector2Int dir in directions)
        {
            int nextX = x + dir.x;
            int nextY = y + dir.y;

            // Check if the next cell is inside the grid and hasn't been visited
            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height && !visited[nextX, nextY])
            {
                // We are moving to a new cell! 
                // We skip building a wall between (x,y) and (nextX, nextY) to leave a path open.
                
                // Recursively dig from the new cell
                GenerateMaze(nextX, nextY);
            }
            else
            {
                // If it's a dead end or already visited, build a wall to block it
                BuildWall(x, y, dir);
            }
        }
    }

    void BuildWall(int x, int y, Vector2Int dir)
    {
        // Calculate the physical position of the wall between grid cells
        Vector3 position = new Vector3(x * wallSize, 0, y * wallSize);
        position.x += dir.x * (wallSize / 2f);
        position.z += dir.y * (wallSize / 2f);

        // Rotate the wall so it faces the correct direction
        Quaternion rotation = Quaternion.identity;
        if (dir.y != 0) // Moving Up or Down (Horizontal wall)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }

        // Spawn the wall and parent it to the MazeManager to keep the Hierarchy clean
        Instantiate(wallPrefab, position, rotation, transform);
    }
}