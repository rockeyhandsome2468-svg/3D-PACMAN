using UnityEngine;

public class SceneBootstrapper : MonoBehaviour
{
    void Awake()
    {
        // ── Player ────────────────────────────────────────────
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag  = "Player";

        Material pm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pm.color = new Color(0.5f, 0.8f, 1f); // Pastel Blue
        pm.SetFloat("_Smoothness", 0f); // Remove shininess for flat look
        
        // Apply base material and the new outline material
        Material outlineMat = new Material(Shader.Find("Custom/ToonOutline"));
        player.GetComponent<Renderer>().materials = new Material[] { pm, outlineMat };

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Disable physics gravity to stop the player from falling
        player.AddComponent<PlayerController>();

        // ── Enemy ─────────────────────────────────────────────
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.tag  = "Enemy";

        Material em = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        em.color = new Color(1f, 0.6f, 0.6f); // Pastel Red
        em.SetFloat("_Smoothness", 0f);
        
        enemy.GetComponent<Renderer>().materials = new Material[] { em, outlineMat };

        enemy.AddComponent<EnemyAI>();
    }
}