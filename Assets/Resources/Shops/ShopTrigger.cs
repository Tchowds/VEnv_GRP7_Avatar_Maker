using UnityEngine;
using Ubiq.Rooms;

public class ShopTrigger : MonoBehaviour
{
    public ShopManager shopManager; // Reference to the MixMatchShopManager script

    public GameObject playerXRig; // Reference to the XR Rig (player's rig)

    void OnTriggerEnter(Collider other)
    {
        // If the object entering the trigger is the player's XR Rig
        if (other.gameObject == playerXRig)
        {
            Debug.Log("Player entered the shop");
            shopManager.EnterShop();  // Play the music when the local player enters
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerXRig)
        {
            Debug.Log("Player exited the shop");
            shopManager.ExitShop();  // Stop the music when the local player exits
        }
    }
}
