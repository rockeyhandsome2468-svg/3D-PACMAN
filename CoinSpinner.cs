using UnityEngine;

public class CoinSpinner : MonoBehaviour
{
    public float spinSpeed = 150f;

    void Update()
    {
        // Spin the coin smoothly around the global Y-axis
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}