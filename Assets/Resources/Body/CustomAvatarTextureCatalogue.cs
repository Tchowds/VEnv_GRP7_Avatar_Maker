using UnityEngine;
using Ubiq.Avatars;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Custom Avatar Texture Catalogue")]
public class CustomAvatarTextureCatalogue : AvatarTextureCatalogue
{
    [SerializeField] private AvatarTextureCatalogue baseCatalogue; // Ubiq's Catalogue
    private List<Texture2D> dynamicTextures = new List<Texture2D>();

    public void Initialize(AvatarTextureCatalogue existingCatalogue)
    {
        if (existingCatalogue == null)
        {
            Debug.LogError("CustomAvatarTextureCatalogue: Provided base catalogue is null!");
            return;
        }
        
        baseCatalogue = existingCatalogue;
        Debug.Log($"CustomAvatarTextureCatalogue: Loaded {baseCatalogue.Textures.Count} base textures.");
    }

    private void OnEnable()
{
    Textures = new List<Texture2D>(baseCatalogue.Textures);
    Debug.Log($"Copied {Textures.Count} textures from base catalogue.");
}


    public Texture2D Get(int i)
    {
        if (baseCatalogue == null || baseCatalogue.Textures == null)
        {
            Debug.LogError("CustomAvatarTextureCatalogue: Base catalogue is not assigned or empty!");
            return null;
        }

        Debug.Log($"Get texture {i} (Base Count: {baseCatalogue.Textures.Count}, Dynamic Count: {dynamicTextures.Count})");

        // Use existing textures first
        if (i < baseCatalogue.Textures.Count)
        {
            return baseCatalogue.Textures[i];
        }
        else
        {
            // Handle dynamically added textures
            int dynamicIndex = i - baseCatalogue.Textures.Count;
            while (dynamicTextures.Count <= dynamicIndex)
            {
                Texture2D newTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
                dynamicTextures.Add(newTex);
            }
            return dynamicTextures[dynamicIndex];
        }
    }

    public void AddDynamicTexture(Texture2D texture)
    {
        dynamicTextures.Add(texture);
    }
}
