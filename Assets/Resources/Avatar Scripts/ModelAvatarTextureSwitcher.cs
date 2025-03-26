using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using System.Linq;

public class ModelAvatarTextureSwitcher : MonoBehaviour
{
    private List<TexturedModelAvatar> avatars = new List<TexturedModelAvatar>();

    private Dictionary<int, List<int>> sectionTextureIds; 
    private int currentSectionIndex = 0; 

    private Quaternion initialRotation;

    private int maxTextures;

    public CustomAvatarTextureCatalogue Textures;  // Reference to the texture catalogue


    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable pokeInteractable;

    private NetworkContext context;
    private struct SwitchMessage
    {
        public int index;
    }

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


        // Ensure we have textures

        maxTextures = Textures.Count;
        initialRotation = avatars[0].transform.rotation;

        DefineSectionTextureIds();
        StartCoroutine(SwitchTextureSectionCoroutine());

        context = NetworkScene.Register(this);
    }

    void DefineSectionTextureIds()
    {   
        // sections
        // 0 Alien and Astro
        // 1 athlete
        // 2 business, casual, farmer, Skater
        // 3 Cyborg, fantasy, Zombie, Robot
        // 4 military Survivor Criminal
        // 5 Racers
        // 6 Custom dynamic textures
        sectionTextureIds = new Dictionary<int, List<int>>();
        sectionTextureIds[0] = new List<int> {0,1,2,3,4,5,6};
        sectionTextureIds[1] = new List<int> {7,8,9,10,11,12,13,14};
        sectionTextureIds[2] = new List<int> {15,16,17,18,19,20,28,29,47,48};
        sectionTextureIds[3] = new List<int> {22,23,24,25,26,27,44,45,46,53,54,55};
        sectionTextureIds[4] = new List<int> {21,30,31,32,33,49,50,51,52,};
        sectionTextureIds[5] = new List<int> { 34,35,36,37,38,39,40,41,42,43};
        sectionTextureIds[6] = new List<int> {56,57,58,59,60,61,62,63,64,65,66,67};
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
        sendMessage();
        StartCoroutine(SwitchTextureSectionCoroutine());
    }


    private IEnumerator SwitchTextureSectionCoroutine()
    {
        List<int> newTextureIds;
        // Get the new section's texture IDs
        if (currentSectionIndex == 6)
        {
            // If we are in the dynamic id section, load in all the textures that were player_stored
            newTextureIds = new List<int>();
            for (int i = Textures.baseCatalogueCount(); i <  Textures.baseCatalogueCount() + Textures.dynamicTexturesCount(); i++)
            {
                if (Textures.Get(i).name.EndsWith("player_stored")){
                    newTextureIds.Add(i);
                }
            }
            if (newTextureIds.Count > avatars.Count)
            {
                newTextureIds = newTextureIds
                    .Skip(newTextureIds.Count - avatars.Count + 2)
                    .ToList();
            }
    
        } else {
            newTextureIds = sectionTextureIds[currentSectionIndex];
        }

        // Enable and update avatars in the section
        for (int i = 0; i < avatars.Count; i++)
        {
            if (i < newTextureIds.Count)
            {
                int newTextureId = newTextureIds[i];
                Texture2D texture = Textures.Get(newTextureId);
                if (texture != null){
                    avatars[i].gameObject.SetActive(true);
                    avatars[i].DefaultTextureId = newTextureId;
                    avatars[i].SetTexture(texture);
                    avatars[i].transform.rotation = initialRotation;
                } else {
                    avatars[i].gameObject.SetActive(false);
                }
            }
            else
            {
                avatars[i].gameObject.SetActive(false);
            }

            yield return null; // Allow Unity to process change s over multiple frames
        }

        // Move to the next section (loop back to 0 after last section)
        currentSectionIndex = (currentSectionIndex + 1) % sectionTextureIds.Count;

    }

    public void sendMessage()
    {
        SwitchMessage msg = new SwitchMessage();
        msg.index = currentSectionIndex;
        context.SendJson(msg);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<SwitchMessage>();
        StartCoroutine(SwitchTextureSectionCoroutine());
    }

}
