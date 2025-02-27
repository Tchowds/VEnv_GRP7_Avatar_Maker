using UnityEngine;

public class MirrorCameraFollow : MonoBehaviour
{
    public Transform playerCamera; // XR Rig's Main Camera
    public Transform mirrorPlane;  // The Mirror Plane (reflective surface)
    private Camera mirrorCam; // The camera rendering the mirror

    void Start()
    {
        mirrorCam = GetComponent<Camera>(); // Get the Camera component attached to this object
        if (mirrorCam == null)
        {
            Debug.LogError("MirrorCameraFollow: No Camera component found on this GameObject!");
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null || mirrorPlane == null || mirrorCam == null)
        {
            Debug.LogError("MirrorCameraFollow: Missing references! Assign playerCamera, mirrorPlane, and ensure a Camera component is attached.");
            return;
        }

        // Ensure the mirror normal is correct (whichever worked before)
        Vector3 mirrorNormal = mirrorPlane.right; // Adjust this if necessary

        // Reflect the camera’s position across the mirror
        Vector3 toPlayer = playerCamera.position - mirrorPlane.position;
        Vector3 reflectedPosition = playerCamera.position - 2 * Vector3.Dot(toPlayer, mirrorNormal) * mirrorNormal;

        // Ensure the mirror camera follows horizontal movement, but not tilt
        Vector3 lookDirection = new Vector3(reflectedPosition.x, transform.position.y, reflectedPosition.z) - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up); // Forces world-up

        // Apply the same projection matrix as the player camera
        mirrorCam.projectionMatrix = playerCamera.GetComponent<Camera>().projectionMatrix;
    }
}
