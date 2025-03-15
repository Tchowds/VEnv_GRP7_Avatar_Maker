using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;

public class AvatarTextureStealerWithSegmentation : MonoBehaviour
{
    public GameObject prefab;

    // public BoxCollider headCollider;
    // public BoxCollider torsoCollider;
    // public BoxCollider leftHandCollider;
    // public BoxCollider rightHandCollider;


    // private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
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
        // Connect up the event for the XRI Avatar Poke.
        // interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        // interactable.selectEntered.AddListener(Interactable_SelectEntered);
        // interactable.selectEntered.AddListener((arg0) => Interactable_SelectEntered_Segment(AvatarPart.HEAD));

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
        // Cleanup the event for the XRI button so it does not get called after
        // we have been destroyed.
        // if (interactable)
        // {
        //     interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        // }
        // Remove the same listeners that were added.
        if (headInteractable != null) headInteractable.selectEntered.RemoveListener(headListener);
        if (torsoInteractable != null) torsoInteractable.selectEntered.RemoveListener(torsoListener);
        if (leftHandInteractable != null) leftHandInteractable.selectEntered.RemoveListener(leftHandListener);
        if (rightHandInteractable != null) rightHandInteractable.selectEntered.RemoveListener(rightHandListener);
        if (cubeInteractable != null) cubeInteractable.selectEntered.RemoveListener(cubeListener);

    }

    // private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    // {
    //     // The button has been pressed.

    //     // Change the local avatar prefab to the default one, because we have
    //     // a few costumes for that avatar bundled with Ubiq. The AvatarManager
    //     // will do the work of letting other peers know about the prefab change.
    //     //avatarManager.avatarPrefab = prefab; 

    //     // Also, set the texture to the texture of the model avatar
    //     SetAvatarTexture();
    // }

    private void Interactable_SelectEntered_Segment(AvatarPart avatarPart)
    {
        SetAvatarTexture(avatarPart);
    }

    private void SetAvatarTexture(AvatarPart avatarPart)
    {
        // Debug.Log("Setting avatar texture...");
        Debug.Log("Avatar part: " + avatarPart);

        // Get the model avatar that was interacted with
        GameObject selectedAvatar = gameObject; // The GameObject this script is attached to (ModelAvatar)

        // Get the TexturedModelAvatar component from the selected avatar
        var modelTexture = selectedAvatar.GetComponent<TexturedModelAvatar>();


        // Retrieve the texture from the model avatar
        // Texture2D stolenTexture = modelTexture.GetTexture();
        if (avatarPart == AvatarPart.FULLBODY)
        {
            SetFullBodyTexture();
            return;
        } else if (avatarPart == AvatarPart.BOTHHANDS)
        {
            SetAvatarTexture(AvatarPart.LEFTHAND);
            SetAvatarTexture(AvatarPart.RIGHTHAND);
            return;
        }
        Texture2D stolenTexture = modelTexture.GetTexture(avatarPart);
        Debug.Log("avatar part: " + avatarPart);
        Debug.Log("Stolen texture: " + stolenTexture);

        // Find the player's avatar
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);

        // Get the player's TexturedAvatar component
        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();

        var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();
        if (floatingAvatar != null){
            floatingAvatar.avatarPart = avatarPart;
        }

        // Apply the stolen texture to the player's avatar
        playerTexture.SetTexture(stolenTexture);

        Debug.Log("Texture successfully applied to the player!");
    }

    private void SetFullBodyTexture()
    {
        SetAvatarTexture(AvatarPart.HEAD);
        SetAvatarTexture(AvatarPart.TORSO);
        SetAvatarTexture(AvatarPart.LEFTHAND);
        SetAvatarTexture(AvatarPart.RIGHTHAND);
    }

}