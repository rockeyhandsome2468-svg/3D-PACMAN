using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Vector2Int gridPos;
    private MazeBuilder maze;
    private PlayerController player;

    void Start()
    {
        maze = Object.FindFirstObjectByType<MazeBuilder>();
        
        if (maze != null) 
        {
            // Snap directly to the intended starting position for the enemy
            gridPos = new Vector2Int(maze.width - 2, maze.height - 2);
            transform.position = new Vector3(gridPos.x, 0.5f, gridPos.y);
        }
        else 
        {
            gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        }
        
        StartCoroutine(ThinkRoutine());
    }

    public void ResetPosition()
    {
        if (maze != null)
        {
            gridPos = new Vector2Int(maze.width - 2, maze.height - 2);
            transform.position = new Vector3(gridPos.x, 0.5f, gridPos.y);
        }
    }

    IEnumerator ThinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.6f);
            
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null || maze == null || maze.grid == null) continue;

            List<Vector2Int> path = BFS(gridPos, player.gridPos);
            if (path != null && path.Count > 1)
            {
                gridPos = path[1]; 
                Vector3 target = new Vector3(gridPos.x, 0.5f, gridPos.y);
                while (Vector3.Distance(transform.position, target) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }
    }

    List<Vector2Int> BFS(Vector2Int start, Vector2Int target)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            if (current == target) break;

            foreach (Vector2Int next in GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int curr = target;
        while (curr != start)
        {
            if (!cameFrom.ContainsKey(curr)) return null; 
            path.Add(curr);
            curr = cameFrom[curr];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    List<Vector2Int> GetNeighbors(Vector2Int p)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            Vector2Int n = p + d;
            
            if (maze != null && !maze.IsWall(n.x, n.y)) neighbors.Add(n);
        }
        return neighbors;
    }

    private void Update() 
    {
        // Automatically find the new player if the old one was destroyed or hidden
        if (player == null || !player.gameObject.activeInHierarchy) 
            player = Object.FindFirstObjectByType<PlayerController>();

        if(player != null && Vector3.Distance(transform.position, player.transform.position) < 0.5f)
        {
            if (GameManager.Instance != null) GameManager.Instance.LoseLife();
        }
    }
}