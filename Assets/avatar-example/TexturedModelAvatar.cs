using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class TexturedModelAvatar : MonoBehaviour
{
    public AvatarTextureCatalogue Textures;  // Reference to the texture catalogue

    public int DefaultTextureId = 5;  // Default texture for non-local avatars

    private Avatar avatar;
    private RoomClient roomClient;

    private string uuid;  // Unique ID for the texture
    private Texture2D cachedTexture;  // Cache for the texture

    private void Start()
    {
        // Initialize references
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        avatar = GetComponent<Avatar>();

        if (avatar == null)
        {
            Debug.LogError("TexturedModelAvatar requires an Avatar component.");
            return;
        }

        // For local avatars, we apply textures as needed
        if (avatar.IsLocal)
        {
            ApplyRandomTexture();
        }
        else
        {
            // For non-local avatars, apply the default texture (ID 5)
            ApplyDefaultTexture();
        }

        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
    }

    private void OnDestroy()
    {
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }

    // Apply the default texture for non-local avatars
    private void ApplyDefaultTexture()
    {
        if (Textures != null)
        {
            SetTexture(Textures.Get(DefaultTextureId));
        }
        else
        {
            Debug.LogError("Texture catalogue is missing!");
        }
    }

    // Apply a random texture for local avatars
    private void ApplyRandomTexture()
    {
        if (Textures != null)
        {
            SetTexture(Textures.Get(UnityEngine.Random.Range(0, Textures.Count)));
        }
        else
        {
            Debug.LogError("Texture catalogue is missing!");
        }
    }

    // Handle peer updates to synchronize textures across the network
    private void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            return;  // Ignore updates for other avatars
        }

        // Update texture based on the UUID received from the peer
        SetTexture(peer["ubiq.avatar.texture.uuid"]);
    }

    // Set texture from UUID string or apply the texture
    public void SetTexture(string uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
        {
            return;
        }

        if (this.uuid != uuid)
        {
            var texture = Textures.Get(uuid);  // Retrieve the texture using the UUID
            this.uuid = uuid;
            this.cachedTexture = texture;

            // Apply the texture to the model
            ApplyTextureToModel(texture);
        }
    }

    // Set texture using a Texture2D object directly
    public void SetTexture(Texture2D texture)
    {
        this.cachedTexture = texture;
        ApplyTextureToModel(texture);
    }

    // Apply the texture to the model (e.g., the avatar's body)
    private void ApplyTextureToModel(Texture2D texture)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.mainTexture = texture;
        }
    }

    // Return the currently applied texture
    public Texture2D GetTexture()
    {
        return cachedTexture;
    }
}
