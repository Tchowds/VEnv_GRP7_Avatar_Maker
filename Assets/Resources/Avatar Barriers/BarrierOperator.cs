using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;

public class BarrierOperator : MonoBehaviour
{

    public bool dualAvatars = false;
    public float countdown = 5.0f;
    public int[] primaryAvatarParts = new int[4];
    public int[] secondaryAvatarParts = new int[4];


    private GameObject primaryAvatar;
    private GameObject SecondaryAvatar;
    private XRSimpleInteractable interactable;
    private NetworkContext context;


    private struct BarrierMessage
    {
        public bool primaryActive;
        public bool secondaryActive;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactable = GetComponentInChildren<XRSimpleInteractable>();
        if (interactable) interactable.selectEntered.AddListener(Interactable_SelectEntered_Match_Avatar);
        
        primaryAvatar = transform.Find("PrimaryAvatar")?.gameObject;
        SecondaryAvatar = transform.Find("SecondaryAvatar")?.gameObject;

        if (dualAvatars)
        {
            primaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, -0.6f);
            SecondaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, 0.6f);
            setupAvatarTextures(SecondaryAvatar, secondaryAvatarParts);
        }
        else
        {
            SecondaryAvatar.SetActive(false);
            primaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, 0);
        }
        setupAvatarTextures(primaryAvatar, primaryAvatarParts);

        context = NetworkScene.Register(this);
    }

    void setupAvatarTextures(GameObject avatar, int[] avatarParts)
{
    // Retrieve the required components
    var floating = avatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();
    var textured = avatar.GetComponent<TexturedAvatar>();

    // Define the avatar parts in the desired order.
    AvatarPart[] parts = { AvatarPart.HEAD, AvatarPart.TORSO, AvatarPart.LEFTHAND, AvatarPart.RIGHTHAND };

    // Loop through each part and set the corresponding texture.
    for (int i = 0; i < parts.Length; i++)
    {
        floating.avatarPart = parts[i];
        textured.SetTexture(avatarParts[i].ToString());
    }
}

    private void Interactable_SelectEntered_Match_Avatar(SelectEnterEventArgs arg0)
    {
        var networkScene = NetworkScene.Find(this);
        var roomClient = networkScene.GetComponentInChildren<RoomClient>();
        var avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);
        
        
        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();
        var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        var avatarCatalogue = playerTexture.Textures;

        if (dualAvatars)
        {
            if (primaryAvatar.activeSelf && SecondaryAvatar.activeSelf)
            {
                if (isSameAvatar(avatarCatalogue, secondaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(true, false);
                else if (isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, true);
            }
            else if (primaryAvatar.activeSelf && isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, false);
            else if (SecondaryAvatar.activeSelf && isSameAvatar(avatarCatalogue, secondaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, false);
            else setActiveAndSendMessage(false, false);
        }
        else
        {
            if (isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) gameObject.SetActive(false);
        }
    }

    private void setActiveAndSendMessage(bool primary, bool secondary)
    {
        gameObject.SetActive(primary || secondary);
        primaryAvatar.SetActive(primary);
        SecondaryAvatar.SetActive(secondary);

        context.SendJson(new BarrierMessage()
        {
            primaryActive = primary,
            secondaryActive = secondary
        });
    }

    private bool isSameAvatar(AvatarTextureCatalogue catalogue, int[] modelParts, FloatingAvatarSeparatedTextures avatar)
    {
        Texture[] avatarTextures = { avatar.headRenderer.material.mainTexture,
                                     avatar.torsoRenderer.material.mainTexture,
                                     avatar.leftHandRenderer.material.mainTexture,
                                     avatar.rightHandRenderer.material.mainTexture };

        for (int i = 0; i < modelParts.Length; i++)
        {
            if (catalogue.Get(modelParts[i]) != avatarTextures[i]) return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (interactable) interactable.selectEntered.RemoveListener(Interactable_SelectEntered_Match_Avatar);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<BarrierMessage>();
        bool remotePrimaryActive = m.primaryActive;
        bool remoteSecondaryActive = m.secondaryActive;
        if(!remotePrimaryActive && !remoteSecondaryActive) gameObject.SetActive(false);
        else
        {
            primaryAvatar.SetActive(remotePrimaryActive);
            SecondaryAvatar.SetActive(remoteSecondaryActive);
        }
    }
}
