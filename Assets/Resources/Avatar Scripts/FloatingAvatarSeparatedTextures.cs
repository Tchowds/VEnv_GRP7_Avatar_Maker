using Ubiq;
using UnityEngine;

//
public enum AvatarPart
{
    HEAD,
    TORSO,
    LEFTHAND,
    RIGHTHAND,
    BOTHHANDS,
    FULLBODY
}

/// <summary>
/// Derived Class from FloatingAvatar
/// Enables the use of separated textures for each part of the avatar through AvatarPart enum
/// </summary>
public class FloatingAvatarSeparatedTextures : FloatingAvatar
{
    // Defines the mode of the avatar part
    public AvatarPart avatarPart = AvatarPart.FULLBODY;

    // Override the texture update method to only apply the texture to the correct part of the avatar
    protected override void TexturedAvatar_OnTextureChanged(Texture2D tex)
    {
        if (avatarPart == AvatarPart.FULLBODY)
        {
            headRenderer.material.mainTexture = tex;
            torsoRenderer.material.mainTexture = tex;
            leftHandRenderer.material.mainTexture = tex;
            rightHandRenderer.material.mainTexture = tex;
        }
        else if (avatarPart == AvatarPart.HEAD)
        {
            headRenderer.material.mainTexture = tex;
        }
        else if (avatarPart == AvatarPart.TORSO)
        {
            torsoRenderer.material.mainTexture = tex;
        }
        else if (avatarPart == AvatarPart.LEFTHAND)
        {
            leftHandRenderer.material.mainTexture = tex;
        }
        else if (avatarPart == AvatarPart.RIGHTHAND)
        {
            rightHandRenderer.material.mainTexture = tex;
        }
        else if (avatarPart == AvatarPart.BOTHHANDS)
        {
            leftHandRenderer.material.mainTexture = tex;
            rightHandRenderer.material = leftHandRenderer.material;
        }
        else
        {
            Debug.LogError("AvatarPart not found");
        }
    }
}