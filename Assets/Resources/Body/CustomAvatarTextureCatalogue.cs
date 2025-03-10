using UnityEngine;
using Ubiq.Avatars;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Custom Avatar Texture Catalogue")]
public class CustomAvatarTextureCatalogue : AvatarTextureCatalogue
{
    [SerializeField] private AvatarTextureCatalogue baseCatalogue; // Ubiq's Catalogue
    [SerializeField] private List<Texture2D> dynamicTextures = new List<Texture2D>();
    // [System.NonSerialized]
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


    public string getNextTextureName()
    {
        return "texture" + dynamicTextures.Count;
    }

    private void OnEnable()
    {
        if (baseCatalogue == null || baseCatalogue.Textures == null)
        {
            Debug.LogError("CustomAvatarTextureCatalogue: Base catalogue is not assigned or empty!");
            return;
        }

        // Load base textures from the base catalogue
        Textures = new List<Texture2D>(baseCatalogue.Textures);
        Debug.Log($"Copied {Textures.Count} textures from base catalogue.");

        // ✅ Only initialize if NULL (DO NOT RESET)
        if (dynamicTextures == null)
        {
            Debug.Log("🔄 Ensuring `dynamicTextures` is initialized.");
            dynamicTextures = new List<Texture2D>(); 
        }
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
            return dynamicTextures[dynamicIndex];
        }
    }

    public Texture2D Get(string luid)
    {
        if (string.IsNullOrEmpty(luid))
        {
            return null;
        }

        // ✅ If UUID is a number, return the base texture
        if (int.TryParse(luid, out int index) && index < baseCatalogue.Textures.Count)
        {
            return baseCatalogue.Get(index);
        }

        // ✅ If UUID is a dynamic texture name, find and return it
        foreach (var texture in dynamicTextures)
        {
            if (texture.name == luid)
            {
                return texture;
            }
        }

        Debug.LogError($"Texture with UUID {luid} not found.");
        return null;
    }

    public string Get(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        // If the texture is in the base catalogue, return its index as UUID
        int index = baseCatalogue.Textures.IndexOf(texture);
        if (index > -1)
        {
            return index.ToString();
        }

        Debug.Log($"Texture name: {texture.name}");

        // If the texture is dynamic, return its name
        foreach (var dynamicTexture in dynamicTextures)
        {
            Debug.Log($"Dynamic texture name: {dynamicTexture.name}");
            if (dynamicTexture.name == texture.name)
            {
                return dynamicTexture.name;
            }
        }

        Debug.LogError($"UUID not found for {texture}.");
        return null;
    }



    public void AddDynamicTexture(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Cannot add a null texture!");
            return;
        }

        // Check if the texture already exists
        foreach (var existingTexture in dynamicTextures)
        {
            if (existingTexture == texture)
            {
                Debug.Log($"Skipping duplicate dynamic texture: {texture.name}");
                return;
            }
        }

        Texture2D savedTexture = SaveTextureAsAsset(texture);

        if (savedTexture != null)
        {
            dynamicTextures.Add(savedTexture);
            Debug.Log($"Successfully saved and added dynamic texture: {savedTexture.name}");
        }
        else
        {
            Debug.LogError("Failed to save dynamic texture!");
        }
    }

    private Texture2D SaveTextureAsAsset(Texture2D texture)
    {
        // Use the texture's name when saving
        string fileName = texture.name + ".png";
        string path = $"{Application.persistentDataPath}/{fileName}";
        
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        Debug.Log($"Saved texture as PNG: {path}");

        // Load the PNG as a new Texture2D
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D newTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (newTexture.LoadImage(fileData))
        {
            newTexture.name = texture.name;
            Debug.Log($"Loaded saved texture: {newTexture.name}");
            return newTexture;
        }

        Debug.LogError("Failed to load saved texture from file!");
        return null;
    }



}
