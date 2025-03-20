using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class PromptHelper : MonoBehaviour
{

    public struct PromptHelperMessage
    {
        public string torso;
        public string hand;
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
        sendPromptMessage();
    }

    public void clearHeadPrompt()
    {
        headPromptText = "";
        headPrompt.text = "Head Prompt: " + headPromptText;
        sendPromptMessage();
    }

    public void clearTorsoPrompt()
    {
        torsoPromptText = "";
        torsoPrompt.text = "Torso Prompt: " + torsoPromptText;
        sendPromptMessage();
    }

    public (string, string) getPrompts()
    {
        return (torsoPromptText, headPromptText);
    }

    public void sendPromptMessage()
    {
        var message = new PromptHelperMessage
        {
            torso = torsoPromptText,
            hand = headPromptText
        };
        context.SendJson(message);
        Debug.Log("prompt message: "+ message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log("prompt message received"+ message);
        var m = message.FromJson<PromptHelperMessage>();
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
