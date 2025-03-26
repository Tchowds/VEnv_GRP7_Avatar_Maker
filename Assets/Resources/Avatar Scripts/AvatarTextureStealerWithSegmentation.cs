using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;

/// <summary>
/// The class enables players to copy parts of textures to different body parts from a ModelAvatar which has separate interactable body parts.
/// </summary>
public class AvatarTextureStealerWithSegmentation : MonoBehaviour
{

    // All the interactable parts on the model (including the floating cube above the head)
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable headInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable torsoInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable leftHandInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable rightHandInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable cubeInteractable;

    private RoomClient roomClient;
    private AvatarManager avatarManager;

    // Store listeners so we can remove them later.
    private UnityAction<SelectEnterEventArgs> headListener;
    private UnityAction<SelectEnterEventArgs> torsoListener;
    private UnityAction<SelectEnterEventArgs> leftHandListener;
    private UnityAction<SelectEnterEventArgs> rightHandListener;
    private UnityAction<SelectEnterEventArgs> cubeListener;


    private void Start()
    {
        // Find all the interactables from the prefab
        headInteractable = transform.Find("Body/Floating_Head").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        torsoInteractable = transform.Find("Body/Floating_Torso_A").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        leftHandInteractable = transform.Find("Body/Floating_LeftHand_A").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        rightHandInteractable = transform.Find("Body/Floating_RightHand_A").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if(transform.Find("Cube") != null) cubeInteractable = transform.Find("Cube").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        // Create delegates that call our method with the proper parameter.
        headListener = (args) => Interactable_SelectEntered_Segment(AvatarPart.HEAD);
        torsoListener = (args) => Interactable_SelectEntered_Segment(AvatarPart.TORSO);
        leftHandListener = (args) => Interactable_SelectEntered_Segment(AvatarPart.BOTHHANDS);
        rightHandListener = (args) => Interactable_SelectEntered_Segment(AvatarPart.BOTHHANDS);
        cubeListener = (args) => Interactable_SelectEntered_Segment(AvatarPart.FULLBODY);

        // Add listeners using the lambda delegates.
        headInteractable.selectEntered.AddListener(headListener);
        torsoInteractable.selectEntered.AddListener(torsoListener);
        leftHandInteractable.selectEntered.AddListener(leftHandListener);
        rightHandInteractable.selectEntered.AddListener(rightHandListener);
        if(cubeInteractable != null) cubeInteractable.selectEntered.AddListener(cubeListener);

        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    private void OnDestroy()
    {
        if (headInteractable != null) headInteractable.selectEntered.RemoveListener(headListener);
        if (torsoInteractable != null) torsoInteractable.selectEntered.RemoveListener(torsoListener);
        if (leftHandInteractable != null) leftHandInteractable.selectEntered.RemoveListener(leftHandListener);
        if (rightHandInteractable != null) rightHandInteractable.selectEntered.RemoveListener(rightHandListener);
        if (cubeInteractable != null) cubeInteractable.selectEntered.RemoveListener(cubeListener);

    }

    // The listener for each body part interaction
    private void Interactable_SelectEntered_Segment(AvatarPart avatarPart)
    {
        SetAvatarTexture(avatarPart);
    }

    // Set the different body parts of the player's avatar based on the interaction
    private void SetAvatarTexture(AvatarPart avatarPart)
    {
        var modelTexture = gameObject.GetComponent<TexturedModelAvatar>();

        Debug.Log("Stealing part: " + avatarPart);
        if (avatarPart == AvatarPart.FULLBODY)
        {
            SetAvatarTexture(AvatarPart.HEAD);
            SetAvatarTexture(AvatarPart.TORSO);
            SetAvatarTexture(AvatarPart.LEFTHAND);
            SetAvatarTexture(AvatarPart.RIGHTHAND);
            return;
        } else if (avatarPart == AvatarPart.BOTHHANDS)
        {
            SetAvatarTexture(AvatarPart.LEFTHAND);
            SetAvatarTexture(AvatarPart.RIGHTHAND);
            return;
        }
        Texture2D stolenTexture = modelTexture.GetTexture(avatarPart);
        Debug.Log("Stolen texture: " + stolenTexture);

        // Find the player's avatar
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);
        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();

        // Apply the stolen texture to the player's avatar
        playerTexture.SetTexture(stolenTexture, avatarPart);

        Debug.Log("Texture successfully applied to the player!");
    }

}