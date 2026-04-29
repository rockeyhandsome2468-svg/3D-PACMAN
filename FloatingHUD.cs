using UnityEngine;

public class FloatingHUD : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0, 2.5f, 0); // How high above the player it floats
    public float bobHeight = 0.2f; // How much it bobs up and down
    public float bobSpeed = 2f;    // How fast it bobs

    void LateUpdate()
    {
        // Automatically find the player if not assigned
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        // Calculate the bobbing effect using a sine wave
        float currentBob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 dynamicOffset = offset + new Vector3(0, currentBob, 0);

        // Smoothly hover above the player's head with the new bobbing offset
        transform.position = Vector3.Lerp(transform.position, target.position + dynamicOffset, 10f * Time.deltaTime);

        // Constantly rotate to face the Isometric Camera
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
    }
}