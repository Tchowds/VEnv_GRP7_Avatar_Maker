using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorHandlePokeInteraction : MonoBehaviour
{
    public DoorController doorController;  // Reference to the DoorController script

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable pokeInteractable;

    void Start()
    {
        pokeInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();  // Get the XRBaseInteractable component
        
        if (pokeInteractable == null)
        {
            Debug.LogError("No XRBaseInteractable found on door handle.");
            return;
        }

        // Set up the event listeners for poke interaction using the updated API
        pokeInteractable.selectEntered.AddListener(OnPoked);
    }

    void OnDestroy()
    {
        // Cleanup the event when this script is destroyed
        if (pokeInteractable != null)
        {
            pokeInteractable.selectEntered.RemoveListener(OnPoked);
        }
    }

    // Trigger action when the door handle is poked
    void OnPoked(SelectEnterEventArgs args)
    {
        Debug.Log(args);
        Debug.Log("Door handle poked!");
        // Toggle the door open/close state
        if (doorController != null)
        {
            doorController.ToggleDoorState();
        }
    }
}
