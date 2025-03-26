using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using Ubiq.Messaging;
using Ubiq.Rooms;
using static SkinConstants;

/// <summary>
/// The class is responsible for selecting the body part modde for the skin generation process
/// It also handles pinging the external API for use in the skin generation process
/// </summary>
public class SkinPartSelector : MonoBehaviour
{
    public struct SkinPartSelectorMessage
    {
        public string part;
    }

    public SkinConstants.SkinPart skinPart;

    public Button headButton;
    public Button torsoButton;
    public Button bothButton;
    public Button generateButton;

    public ApiRequestHandler apiRequestHandler;

    public GameObject statusCube;

    
    public PromptHelper promptHelper;

    // This text objects is used to display the current endpoint at the top of the menu
    public TMP_Text ip_text;

    private Color selectedColor = Color.red;
    private Color normalColor = Color.white;    

    private NetworkContext context;
    void Start()
    {

        headButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Head));
        torsoButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Torso));
        bothButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Both));

        generateButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnGenerateButtonPressed());

        context = NetworkScene.Register(this);

        OnButtonPressed(skinPart);
        StartCoroutine(CheckEndpointStatus());
    }

    // Every second pings the endpoint to see if its valid and working
    private IEnumerator CheckEndpointStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f); 

            UpdateEndpointStatus();
        }
    }

    // Use the statusCube to find the status of the endpoint and display it
    private void UpdateEndpointStatus()
    {
    if (statusCube != null)
        {
            var indicator = statusCube.GetComponent<StatusIndicator>();
            if (indicator != null && indicator.status == "Online")
            {
                ip_text.text = "Connected to Generation Endpoint: " + apiRequestHandler.getServerURL();
            }
            else
            {
                ip_text.text = "Connection failed: " + apiRequestHandler.getServerURL();
            }
        }
    }

    void OnButtonPressed(SkinConstants.SkinPart part)
    {
        skinPart = part;
        sendMessage();
        UpdateButtonVisuals();
    }

    // Used by player experience to set the initial body part for each player
    public void InitialSetBodyPart(SkinConstants.SkinPart part)
    {
        skinPart = part;
        sendMessage();
        UpdateButtonVisuals();
    }

    // Ensure all the buttons match the selection of the skin part
    void UpdateButtonVisuals()
    {
        // Change each button's color based on whether it is the current selection.
        UpdateButtonColor(headButton, skinPart == SkinConstants.SkinPart.Head);
        UpdateButtonColor(torsoButton, skinPart == SkinConstants.SkinPart.Torso);
        UpdateButtonColor(bothButton, skinPart == SkinConstants.SkinPart.Both);
    }

    void UpdateButtonColor(Button button, bool isSelected)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = isSelected ? selectedColor : normalColor;
        button.colors = colors;
    }

    public void sendMessage()
    {
        SkinPartSelectorMessage message = new SkinPartSelectorMessage
        {
            part = skinPart.ToString()
        };
        context.SendJson(message);
    }

    // Keeps the button selections in sync, players should see opposing selections unless they're selecting both
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        SkinPartSelectorMessage msg = message.FromJson<SkinPartSelectorMessage>();
        skinPart = (SkinConstants.SkinPart)Enum.Parse(typeof(SkinConstants.SkinPart), msg.part);
        
        if (skinPart == SkinConstants.SkinPart.Head) skinPart = SkinConstants.SkinPart.Torso;
        else if (skinPart == SkinConstants.SkinPart.Torso) skinPart = SkinConstants.SkinPart.Head;

        UpdateButtonVisuals();
    }

    // When the generate button is pressed, call the generation process
    public void OnGenerateButtonPressed()
    {
        var (torsoPrompt, headPrompt) = promptHelper.getPrompts(); 
        List<string> prompts = new List<string> { headPrompt, torsoPrompt };    
        apiRequestHandler.HandleRequest(prompts, RequestMode.GenerateSkin);
    }
}
