using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;
using System.Linq;

/// <summary>
/// This class sets the avatar to use a specific texture. It also handles
/// syncing the currently active texture over the network using properties.
/// </summary>
public class TexturedAvatar : MonoBehaviour
{
    public CustomAvatarTextureCatalogue Textures;
    public bool RandomTextureOnSpawn;
    public bool SaveTextureSetting;

    [Serializable]
    public class TextureEvent : UnityEvent<Texture2D> { }
    public TextureEvent OnTextureChanged;

    private Avatar avatar;
    private string uuid;
    private RoomClient roomClient;

    private Texture2D cached; // Cache for GetTexture. Do not do anything else with this; use the uuid

    private int peerCount = 1;

    private void Start()
    {
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        
        avatar = GetComponent<Avatar>();

        if (avatar == null)
        {
            Debug.LogError("TexturedAvatar requires an Avatar component.");
            return;
        }
        
        if (avatar.IsLocal)
        {
            var hasSavedSettings = false;
            if (SaveTextureSetting)
            {
                hasSavedSettings = LoadSettings();
            }
            if (!hasSavedSettings && RandomTextureOnSpawn)
            {
                SetTexture(Textures.Get(UnityEngine.Random.Range(0, Textures.baseCatalogueCount())));
            }
        }
        
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
    }

    private void Update()
    {
        if (roomClient.Peers.Count() != peerCount)
        {
            peerCount = roomClient.Peers.Count();
            SaveSettings();
        }
    }

    private void OnDestroy()
    {
        // Cleanup the event for new properties so it does not get called after
        // we have been destroyed.
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }

    void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        
        SetTexture(peer["ubiq.avatar.texture.uuid"]);
        SetTexture(Textures.Get(peer["ubiq.avatar.texture.head.uuid"]), AvatarPart.HEAD);
        SetTexture(Textures.Get(peer["ubiq.avatar.texture.torso.uuid"]), AvatarPart.TORSO);
        SetTexture(Textures.Get(peer["ubiq.avatar.texture.lefthand.uuid"]), AvatarPart.LEFTHAND);
        SetTexture(Textures.Get(peer["ubiq.avatar.texture.righthand.uuid"]), AvatarPart.RIGHTHAND);
    }

    /// <summary>
    /// Try to set the Texture by reference to a Texture in the Catalogue. If the Texture is not in the
    /// catalogue then this method has no effect, as Texture2Ds cannot be streamed yet.
    /// </summary>
    public void SetTexture(Texture2D texture)
    {
        Debug.Log("Setting texture to" + texture);
        SetTexture(Textures.Get(texture));
    }

    public void SetTexture(Texture2D texture, AvatarPart avatarPart)
    {
        var floatingAvatar = GetComponentInChildren<FloatingAvatarSeparatedTextures>();
        floatingAvatar.avatarPart = avatarPart;
        SetTexture(Textures.Get(texture));
    }

    public void SetTexture(string uuid)
    {
        var floatingAvatar = GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        if(String.IsNullOrWhiteSpace(uuid))
        {
            Debug.Log(uuid);
            Debug.Log("uuid is null");
            return;
        }

        var texture = Textures.Get(uuid);
        this.uuid = uuid;
        this.cached = texture;

        OnTextureChanged.Invoke(texture);

        if(avatar.IsLocal)
        {
            roomClient.Me["ubiq.avatar.texture.uuid"] = this.uuid;
            roomClient.Me["ubiq.avatar.texture.head.uuid"] = Textures.Get(floatingAvatar.headRenderer.material.mainTexture as Texture2D);
            roomClient.Me["ubiq.avatar.texture.torso.uuid"] = Textures.Get(floatingAvatar.torsoRenderer.material.mainTexture as Texture2D);
            roomClient.Me["ubiq.avatar.texture.lefthand.uuid"] = Textures.Get(floatingAvatar.leftHandRenderer.material.mainTexture as Texture2D);
            roomClient.Me["ubiq.avatar.texture.righthand.uuid"] = Textures.Get(floatingAvatar.rightHandRenderer.material.mainTexture as Texture2D);
        }

        if (avatar.IsLocal && SaveTextureSetting)
        {
            SaveSettings();
        }
    }

    // These settings have been changed by Taha
    private void SaveSettings()
    {
        var floatingAvatar = GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        PlayerPrefs.SetString("ubiq.avatar.texture.uuid", uuid);
        PlayerPrefs.SetString("ubiq.avatar.texture.head.uuid", Textures.Get(floatingAvatar.headRenderer.material.mainTexture as Texture2D));
        PlayerPrefs.SetString("ubiq.avatar.texture.torso.uuid", Textures.Get(floatingAvatar.torsoRenderer.material.mainTexture as Texture2D));
        PlayerPrefs.SetString("ubiq.avatar.texture.lefthand.uuid", Textures.Get(floatingAvatar.leftHandRenderer.material.mainTexture as Texture2D));
        PlayerPrefs.SetString("ubiq.avatar.texture.righthand.uuid", Textures.Get(floatingAvatar.rightHandRenderer.material.mainTexture as Texture2D));
    }

    private bool LoadSettings()
    {
        var uuid = PlayerPrefs.GetString("ubiq.avatar.texture.uuid", "");

        var headUuid = PlayerPrefs.GetString("ubiq.avatar.texture.head.uuid", "");
        var torsoUuid = PlayerPrefs.GetString("ubiq.avatar.texture.torso.uuid", "");
        var leftHandUuid = PlayerPrefs.GetString("ubiq.avatar.texture.lefthand.uuid", "");
        var rightHandUuid = PlayerPrefs.GetString("ubiq.avatar.texture.righthand.uuid", "");

        SetTexture(uuid);
        SetTexture(Textures.Get(headUuid), AvatarPart.HEAD);
        SetTexture(Textures.Get(torsoUuid), AvatarPart.TORSO);
        SetTexture(Textures.Get(leftHandUuid), AvatarPart.LEFTHAND);
        SetTexture(Textures.Get(rightHandUuid), AvatarPart.RIGHTHAND);

        return !String.IsNullOrWhiteSpace(uuid);
    }

    public void ClearSettings()
    {
        PlayerPrefs.DeleteKey("ubiq.avatar.texture.uuid");
    }

    public Texture2D GetTexture()
    {
        return cached;
    }
}
