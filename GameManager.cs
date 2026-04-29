using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("HUD Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel; // Drag GameOverPanel here!
    
    private int coins;
    private int lives = 3;
    private int level = 1;
    private float timer = 120f;
    private bool isGameOver = false;

    // This runs exactly ONCE when you press the Play button in the Editor (or launch the built game).
    // It prevents the game from remembering your level from a previous play session!
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStateOnAppStart()
    {
        PlayerPrefs.DeleteKey("MazeWidth");
        PlayerPrefs.DeleteKey("MazeHeight");
        PlayerPrefs.DeleteKey("CurrentLevel");
    }

    void Awake() => Instance = this;

    void Start()
    {
        level = PlayerPrefs.GetInt("CurrentLevel", 1);

        // 1. Ensure an EventSystem exists so UI clicks can register!
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. Extract panels to a clean Screen Space Canvas!
        // Because the original Canvas was set to "World Space" for the floating HUD,
        // the Win/Lose buttons became trapped in 3D space and unclickable.
        GameObject overlayCanvas = new GameObject("OverlayCanvas");
        Canvas canvas = overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.AddComponent<CanvasScaler>();
        overlayCanvas.AddComponent<GraphicRaycaster>();

        if (winPanel != null) 
        { 
            winPanel.transform.SetParent(overlayCanvas.transform, false); 
            winPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
            winPanel.GetComponent<RectTransform>().localScale = Vector3.one; 
        }
        if (losePanel != null) 
        { 
            losePanel.transform.SetParent(overlayCanvas.transform, false); 
            losePanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
            losePanel.GetComponent<RectTransform>().localScale = Vector3.one; 
        }

        // Automatically setup the panels so text and buttons don't overlap,
        // and automatically hook up the Restart buttons to work!
        SetupPanel(winPanel, true);
        SetupPanel(losePanel, false);
    }

    private void SetupPanel(GameObject panel, bool isWinPanel)
    {
        if (panel == null) return;
        
        // 1. Move the Main Text up so it doesn't overlap the button
        TextMeshProUGUI[] allTexts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in allTexts)
        {
            if (t.GetComponentInParent<Button>() == null) // Make sure it's the main text, not a button's text
            {
                t.rectTransform.anchoredPosition = new Vector2(0, 60);
                break;
            }
        }

        // 2. Find all buttons
        Button[] btns = panel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            Button btn = btns[i];
            
            if (i == 0) // We only keep the VERY FIRST button to prevent any overlaps!
            {
                btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50); // Move button down safely
                
                TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (btnText != null) btnText.text = isWinPanel ? "Next Level" : "Restart";
                
                btn.onClick.RemoveAllListeners();
                if (isWinPanel) btn.onClick.AddListener(NextLevel);
                else btn.onClick.AddListener(Restart);
            }
            else 
            {
                // Remove any extra/duplicate buttons so they NEVER overlap each other
                Destroy(btn.gameObject);
            }
        }
    }

    public void CoinCollected() 
    {
        if (isGameOver) return;
        coins++;
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        
        lives--;
        
        // Find ALL players to ensure no dead bodies or accidental clones are left behind
        PlayerController[] allPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        
        if (allPlayers.Length > 0) 
        {
            PlayerController activePlayer = allPlayers[0];
            CreateExplosion(activePlayer.transform.position); // Spawn particles exactly where they died
            
            if (lives > 0)
            {
                // Create a completely new player object to replace the killed one
                GameObject newPlayer = Instantiate(activePlayer.gameObject);
                newPlayer.name = "Player";
                
                // Move it to the start BEFORE activating to prevent any 1-frame glitches
                newPlayer.GetComponent<PlayerController>().ResetPosition();
                newPlayer.SetActive(true);
            }
            
            // Aggressively remove ALL old players from the screen and memory
            foreach (PlayerController p in allPlayers)
            {
                p.gameObject.SetActive(false); // Hide immediately
                Destroy(p.gameObject); // Remove completely
            }
        }

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void CreateExplosion(Vector3 pos)
    {
        GameObject go = new GameObject("PlayerExplosion");
        go.transform.position = pos;
        
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 8f;
        main.startSize = 0.25f;
        main.startColor = new Color(0.5f, 0.8f, 1f); // Matches the player's pastel blue
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) }); // Burst of 30 particles

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = new Color(0.5f, 0.8f, 1f);
        renderer.material.SetFloat("_Smoothness", 0f); // Make the particles flat and un-shiny

        Destroy(go, 2f); // Clean up the particle GameObject after the animation finishes
    }

    void Update() 
    {
        if (isGameOver) return;
        
        timer -= Time.deltaTime;
        
        // No more broken emojis, just clean text!
        if (coinText != null) 
            coinText.text = $"Coins: {coins}";
            
        if (timerText != null) 
        {
            int secs = Mathf.Max(0, Mathf.CeilToInt(timer));
            timerText.text = $"Level {level} | Time: {secs / 60:00}:{secs % 60:00} | Lives: {lives}"; 
        }

        if (timer <= 0) GameOver();
    }

    public void WinGame() { 
        isGameOver = true; 
        Time.timeScale = 0f; // Freeze game
        if(winPanel) winPanel.SetActive(true); 
    }
    
    public void GameOver() { 
        isGameOver = true; 
        Time.timeScale = 0f; // Freeze game
        if(losePanel) losePanel.SetActive(true); 
    }
    
    public void Restart() {
        PlayerPrefs.SetInt("MazeWidth", 15); // Reset size on Game Over
        PlayerPrefs.SetInt("MazeHeight", 15);
        PlayerPrefs.SetInt("CurrentLevel", 1); // Reset level
        Time.timeScale = 1f; // Unfreeze game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel() {
        // Increase maze size by 2 to keep it an odd number
        PlayerPrefs.SetInt("MazeWidth", PlayerPrefs.GetInt("MazeWidth", 15) + 2);
        PlayerPrefs.SetInt("MazeHeight", PlayerPrefs.GetInt("MazeHeight", 15) + 2);
        PlayerPrefs.SetInt("CurrentLevel", level + 1); // Go to next level
        Time.timeScale = 1f; // Unfreeze game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}