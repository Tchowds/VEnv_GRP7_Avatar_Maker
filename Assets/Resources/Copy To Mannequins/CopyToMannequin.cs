using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;

public class CopyToMannequin : MonoBehaviour
{


    private XRSimpleInteractable copySphereInteractable;
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

    private struct CopyMessage
    {
        public Texture headTexture;
        public Texture torsoTexture;
        public Texture leftHandTexture;
        public Texture rightHandTexture;
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
    }

    private void Interactable_SelectEntered_CopyToMannequin(SelectEnterEventArgs arg0)
    {
        var networkScene = NetworkScene.Find(this);
        var roomClient = networkScene.GetComponentInChildren<RoomClient>();
        var avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);

        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();
        var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        headRenderer.material.mainTexture = floatingAvatar.headRenderer.material.mainTexture;
        torsoRenderer.material.mainTexture = floatingAvatar.torsoRenderer.material.mainTexture;
        leftHandRenderer.material.mainTexture = floatingAvatar.leftHandRenderer.material.mainTexture;
        rightHandRenderer.material.mainTexture = floatingAvatar.rightHandRenderer.material.mainTexture;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<CopyMessage>();
        headRenderer.material.mainTexture = m.headTexture;
        torsoRenderer.material.mainTexture = m.torsoTexture;
        leftHandRenderer.material.mainTexture = m.leftHandTexture;
        rightHandRenderer.material.mainTexture = m.rightHandTexture;
    }

    void OnDestroy()
    {
        if (copySphereInteractable) copySphereInteractable.selectEntered.RemoveListener(Interactable_SelectEntered_CopyToMannequin);
    }
}
