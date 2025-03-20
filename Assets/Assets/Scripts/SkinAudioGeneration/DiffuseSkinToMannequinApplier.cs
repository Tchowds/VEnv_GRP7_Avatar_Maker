using System;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class DiffuseSkinToMannequinApplier : MonoBehaviour
{
    [Header("Assign the 4 Avatar Mannequins Here")]
    // List of avatar mannequins (each must have a TexturedModelAvatar component)
    public List<CopyToMannequin> avatarMannequins; 

    [Header("Catalogue Reference")]
    // Reference to the shared texture catalogue where dynamic textures are stored
    public CustomAvatarTextureCatalogue textureCatalogue; 

    private NetworkContext context;

    private void Start() {
        context = NetworkScene.Register(this);
    }

    public void DistributeAndApplySkins(List<string> imagesBase64, string body_part)
    {
        SendSkinsMessage(imagesBase64, body_part);
        ApplyGeneratedSkins(imagesBase64, body_part);
    }

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



        for (int i = 0; i < imagesBase64.Count; i++)
        {
            // Convert base64 string to a Texture2D
            Texture2D texture = ConvertBase64ToTexture(imagesBase64[i]);
            // Add the texture to the catalogue so it remains available
            if (textureCatalogue != null)
            {
                Debug.Log("Adding dynamic texture to catalogue from mannequin: " + i);
                textureCatalogue.AddDynamicTexture(texture);
            }
            else
            {
                Debug.LogWarning("Texture catalogue reference is missing!");
            }

            // Equip the texture on the corresponding avatar mannequin
            if (body_part == "face"){
                // avatarMannequins[i].transform.Find("Body/Floating_Head").GetComponent<Renderer>().material.mainTexture = texture;
                avatarMannequins[i].ApplyOnlyHead(texture);
            }
            else{
                // avatarMannequins[i].transform.Find("Body/Floating_Torso_A").GetComponent<Renderer>().material.mainTexture = texture;
                avatarMannequins[i].ApplyOnlyTorso(texture);
            }
            
        }
    }

    public struct SkinDistibutionMessage
    {
        public List<string> imagesBase64;
        public string body_part;
    }

    public void SendSkinsMessage(List<string> imagesBase64, string body_part)
    {
        var message = new SkinDistibutionMessage
        {
            imagesBase64 = imagesBase64,
            body_part = body_part
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<SkinDistibutionMessage>();
        ApplyGeneratedSkins(m.imagesBase64, m.body_part);
    }


    private Texture2D ConvertBase64ToTexture(string base64Image)
    {
        byte[] imageData = Convert.FromBase64String(base64Image);
        Texture2D texture = new Texture2D(1024, 1024);
        texture.LoadImage(imageData);
        return texture;
    }
}
