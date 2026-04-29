using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    public int width = 15, height = 15;
    public GameObject playerPrefab, enemyPrefab, coinPrefab;
    
    public bool[,] grid;
    private Material wallMat, groundMat, coinMat, startMat, endMat;

    void Start()
    {
        // Load dynamic size from PlayerPrefs (defaults to 15x15)
        width = PlayerPrefs.GetInt("MazeWidth", 15);
        height = PlayerPrefs.GetInt("MazeHeight", 15);

        // Setup URP-compatible materials so things aren't pink
        string urpShader = "Universal Render Pipeline/Lit";
        wallMat = new Material(Shader.Find(urpShader)) { color = new Color(0.75f, 0.8f, 0.85f) }; // Pastel bluish-gray
        groundMat = new Material(Shader.Find(urpShader)) { color = new Color(0.95f, 0.95f, 0.9f) }; // Soft warm white
        coinMat = new Material(Shader.Find(urpShader)) { color = new Color(1f, 0.9f, 0.5f) }; // Pastel yellow
        startMat = new Material(Shader.Find(urpShader)) { color = new Color(0.6f, 0.9f, 0.6f) }; // Pastel green
        endMat = new Material(Shader.Find(urpShader)) { color = new Color(0.9f, 0.5f, 0.5f) }; // Pastel red

        // Set smoothness to 0 to create a flat, matte appearance
        wallMat.SetFloat("_Smoothness", 0f);
        groundMat.SetFloat("_Smoothness", 0f);
        coinMat.SetFloat("_Smoothness", 0f);
        startMat.SetFloat("_Smoothness", 0f);
        endMat.SetFloat("_Smoothness", 0f);

        MazeGenerator gen = new MazeGenerator();
        grid = gen.GenerateMaze(width, height);
        BuildPhysicalMaze();
    }

    void BuildPhysicalMaze()
    {
        // Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = new Vector3(width / 2f, 0, height / 2f);
        ground.transform.localScale = new Vector3(width / 10f, 1, height / 10f);
        ground.GetComponent<Renderer>().material = groundMat;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x, 0, z);
                if (grid[x, z])
                {
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.position = pos + Vector3.up; 
                    wall.transform.localScale = new Vector3(1, 2, 1);
                    wall.GetComponent<Renderer>().material = wallMat;
                }
                else
                {
                    if (x == 1 && z == 1) 
                    {
                        SpawnUnit(playerPrefab, pos, "Player");
                        CreatePad(pos, startMat, "StartPad");
                    }
                    else if (x == width - 2 && z == height - 2) 
                    {
                        SpawnUnit(enemyPrefab, pos, "Enemy");
                        CreatePad(pos, endMat, "EndPad");
                    }
                    else {
                        GameObject coin = Instantiate(coinPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
                        coin.GetComponent<Renderer>().material = coinMat;
                        
                        // Flatten the scale to look like a disc and attach the spinner script
                        coin.transform.localScale = new Vector3(0.5f, 0.5f, 0.1f);
                        if (coin.GetComponent<CoinSpinner>() == null) coin.AddComponent<CoinSpinner>();

                    }
                }
            }
        }
    }

    void CreatePad(Vector3 pos, Material mat, string padName)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = padName;
        pad.transform.position = pos + Vector3.up * 0.05f; // Slightly above the ground
        pad.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f); // Flat tile shape
        Destroy(pad.GetComponent<Collider>()); // Prevent collision snags
        pad.GetComponent<Renderer>().material = mat;
    }

    void SpawnUnit(GameObject prefab, Vector3 pos, string tag)
    {
        if (prefab == null) return;
        GameObject unit = Instantiate(prefab, pos + Vector3.up * 0.5f, Quaternion.identity);
        unit.tag = tag;
        
        var renderer = unit.GetComponent<Renderer>();
        if(renderer != null) {
            Material baseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            baseMat.color = (tag == "Player") ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.6f, 0.6f); // Pastel Blue/Red
            baseMat.SetFloat("_Smoothness", 0f);
            
            // Add the outline as a secondary material
            Material outlineMat = new Material(Shader.Find("Custom/ToonOutline"));
            renderer.materials = new Material[] { baseMat, outlineMat };
        }
    }

    public bool IsWall(int x, int z) 
    {
        // Safety check to prevent IndexOutOfRange crashes!
        if (x < 0 || x >= width || z < 0 || z >= height) 
        {
            return true; 
        }
        if (grid == null) return true;
        return grid[x, z];
    }
}