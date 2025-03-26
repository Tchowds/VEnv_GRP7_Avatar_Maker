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
using Newtonsoft.Json.Linq; 

/// <summary>
/// Purpose of this class is to interface to the Mannequin avatar prefab to apply
/// skins to them in a way that is consistent with networking and other experience processes
/// </summary>
public class CopyToMannequin : MonoBehaviour
{

    private NetworkContext context;
    private RoomClient roomClient;

    // Interactable, avatar and rendering components attached to the mannequin
    private XRSimpleInteractable copySphereInteractable;
    private FloatingAvatarSeparatedTextures playerFloating;
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

    

    [SerializeField]
    private int mannequinPlayerNum = -1; // Is this mannequin for player 1 or player 2. -1 if the mannequin should not be assigned to a player as by default

    public PlayerExperienceController playerExperienceController;

    public CustomAvatarTextureCatalogue textureCatalogue;  // Reference to the texture catalogue

    public int mannequinId = -1; // Numbering for player experience

    private struct CopyMessage
    {
        public string name;
        public string texture;
    }

    void Start()
    {
        // Floating sphere on mannequin is the interactable object to copy the player's skin to the mannequin
        if (transform.Find("Sphere"))
        {
            copySphereInteractable = transform.Find("Sphere").GetComponent<XRSimpleInteractable>();
            copySphereInteractable.selectEntered.AddListener(Interactable_SelectEntered_CopyToMannequin);
        }

        // renderers for the mannequin
        var floating = transform.Find("Body").GetComponent<FloatingAvatarSeparatedTextures>();
        headRenderer = floating.headRenderer;
        torsoRenderer = floating.torsoRenderer;
        leftHandRenderer = floating.leftHandRenderer;
        rightHandRenderer = floating.rightHandRenderer;

        context = NetworkScene.Register(this);
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
    }

    private void Interactable_SelectEntered_CopyToMannequin(SelectEnterEventArgs arg0)
    {
        var networkScene = NetworkScene.Find(this);
        var avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);

        // Check that the mannequin being saved to is the player's own from Tinker Tailor
        if (playerExperienceController.getPlayerState(roomClient.Me.uuid)!=null)
        {
            int playerNum = playerExperienceController.getPlayerState(roomClient.Me.uuid).playerNum;
            if (playerNum != -1 && playerNum != mannequinPlayerNum)
            {
                Debug.Log("This mannequin is not for player " + roomClient.Me.uuid);
                return;
            }
        }

        playerFloating = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        // Get the player's textures
        Texture2D headTex = playerFloating.headRenderer.material.mainTexture as Texture2D;
        Texture2D torsoTex = playerFloating.torsoRenderer.material.mainTexture as Texture2D;
        Texture2D leftHandTex = playerFloating.leftHandRenderer.material.mainTexture as Texture2D;
        Texture2D rightHandTex = playerFloating.rightHandRenderer.material.mainTexture as Texture2D;

        ApplyAndSave(headTex, torsoTex, leftHandTex, rightHandTex, true, Guid.NewGuid().ToString()); // playerStored: true (the player stored this on the mannequin)

        // Send skin over the network to ensure mannequins (and catalogue) are in sync
        sendMessage();
    
    }

    public void ApplyAndSave(Texture2D headTex, Texture2D torsoTex, Texture2D leftHandTex, Texture2D rightHandTex, bool playerStored, string newTexName)
    {
        // Combine into one texture to be saved and reapplied to the mannequin
        Texture2D combinedTexture = textureCatalogue.CombineTextures(headTex, torsoTex, leftHandTex, rightHandTex);
        combinedTexture.name = newTexName;
        if (playerStored){
            combinedTexture.name += "_player_stored";
        }

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

        if (mannequinPlayerNum != -1)
        {
            playerExperienceController.UpdateMannequinSkin(mannequinId);
            playerExperienceController.SkinSavedOnMannequin(roomClient.Me.uuid, mannequinPlayerNum);
            context.SendJson(new MannequinStoreMessage { playerID = roomClient.Me.uuid, playerNum = mannequinPlayerNum });
        }

    }

    // Method to apply a texture to the mannequin's head, used as an interface for diffusion output
    public void ApplyOnlyHead(Texture2D headTex)
    {
        // Diffusion by default doesn't automatically save the texture   
        ApplyAndSave(
            headTex,
            torsoRenderer.material.mainTexture as Texture2D, 
            leftHandRenderer.material.mainTexture as Texture2D, 
            rightHandRenderer.material.mainTexture as Texture2D,
            false, 
            headTex.name
        );
    }

    // Method to apply a texture to the mannequin's torso, used as an interface for diffusion output
    public void ApplyOnlyTorso(Texture2D torsoTex)
    {
        ApplyAndSave(
            headRenderer.material.mainTexture as Texture2D, 
            torsoTex,
            leftHandRenderer.material.mainTexture as Texture2D, 
            rightHandRenderer.material.mainTexture as Texture2D,
            false, 
            torsoTex.name
        );
    }

    // Sends the texture over the network to ensure mannequins (and catalogue) are in sync
    public void sendMessage()
    {
        context.SendJson(new CopyMessage
        {
            name = headRenderer.material.mainTexture.name,
            texture = EncodeTexture(headRenderer.material.mainTexture)
        });
    }

    // Need to decode the message, apply the skin and sync with catalogue and player experience
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {

        JObject jsonMessage = JObject.Parse(message.ToString());
        if (jsonMessage.ContainsKey("texture"))
        {
            Texture2D tex = DecodeTexture(jsonMessage["texture"].ToString());
            tex.name = jsonMessage["name"].ToString();

            headRenderer.material.mainTexture = tex;
            torsoRenderer.material.mainTexture = tex;
            leftHandRenderer.material.mainTexture = tex;
            rightHandRenderer.material.mainTexture = tex;

            textureCatalogue.AddDynamicTexture(tex);
            ApplyAndSave(tex, tex, tex, tex, true, jsonMessage["name"].ToString());

        } else {
            playerExperienceController.SkinSavedOnMannequin(jsonMessage["playerID"].ToString(), jsonMessage["playerNum"].ToObject<int>());
        }        
    }

    // Encoding method to serialize a texture over WebRTC
    private string EncodeTexture (Texture tex)
    {
        Texture2D tex2D = tex as Texture2D;
        if (tex2D == null) return "";

        byte[] bytes = tex2D.EncodeToPNG();
        return System.Convert.ToBase64String(bytes);
    }

    // Decoding method to deserialize a texture over WebRTC
    private Texture2D DecodeTexture (string encoded)
    {
        byte[] bytes = System.Convert.FromBase64String(encoded);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }

    // Remove interactable listener when the object is destroyed
    void OnDestroy()
    {
        if (copySphereInteractable) copySphereInteractable.selectEntered.RemoveListener(Interactable_SelectEntered_CopyToMannequin);
    }

}
