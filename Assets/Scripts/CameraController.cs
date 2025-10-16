using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player; // The player the camera will follow

    void LateUpdate()
    {
        if (player == null) return;

        // Follow the player's position and keep the camera's original Z value
        Vector3 newPos = player.transform.position;
        newPos.z = transform.position.z;

        transform.position = newPos;
    }
}
