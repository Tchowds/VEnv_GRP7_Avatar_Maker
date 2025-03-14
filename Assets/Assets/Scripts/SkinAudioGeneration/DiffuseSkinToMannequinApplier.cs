using System;
using System.Collections.Generic;
using UnityEngine;

public class DiffuseSkinToMannequinApplier : MonoBehaviour
{
    [Header("Assign the 4 Avatar Mannequins Here")]
    // List of avatar mannequins (each must have a TexturedModelAvatar component)
    public List<TexturedModelAvatar> avatarMannequins; 

    [Header("Catalogue Reference")]
    // Reference to the shared texture catalogue where dynamic textures are stored
    public CustomAvatarTextureCatalogue textureCatalogue; 

    public void ApplyGeneratedSkins(List<string> imagesBase64, string body_part)
    {
        if (avatarMannequins == null || avatarMannequins.Count == 0)
        {
            Debug.LogError("No avatar mannequins assigned!");
            return;
        }
        if (imagesBase64 == null || imagesBase64.Count < avatarMannequins.Count)
        {
            Debug.LogError("Not enough images received to apply to all mannequins.");
            return;
        }

        for (int i = 0; i < avatarMannequins.Count; i++)
        {
            // Convert base64 string to a Texture2D
            Texture2D texture = ConvertBase64ToTexture(imagesBase64[i]);

            // Add the texture to the catalogue so it remains available
            if (textureCatalogue != null)
            {
                textureCatalogue.AddDynamicTexture(texture);
            }
            else
            {
                Debug.LogWarning("Texture catalogue reference is missing!");
            }

            // Equip the texture on the corresponding avatar mannequin
            avatarMannequins[i].SetTexture(texture);
        }
    }

    private Texture2D ConvertBase64ToTexture(string base64Image)
    {
        byte[] imageData = Convert.FromBase64String(base64Image);
        Texture2D texture = new Texture2D(1024, 1024);
        texture.LoadImage(imageData);
        return texture;
    }
}
