using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
using System;


public class CopyToMannequin : MonoBehaviour
{

    private NetworkContext context;

    private XRSimpleInteractable copySphereInteractable;
    private FloatingAvatarSeparatedTextures playerFloating;
    private TexturedAvatar playerTextured;
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

    public CustomAvatarTextureCatalogue textureCatalogue;  // Reference to the texture catalogue

    private struct CopyMessage
    {
        public string name;
        public string texture;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        copySphereInteractable = transform.Find("Sphere").GetComponent<XRSimpleInteractable>();
        if (copySphereInteractable) copySphereInteractable.selectEntered.AddListener(Interactable_SelectEntered_CopyToMannequin);

        // renderers for the mannequin
        var floating = transform.Find("Body").GetComponent<FloatingAvatarSeparatedTextures>();
        headRenderer = floating.headRenderer;
        torsoRenderer = floating.torsoRenderer;
        leftHandRenderer = floating.leftHandRenderer;
        rightHandRenderer = floating.rightHandRenderer;

        context = NetworkScene.Register(this);
    }

    private void Interactable_SelectEntered_CopyToMannequin(SelectEnterEventArgs arg0)
    {
        var networkScene = NetworkScene.Find(this);
        var roomClient = networkScene.GetComponentInChildren<RoomClient>();
        var avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);

        playerTextured = playerAvatar.GetComponent<TexturedAvatar>();
        playerFloating = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        // Get the player's textures
        Texture2D headTex = playerFloating.headRenderer.material.mainTexture as Texture2D;
        Texture2D torsoTex = playerFloating.torsoRenderer.material.mainTexture as Texture2D;
        Texture2D leftHandTex = playerFloating.leftHandRenderer.material.mainTexture as Texture2D;
        Texture2D rightHandTex = playerFloating.rightHandRenderer.material.mainTexture as Texture2D;

        ApplyAndSave(headTex, torsoTex, leftHandTex, rightHandTex);

        sendMessage();
    
    }

    public void ApplyAndSave(Texture2D headTex, Texture2D torsoTex, Texture2D leftHandTex, Texture2D rightHandTex)
    {
        // Combine into 1 texture
        Debug.Log("Cominging textures");
        Texture2D combinedTexture = textureCatalogue.CombineTextures(headTex, torsoTex, leftHandTex, rightHandTex);

        headRenderer.material.mainTexture = combinedTexture;
        torsoRenderer.material.mainTexture = combinedTexture;
        leftHandRenderer.material.mainTexture = combinedTexture;
        rightHandRenderer.material.mainTexture = combinedTexture;

        // Save the modified textures as new dynamic textures
        if (textureCatalogue != null)
        {
            textureCatalogue.AddDynamicTexture(combinedTexture);

            Debug.Log("Modified textures and added them to CustomAvatarTextureCatalogue!");
        }
        else
        {
            Debug.LogError("CustomAvatarTextureCatalogue not found in the scene!");
        }
    }

    public void sendMessage()
    {
        context.SendJson(new CopyMessage
        {
            // headName = headRenderer.material.mainTexture.name,
            // headTexture = EncodeTexture(headRenderer.material.mainTexture),
            // torsoName = torsoRenderer.material.mainTexture.name,
            // torsoTexture = EncodeTexture(torsoRenderer.material.mainTexture),
            // leftHandName = leftHandRenderer.material.mainTexture.name,
            // leftHandTexture = EncodeTexture(leftHandRenderer.material.mainTexture),
            // rightHandName = rightHandRenderer.material.mainTexture.name,
            // rightHandTexture = EncodeTexture(rightHandRenderer.material.mainTexture)
            name = headRenderer.material.mainTexture.name,
            texture = EncodeTexture(headRenderer.material.mainTexture)
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<CopyMessage>();
        // Texture2D headTex = DecodeTexture(m.headTexture);
        // headTex.name = m.headName;
        // Texture2D torsoTex = DecodeTexture(m.torsoTexture);
        // torsoTex.name = m.torsoName;
        // Texture2D leftHandTex = DecodeTexture(m.leftHandTexture);
        // leftHandTex.name = m.leftHandName;
        // Texture2D rightHandTex = DecodeTexture(m.rightHandTexture);
        // rightHandTex.name = m.rightHandName;
        Texture2D tex = DecodeTexture(m.texture);
        tex.name = m.name;

        headRenderer.material.mainTexture = tex;
        torsoRenderer.material.mainTexture = tex;
        leftHandRenderer.material.mainTexture = tex;
        rightHandRenderer.material.mainTexture = tex;

        textureCatalogue.AddDynamicTexture(tex);
        // ApplyAndSave(headTex, torsoTex, leftHandTex, rightHandTex);
    }

    private string EncodeTexture (Texture tex)
    {
        Texture2D tex2D = tex as Texture2D;
        if (tex2D == null) return "";

        byte[] bytes = tex2D.EncodeToPNG();
        return System.Convert.ToBase64String(bytes);
    }

    private Texture2D DecodeTexture (string encoded)
    {
        byte[] bytes = System.Convert.FromBase64String(encoded);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }

    void OnDestroy()
    {
        if (copySphereInteractable) copySphereInteractable.selectEntered.RemoveListener(Interactable_SelectEntered_CopyToMannequin);
    }

    private Texture2D ModifyTexture(Texture2D originalTexture)
    {
        if (originalTexture == null)
        {
            Debug.LogError("ModifyTexture: Original texture is null!");
            return null;
        }
        Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGB24, false);

        newTexture.SetPixels(originalTexture.GetPixels());

        // Apply modification (red tint)
        Color[] pixels = newTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] *= new Color(0.8f, 1.2f, 0.8f); // Apply green tint
            pixels[i].a = 1f;
        }
        newTexture.SetPixels(pixels);
        newTexture.Apply();

        newTexture.name = originalTexture.name + "_Modified_" + DateTime.Now.ToString("HHmmss");
        // newTexture.name = textureCatalogue.getNextTextureName();
        Debug.Log($"Created new modified texture {newTexture.name} with matching format: {originalTexture.format}");
        return newTexture;
    }

}
