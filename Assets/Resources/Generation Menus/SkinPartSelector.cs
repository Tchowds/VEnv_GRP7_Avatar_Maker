using System;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Messaging;
using Ubiq.Rooms;
using static SkinConstants;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

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
    public PromptHelper promptHelper;

    public TMP_Text ip_text;

    // Define your selected and normal colors.
    // You can adjust these as needed.
    private Color selectedColor = Color.red;
    private Color normalColor = Color.white;    

    private NetworkContext context;
    void Start()
    {
        // Add click listeners to each button.
        headButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Head));
        torsoButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Torso));
        bothButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Both));

        headButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Head));
        torsoButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Torso));
        bothButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnButtonPressed(SkinConstants.SkinPart.Both));

        generateButton.onClick.AddListener(OnGenerateButtonPressed);
        generateButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => OnGenerateButtonPressed());

        context = NetworkScene.Register(this);

        // Optionally, initialize the visuals with a default selection.
        OnButtonPressed(skinPart);

    }

    void Update()
    {
        ip_text.text = "Generation Endpoint API Ip:" + apiRequestHandler.ipAddress;
    }

    void OnButtonPressed(SkinConstants.SkinPart part)
    {
        skinPart = part;
        sendMessage();
        incurSkinPart();
    }

    public void ButtonOnClick(string part)
    {
        skinPart = (SkinConstants.SkinPart)Enum.Parse(typeof(SkinConstants.SkinPart), part);
        OnButtonPressed(skinPart);
    }

    void incurSkinPart()
    {
        UpdateButtonVisuals();
        if (apiRequestHandler) apiRequestHandler.selectedSkinPart = skinPart;
    }

    void UpdateButtonVisuals()
    {
        // Change each button's color based on whether it is the current selection.
        UpdateButtonColor(headButton, skinPart == SkinConstants.SkinPart.Head);
        UpdateButtonColor(torsoButton, skinPart == SkinConstants.SkinPart.Torso);
        UpdateButtonColor(bothButton, skinPart == SkinConstants.SkinPart.Both);
    }

    void UpdateButtonColor(Button button, bool isSelected)
    {
        // Get the current ColorBlock settings.
        ColorBlock colors = button.colors;
        // Set the normal (idle) color to selected or normal.
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

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        SkinPartSelectorMessage msg = message.FromJson<SkinPartSelectorMessage>();
        skinPart = (SkinConstants.SkinPart)Enum.Parse(typeof(SkinConstants.SkinPart), msg.part);
        
        if (skinPart == SkinConstants.SkinPart.Head) skinPart = SkinConstants.SkinPart.Torso;
        else if (skinPart == SkinConstants.SkinPart.Torso) skinPart = SkinConstants.SkinPart.Head;

        incurSkinPart();
    }

    public void OnGenerateButtonPressed()
    {
        var (torsoPrompt, headPrompt) = promptHelper.getPrompts();

        if (string.IsNullOrEmpty(torsoPrompt) && string.IsNullOrEmpty(headPrompt))
        {
            Debug.LogWarning("No confirmed prompt available.");
            return;
        }
        
        string confirmedPromptText = "";

        if (apiRequestHandler.CurrentMode == RequestMode.GenerateSkin)
        {
            if (skinPart == SkinConstants.SkinPart.Head || skinPart == SkinConstants.SkinPart.Both)
            {
                confirmedPromptText = headPrompt;
            }
            else if (skinPart == SkinConstants.SkinPart.Torso)
            {
                confirmedPromptText = torsoPrompt;
            }
            
            apiRequestHandler.HandleRequest(confirmedPromptText);
        }
        else
        {
            Debug.LogWarning("Unhandled API Request Mode: " + apiRequestHandler.CurrentMode);
        }
    }
}
