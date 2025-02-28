using UnityEngine;
using Ubiq.Rooms;

public class ShopTrigger : MonoBehaviour
{
    public ShopMusicManager musicManager; // Reference to the ShopMusicManager script
    public CrowdNoiseManager crowdNoiseManager; // Reference to the ShopMusicManager script

    public GameObject playerXRig; // Reference to the XR Rig (player's rig)

    void OnTriggerEnter(Collider other)
    {
        // If the object entering the trigger is the player's XR Rig
        if (other.gameObject == playerXRig)
        {
            Debug.Log("Player entered the shop");
            musicManager.EnterShop();  // Play the music when the local player enters
            crowdNoiseManager.ExitOutside();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // If the object exiting the trigger is the player's XR Rig
        if (other.gameObject == playerXRig)
        {
            Debug.Log("Player exited the shop");
            musicManager.ExitShop();  // Stop the music when the local player exits
            crowdNoiseManager.EnterOutside();
        }
    }
}
