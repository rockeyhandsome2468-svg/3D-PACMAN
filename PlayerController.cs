using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2Int gridPos = new Vector2Int(1, 1);
    private Vector3 targetWorldPos;
    private bool isMoving = false;
    private MazeBuilder maze;

    void Start()
    {
        maze = Object.FindFirstObjectByType<MazeBuilder>();
        
        // Snap directly to the intended starting position for the player
        transform.position = new Vector3(gridPos.x, 0.5f, gridPos.y);
        targetWorldPos = transform.position;
    }

    public void ResetPosition()
    {
        gridPos = new Vector2Int(1, 1);
        transform.position = new Vector3(gridPos.x, 0.5f, gridPos.y);
        targetWorldPos = transform.position;
        isMoving = false;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f) isMoving = false;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                // Get camera's forward and right vectors, flattened to the XZ plane
                Vector3 forward = cam.transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = cam.transform.right;
                right.y = 0;
                right.Normalize();

                // Map input to camera's orientation
                Vector3 desiredDir = right * h + forward * v;

                // Snap to the closest major grid axis
                if (Mathf.Abs(desiredDir.x) > Mathf.Abs(desiredDir.z)) Move(new Vector2Int(desiredDir.x > 0 ? 1 : -1, 0));
                else Move(new Vector2Int(0, desiredDir.z > 0 ? 1 : -1));
            }
            else
            {
                if (h != 0) Move(new Vector2Int((int)h, 0));
                else if (v != 0) Move(new Vector2Int(0, (int)v));
            }
        }
    }

    void Move(Vector2Int dir)
    {
        Vector2Int nextPos = gridPos + dir;
        if (maze != null && !maze.IsWall(nextPos.x, nextPos.y))
        {
            gridPos = nextPos;
            targetWorldPos = new Vector3(gridPos.x, 0.5f, gridPos.y);
            isMoving = true;
            
            // Win the game if the player steps on the Red End Pad
            if (gridPos.x == maze.width - 2 && gridPos.y == maze.height - 2)
            {
                if (GameManager.Instance != null) GameManager.Instance.WinGame();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoinCollected();
            }
        }
    }
}