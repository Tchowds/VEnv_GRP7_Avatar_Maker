using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
using UnityEngine.XR.Interaction.Toolkit;

public class ModelAvatarTextureSwitcher : MonoBehaviour
{
    private List<TexturedModelAvatar> avatars = new List<TexturedModelAvatar>();

    private Dictionary<int, List<int>> sectionTextureIds; 
    private int currentSectionIndex = 0; 

    private Quaternion initialRotation;

    private int maxTextures;


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
            initialRotation = avatars[0].transform.rotation;
        }

        

        DefineSectionTextureIds();
        StartCoroutine(SwitchTextureSectionCoroutine());
    }

    void DefineSectionTextureIds()
    {   
        // sections
        // 0 - Alien and Astro
        // 1 athlete
        //2 business, casual, farmer, Skater
        //3 Cyborg, fantasy, Zombie, Robot
        //4 military Survivor Criminal
        //5 Racers
        sectionTextureIds = new Dictionary<int, List<int>>();
        sectionTextureIds[0] = new List<int> {0,1,2,3,4,5,6};
        sectionTextureIds[1] = new List<int> {7,8,9,10,11,12,13,14};
        sectionTextureIds[2] = new List<int> {15,16,17,18,19,20,28,29,47,48};
        sectionTextureIds[3] = new List<int> {22,23,24,25,26,27,44,45,46,53,54,55};
        sectionTextureIds[4] = new List<int> {21,30,31,32,33,49,50,51,52,};
        sectionTextureIds[5] = new List<int> { 34,35,36,37,38,39,40,41,42,43};

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
        StartCoroutine(SwitchTextureSectionCoroutine());
    }


    private IEnumerator SwitchTextureSectionCoroutine()
    {
        // Get the new section's texture IDs
        List<int> newTextureIds = sectionTextureIds[currentSectionIndex];

        // Enable and update avatars in the section
        for (int i = 0; i < avatars.Count; i++)
        {
            if (i < newTextureIds.Count)
            {
                int newTextureId = newTextureIds[i];
                avatars[i].gameObject.SetActive(true);
                avatars[i].DefaultTextureId = newTextureId;
                avatars[i].SetTexture(avatars[i].Textures.Get(newTextureId));
                avatars[i].transform.rotation = initialRotation;
                
                Debug.Log($"Avatar {avatars[i].name} switched to Texture ID {newTextureId}");
            }
            else
            {
                avatars[i].gameObject.SetActive(false);
            }

            yield return null; // Allow Unity to process change s over multiple frames
        }
        Debug.Log("Textures switched! Current Section: " + currentSectionIndex);

        // Move to the next section (loop back to 0 after last section)
        currentSectionIndex = (currentSectionIndex + 1) % sectionTextureIds.Count;
        Debug.Log($"Switching to Section {currentSectionIndex}");

    }

}
