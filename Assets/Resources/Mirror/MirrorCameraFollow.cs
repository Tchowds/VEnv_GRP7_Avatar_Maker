using UnityEngine;

public class MirrorCameraFollow : MonoBehaviour
{   
    [Tooltip("Assign the player's XR Rig Main Camera here. If the camera does not move, it's because it hasn't been assigned here.")]

    public Transform playerCamera; // XR Rig's Main Camera
    [Tooltip("Assign the mirror's plane (the reflective surface).")]
    public Transform mirrorPlane;  // The Mirror Plane (reflective surface)
    private Camera mirrorCam; // The camera rendering the mirror

    private RenderTexture mirrorTexture; // Unique Render Texture

    void Start()
    {
        mirrorCam = GetComponent<Camera>(); // Get the Camera component attached to this object
        if (mirrorCam == null)
        {
            Debug.LogError("MirrorCameraFollow: No Camera component found on this GameObject!");
        }

         // create a unique Render Texture for this mirror
        mirrorTexture = new RenderTexture(1024, 1024, 16) 
        {
            name = "MirrorTexture_" + gameObject.name,
            filterMode = FilterMode.Bilinear,
            antiAliasing = 4 
        };

        mirrorCam.targetTexture = mirrorTexture; 

         // Assign the new Render Texture to the mirror material
        Renderer mirrorRenderer = mirrorPlane.GetComponent<Renderer>();
        if (mirrorRenderer != null)
        {
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }
        else
        {
            Debug.LogError("MirrorCameraFollow: No Renderer found on mirrorPlane! Make sure your mirror has a material.");
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

    // Ensure the mirror camera follows horizontal movement, but not tilt (lock roll)
    Vector3 lookDirection = new Vector3(reflectedPosition.x, transform.position.y, reflectedPosition.z) - transform.position;
    Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up); // Forces world-up

    // Lock the roll by setting the z-axis rotation to zero
    targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0f);

    transform.rotation = targetRotation;

    // Apply the same projection matrix as the player camera
    mirrorCam.projectionMatrix = playerCamera.GetComponent<Camera>().projectionMatrix;
}
}
