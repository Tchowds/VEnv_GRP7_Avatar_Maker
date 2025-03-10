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

    private NetworkContext context;

    private XRSimpleInteractable copySphereInteractable;
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

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

        headRenderer.material.mainTexture = floatingAvatar.headRenderer.material.mainTexture;
        torsoRenderer.material.mainTexture = floatingAvatar.torsoRenderer.material.mainTexture;
        leftHandRenderer.material.mainTexture = floatingAvatar.leftHandRenderer.material.mainTexture;
        rightHandRenderer.material.mainTexture = floatingAvatar.rightHandRenderer.material.mainTexture;

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
}
