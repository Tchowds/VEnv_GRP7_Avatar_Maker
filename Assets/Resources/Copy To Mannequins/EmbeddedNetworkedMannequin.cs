using UnityEngine;
using Ubiq.Messaging;


public struct EmbeddedMessage
{
    public string texName;
    public int texId;
}

public class EmbeddedNetworkedMannequin : MonoBehaviour
{
    private NetworkContext context;
    private CopyToMannequin embeddedMannequin;

    private void Start()
    {
        context = NetworkScene.Register(this);
        embeddedMannequin = GetComponent<CopyToMannequin>();
    }

    public void ApplyEmbeddedSkin(int texId)
    {
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, tex.name);
        EmbeddedMessage message = new EmbeddedMessage
        {
            texName = tex.name,
            texId = texId
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var embeddedMessage = message.FromJson<EmbeddedMessage>();
        Texture2D tex = embeddedMannequin.textureCatalogue.Get(embeddedMessage.texId);
        embeddedMannequin.ApplyAndSave(tex, tex, tex, tex, false, embeddedMessage.texName);
    }
}
