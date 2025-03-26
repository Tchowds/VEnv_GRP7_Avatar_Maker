using UnityEngine;
using Ubiq.Messaging;
using TMPro;

// Due to output of word embeddings only being from the base catalogue we can just give the index of the texture
public struct EmbeddedMessage
{
    public string texName;
    public int texId;
    public string promptText;
}

/// <summary>
/// This class is attached to a Manniquin prefab object and is responsible for networking 
/// the skin state after running the word embedding skin selection process.
/// This also handles the word embedding prompt text networking
/// </summary>
public class EmbeddedNetworkedMannequin : MonoBehaviour
{
    public TMP_Text promptText;
    private NetworkContext context;
    private CopyToMannequin embeddedMannequin;

    private void Start()
    {
        context = NetworkScene.Register(this);
        // We use CopyToMannequin to apply the skins
        embeddedMannequin = GetComponent<CopyToMannequin>();
    }

    // This class can be used on the output of the word embedding skin selection subprocess to apply the skin in a networked fashion
    public void ApplyEmbeddedSkin(int texId, string promptText)
    {
        // texId will be in the base catalogue so should be the same acoss peers
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, tex.name);
        // If skin changes, this needs to be updated across the network
        EmbeddedMessage message = new EmbeddedMessage
        {
            texName = tex.name,
            texId = texId,
            promptText = promptText
        };
        context.SendJson(message);
    }

    // Decodes and applies the skin from the networked message
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var embeddedMessage = message.FromJson<EmbeddedMessage>();
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(embeddedMessage.texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, embeddedMessage.texName);
        promptText.text = embeddedMessage.promptText;
    }
}
