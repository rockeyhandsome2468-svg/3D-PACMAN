using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a 2D maze grid using Recursive Backtracking (DFS).
/// true  = wall cell
/// false = open path cell
/// </summary>
public class MazeGenerator
{
    private int _width;
    private int _height;
    private bool[,] _maze;
    private System.Random _rng = new System.Random();

    private readonly Vector2Int[] _dirs = new Vector2Int[]
    {
        new Vector2Int(2,  0),
        new Vector2Int(-2, 0),
        new Vector2Int(0,  2),
        new Vector2Int(0, -2)
    };

    /// <summary>
    /// Returns a bool[,] maze. true = wall, false = open path.
    /// </summary>
    public bool[,] GenerateMaze(int width, int height)
    {
        _width  = (width  % 2 == 0) ? width  + 1 : width;
        _height = (height % 2 == 0) ? height + 1 : height;

        _maze = new bool[_width, _height];

        // Fill everything as walls
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                _maze[x, y] = true;

        // Carve passages using iterative DFS
        Vector2Int start = new Vector2Int(1, 1);
        _maze[start.x, start.y] = false;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisited = GetUnvisitedNeighbors(current);

            if (unvisited.Count > 0)
            {
                Vector2Int chosen = unvisited[_rng.Next(unvisited.Count)];

                Vector2Int wall = new Vector2Int(
                    current.x + (chosen.x - current.x) / 2,
                    current.y + (chosen.y - current.y) / 2
                );
                _maze[wall.x, wall.y]    = false;
                _maze[chosen.x, chosen.y] = false;

                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }

        // Post-processing: Remove random walls to create multiple paths (loops)
        int extraPaths = (_width * _height) / 10; // Number of walls to remove
        int attempts = 0;
        
        while (extraPaths > 0 && attempts < 1000)
        {
            int x = _rng.Next(1, _width - 1);
            int y = _rng.Next(1, _height - 1);

            if (_maze[x, y])
            {
                // Ensure it's a valid wall separating two paths (horizontal or vertical)
                bool horizPaths = !_maze[x - 1, y] && !_maze[x + 1, y];
                bool vertPaths  = !_maze[x, y - 1] && !_maze[x, y + 1];

                if (horizPaths ^ vertPaths) // Exclusive OR: it separates either horizontally OR vertically
                {
                    _maze[x, y] = false;
                    extraPaths--;
                }
            }
            attempts++;
        }

        return _maze;
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        foreach (Vector2Int dir in _dirs)
        {
            int nx = cell.x + dir.x;
            int ny = cell.y + dir.y;

            if (nx >= 1 && nx < _width - 1 && ny >= 1 && ny < _height - 1)
                if (_maze[nx, ny])
                    neighbors.Add(new Vector2Int(nx, ny));
        }
        return neighbors;
    }
}