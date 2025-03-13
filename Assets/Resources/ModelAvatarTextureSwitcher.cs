using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
using UnityEngine.XR.Interaction.Toolkit;

public class ModelAvatarTextureSwitcher : MonoBehaviour
{
    private List<TexturedModelAvatar> avatars = new List<TexturedModelAvatar>(); // Auto-filled
    private int textureOffset = 0; // Current offset, 0 or avatars.Count
    private int maxTextures;
    private bool isSwitching = false; 

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable pokeInteractable;

    void Start()
    {
        pokeInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        
         if (pokeInteractable == null)
        {
            Debug.LogError("No XRBaseInteractable found on door handle.");
            return;
        }
        
        pokeInteractable.selectEntered.AddListener(OnPoked);

        // Get the MixMatchModelAvatars object (sibling of this script's GameObject)
        Transform avatarsParent = transform.parent.Find("MixMatchModelAvatars");
        if (avatarsParent != null)
        {
            // Get all TexturedModelAvatar components inside it
            avatars.AddRange(avatarsParent.GetComponentsInChildren<TexturedModelAvatar>());
        }
        else
        {
            Debug.LogError("MixMatchModelAvatars not found! Check hierarchy.");
        }

        Debug.Log("Avatars found: "+avatars.Count);

        // Ensure we have textures
        if (avatars.Count > 0 && avatars[0].Textures != null)
        {
            maxTextures = avatars[0].Textures.Count;
        }
    }

    void OnDestroy()
    {
        // Cleanup the event when this script is destroyed
        if (pokeInteractable != null)
        {
            pokeInteractable.selectEntered.RemoveListener(OnPoked);
        }
    }

    void OnPoked(SelectEnterEventArgs args)
    {
        StartCoroutine(SwitchTexturesCoroutine());
    }


    private IEnumerator SwitchTexturesCoroutine()
    {
        textureOffset = (textureOffset == avatars.Count) ? 0 : avatars.Count; // Toggle between 0 and the number of avatars we have

        foreach (var avatar in avatars)
        {
            if (avatar != null)
            {
                int newTextureId = (avatar.DefaultTextureId + textureOffset) % maxTextures;
                avatar.DefaultTextureId = newTextureId;
                avatar.SetTexture(avatar.Textures.Get(newTextureId)); // Apply the new texture
                Debug.Log("applied texture to avatar with new textureid "+newTextureId);
            } else {
                Debug.Log("Null");
            }

            yield return null;
        }

        Debug.Log("Textures switched! Current Offset: " + textureOffset);

        yield return null;
    }
}
