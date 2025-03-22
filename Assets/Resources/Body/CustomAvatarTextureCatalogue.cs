using UnityEngine;
using Ubiq.Avatars;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;


[CreateAssetMenu(menuName = "Custom Avatar Texture Catalogue")]
public class CustomAvatarTextureCatalogue : AvatarTextureCatalogue
{
    [SerializeField, Tooltip("Base Ubiq Avatar Catalogue")]
     public AvatarTextureCatalogue baseCatalogue; // Ubiq's Catalogue
    [SerializeField, Tooltip("Dynamically generated and saved textures")]
    private List<Texture2D> dynamicTextures = new List<Texture2D>();
    
    [SerializeField, Tooltip("Number of recent textures to load.")]
    private int numDynamicTexturesLoad;

    public void LoadRecentDynamicTextures()
    {
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path))
        {
            Debug.LogError("Persistent data path does not exist!");
            return;
        }
        var skinFiles = Directory.GetFiles(path, "*.png")
                             .OrderByDescending(File.GetLastWriteTime)
                             .Take(numDynamicTexturesLoad)
                             .Reverse()
                             .ToList();
    
        foreach (var file in skinFiles)
        {
            Texture2D loadedTexture = LoadTextureFromFile(file);
            if (loadedTexture != null)
            {
                dynamicTextures.Add(loadedTexture);
            }
        }
    }

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

    public int baseCatalogueCount()
    {
        return baseCatalogue.Textures.Count;
    }

    public int dynamicTexturesCount()
    {
        return dynamicTextures.Count;
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

        dynamicTextures = new List<Texture2D>(); 
        LoadRecentDynamicTextures();
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
            if (dynamicIndex >= dynamicTextures.Count){
                return null;
            }
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

        // Debug.Log($"Texture name: {texture.name}");

        // If the texture is dynamic, return its name
        foreach (var dynamicTexture in dynamicTextures)
        {
            // Debug.Log($"Dynamic texture name: {dynamicTexture.name}");
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
        // TODO - this doesnt need to return the texture
        Texture2D savedTexture = SaveTexture(texture);

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

    public Texture2D CombineTextures(Texture2D headTex, Texture2D torsoTex, Texture2D leftHandTex, Texture2D rightHandTex)
    {
        Texture2D combinedTexture = new Texture2D(1024, 1024);
        combinedTexture.name = "combined_texture_"+Guid.NewGuid().ToString()+"_player_stored";

        Rect headRegion = new Rect(0, 535, 641, 488);  
        Rect torsoRegionUpper = new Rect(0, 0, 624, 533);
        Rect torsoRegionLower = new Rect(620, 0, 400, 259);
        Rect handRegion = new Rect(640, 502, 381, 522);
        Rect unknownRegion = new Rect(625, 260, 397, 240);

        Debug.Log("here");
        Debug.Log("Combined texture: "+combinedTexture.name);

        // Copy regions from each texture
        CopyRegion(combinedTexture, headTex, headRegion);
        CopyRegion(combinedTexture, torsoTex, torsoRegionUpper);
        CopyRegion(combinedTexture, torsoTex, torsoRegionLower);
        CopyRegion(combinedTexture, leftHandTex, handRegion);
        CopyRegion(combinedTexture, rightHandTex, unknownRegion);

        // Apply changes
        combinedTexture.Apply();

        return combinedTexture;
    }

    private void CopyRegion(Texture2D target, Texture2D source, Rect region)
    {
        int xStart = Mathf.RoundToInt(region.x);
        int yStart = Mathf.RoundToInt(region.y);
        int width = Mathf.RoundToInt(region.width);
        int height = Mathf.RoundToInt(region.height);

        Color[] pixels = source.GetPixels(xStart, yStart, width, height);
        target.SetPixels(xStart, yStart, width, height, pixels);
    }

    private Texture2D SaveTexture(Texture2D texture)
    {
        // Use the texture's name when saving
        if (texture.name == null)
        {
            Debug.LogError("Texture name is null! for texture: "+texture);
            Debug.Log(texture.name);
        }

        string fileName = texture.name + ".png";
        string path = $"{Application.persistentDataPath}/{fileName}";
        
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        Debug.Log($"Saved texture as PNG: {path}");

        // Load the PNG as a new Texture2D - this solves a Null texture error by reloading the texture
        return LoadTextureFromFile(path);
    }
    
    private Texture2D LoadTextureFromFile(string filePath)
    {

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (texture.LoadImage(fileData))
        {
            texture.name = Path.GetFileNameWithoutExtension(filePath);
            return texture;
        } 

        Debug.LogError($"Failed to load texture from {filePath}");
        return null;
    }



}
