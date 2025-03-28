using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class helps to manage prompt messages for the diffusion components
/// Specifically it manages the text and buttons for the torso and head prompts in the generation menu
/// These components are also networked to ensure that the prompt messages are consistent across peers
/// </summary>
public class PromptHelper : MonoBehaviour
{

    public struct PromptHelperMessage
    {
        public string torso;
        public string hand;
        public bool clearTorso;
        public bool clearHead;
    }

    public TMP_Text torsoPrompt;
    public TMP_Text headPrompt;

    private Button clearHeadPromptButton;
    private Button clearTorsoPromptButton;

    private string torsoPromptText;
    private string headPromptText;

    private SkinPartSelector skinPartSelector;
    private NetworkContext context;

    void Start()
    {
        torsoPromptText = "";
        headPromptText = "";
        skinPartSelector = GetComponent<SkinPartSelector>();
        context = NetworkScene.Register(this);

        clearHeadPromptButton = headPrompt.transform.GetComponentInChildren<Button>();
        clearTorsoPromptButton = torsoPrompt.transform.GetComponentInChildren<Button>();

        clearHeadPromptButton.onClick.AddListener(() => clearHeadPrompt());
        clearTorsoPromptButton.onClick.AddListener(() => clearTorsoPrompt());

        clearHeadPromptButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => clearHeadPrompt());
        clearTorsoPromptButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => clearTorsoPrompt());

    }

    // Applies the prompt based on what body part is selected in the skin part selector
    public void SetPrompt(string prompt)
    {
        if(skinPartSelector.skinPart == SkinConstants.SkinPart.Torso)
        {
            torsoPromptText = prompt;
            torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        }
        else if(skinPartSelector.skinPart == SkinConstants.SkinPart.Head)
        {
            headPromptText = prompt;
            headPrompt.text = "Head Prompt: " + headPromptText;
        }
        else
        {
            headPromptText = prompt;
            headPrompt.text = "Head Prompt: " + headPromptText;
            torsoPromptText = prompt;
            torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        }
        sendPromptMessage(false, false);
    }

    public void clearHeadPrompt()
    {
        headPromptText = "";
        headPrompt.text = "Head Prompt: " + headPromptText;
        sendPromptMessage(false, true);
    }

    public void clearTorsoPrompt()
    {
        torsoPromptText = "";
        torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        sendPromptMessage(true, false);
    }

    public (string, string) getPrompts()
    {
        return (torsoPromptText, headPromptText);
    }

    public void sendPromptMessage(bool clearTorso, bool clearHead)
    {
        var message = new PromptHelperMessage
        {
            torso = torsoPromptText,
            hand = headPromptText,
            clearTorso = clearTorso,
            clearHead = clearHead,
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<PromptHelperMessage>();
        if (m.clearTorso)
        {
            torsoPromptText = "";
            torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        }
        if (m.clearHead)
        {
            headPromptText = "";
            headPrompt.text = "Head Prompt: " + headPromptText;
        }
        if(m.torso != "")
        {
            torsoPromptText = m.torso;
            torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        }
        if(m.hand != "")
        {
            headPromptText = m.hand;
            headPrompt.text = "Head Prompt: " + headPromptText;
        }

    }

}
