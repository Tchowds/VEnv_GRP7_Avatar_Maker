using UnityEngine;
using Ubiq.Messaging;
using TMPro;


public struct EmbeddedMessage
{
    public string texName;
    public int texId;
    public string promptText;
}

public class EmbeddedNetworkedMannequin : MonoBehaviour
{
    public TMP_Text promptText;
    private NetworkContext context;
    private CopyToMannequin embeddedMannequin;

    private void Start()
    {
        context = NetworkScene.Register(this);
        embeddedMannequin = GetComponent<CopyToMannequin>();
    }

    public void ApplyEmbeddedSkin(int texId, string promptText)
    {
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, tex.name);
        EmbeddedMessage message = new EmbeddedMessage
        {
            texName = tex.name,
            texId = texId,
            promptText = promptText
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var embeddedMessage = message.FromJson<EmbeddedMessage>();
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(embeddedMessage.texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, embeddedMessage.texName);
        promptText.text = embeddedMessage.promptText;
    }
}
