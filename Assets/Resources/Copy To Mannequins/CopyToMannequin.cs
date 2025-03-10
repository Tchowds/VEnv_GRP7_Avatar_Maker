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
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

    public CustomAvatarTextureCatalogue textureCatalogue;  // Reference to the texture catalogue

    private struct CopyMessage
    {
        public string headTexture;
        public string torsoTexture;
        public string leftHandTexture;
        public string rightHandTexture;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        copySphereInteractable = transform.Find("Sphere").GetComponent<XRSimpleInteractable>();
        if (copySphereInteractable) copySphereInteractable.selectEntered.AddListener(Interactable_SelectEntered_CopyToMannequin);


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

        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();
        var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        // Get the player's textures
        Texture2D headTex = floatingAvatar.headRenderer.material.mainTexture as Texture2D;
        Texture2D torsoTex = floatingAvatar.torsoRenderer.material.mainTexture as Texture2D;
        Texture2D leftHandTex = floatingAvatar.leftHandRenderer.material.mainTexture as Texture2D;
        Texture2D rightHandTex = floatingAvatar.rightHandRenderer.material.mainTexture as Texture2D;

        // Modify textures (example: tinting them)
        Texture2D modifiedHeadTex = ModifyTexture(headTex);
        Texture2D modifiedTorsoTex = ModifyTexture(torsoTex);
        Texture2D modifiedLeftHandTex = ModifyTexture(leftHandTex);
        Texture2D modifiedRightHandTex = ModifyTexture(rightHandTex);

        // Apply the modified textures to the mannequin
        headRenderer.material.mainTexture = modifiedHeadTex;
        torsoRenderer.material.mainTexture = modifiedTorsoTex;
        leftHandRenderer.material.mainTexture = modifiedLeftHandTex;
        rightHandRenderer.material.mainTexture = modifiedRightHandTex;

        // Save the modified textures as new dynamic textures
        if (textureCatalogue != null)
        {
            textureCatalogue.AddDynamicTexture(modifiedHeadTex);
            textureCatalogue.AddDynamicTexture(modifiedTorsoTex);
            textureCatalogue.AddDynamicTexture(modifiedLeftHandTex);
            textureCatalogue.AddDynamicTexture(modifiedRightHandTex);

            Debug.Log("Modified textures and added them to CustomAvatarTextureCatalogue!");
        }
        else
        {
            Debug.LogError("CustomAvatarTextureCatalogue not found in the scene!");
        }

        sendMessage();
    

    }

    public void sendMessage()
    {
        context.SendJson(new CopyMessage
        {
            headTexture = EncodeTexture(headRenderer.material.mainTexture),
            torsoTexture = EncodeTexture(torsoRenderer.material.mainTexture),
            leftHandTexture = EncodeTexture(leftHandRenderer.material.mainTexture),
            rightHandTexture = EncodeTexture(rightHandRenderer.material.mainTexture)
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<CopyMessage>();
        headRenderer.material.mainTexture = DecodeTexture(m.headTexture);
        torsoRenderer.material.mainTexture = DecodeTexture(m.torsoTexture);
        leftHandRenderer.material.mainTexture = DecodeTexture(m.leftHandTexture);
        rightHandRenderer.material.mainTexture = DecodeTexture(m.rightHandTexture);
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

        //newTexture.name = originalTexture.name + "_Modified_" + DateTime.Now.ToString("HHmmss");
        newTexture.name = textureCatalogue.getNextTextureName();
        Debug.Log($"Created new modified texture {newTexture.name} with matching format: {originalTexture.format}");
        return newTexture;
    }

}
