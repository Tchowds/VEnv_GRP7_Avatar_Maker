using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;

public class AvatarTextureStealer : MonoBehaviour
{
     public GameObject prefab;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;
    

     private void Start()
    {
        // Connect up the event for the XRI Avatar Poke.
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(Interactable_SelectEntered);
        
        var networkScene = NetworkScene.Find(this); 
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    private void OnDestroy()
    {
        // Cleanup the event for the XRI button so it does not get called after
        // we have been destroyed.
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        }
    }

    private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    {
        // The button has been pressed.
        
        // Change the local avatar prefab to the default one, because we have
        // a few costumes for that avatar bundled with Ubiq. The AvatarManager
        // will do the work of letting other peers know about the prefab change.
        //avatarManager.avatarPrefab = prefab; 
        
        // Also, set the texture to the texture of the model avatar
        SetAvatarTexture();
    }

        private void SetAvatarTexture()
        {
            Debug.Log("Setting avatar texture...");

            // Get the model avatar that was interacted with
            GameObject selectedAvatar = gameObject; // The GameObject this script is attached to (ModelAvatar)

            // Get the TexturedModelAvatar component from the selected avatar
            var modelTexture = selectedAvatar.GetComponent<TexturedModelAvatar>();


            // Retrieve the texture from the model avatar
            Texture2D stolenTexture = modelTexture.GetTexture();


            // Find the player's avatar
            var playerAvatar = avatarManager.FindAvatar(roomClient.Me);

            // Get the player's TexturedAvatar component
            var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();


            // Apply the stolen texture to the player's avatar
            playerTexture.SetTexture(stolenTexture);

            Debug.Log("Texture successfully applied to the player!");
        }

}