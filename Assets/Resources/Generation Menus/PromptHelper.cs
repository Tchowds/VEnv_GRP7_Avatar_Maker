using UnityEngine;
using TMPro;
using Ubiq.Messaging;

public class PromptHelper : MonoBehaviour
{

    public struct PromptHelperMessage
    {
        public string torso;
        public string hand;
    }

    public TMP_Text torsoPrompt;
    public TMP_Text headPrompt;

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
    }

    public (string, string) getPrompts()
    {
        return (torsoPromptText, headPromptText);
    }

    public void sendMessage()
    {
        var message = new PromptHelperMessage
        {
            torso = torsoPromptText,
            hand = headPromptText
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
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
