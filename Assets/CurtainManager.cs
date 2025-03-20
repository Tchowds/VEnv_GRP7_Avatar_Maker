using UnityEngine;
using Ubiq.Messaging;

public class CurtainManager : MonoBehaviour
{
    public Animator curtainAnimator;
    private NetworkContext context;

     private struct CurtainMessage
    {
        public bool show;
    }

    public void showCurtain() {
        curtainAnimator.SetTrigger("Show");
        SendCurtainState(true);
    }

    public void hideCurtain() {
        curtainAnimator.SetTrigger("Hide");
        SendCurtainState(false);
    }

    private void SendCurtainState(bool show)
    {
        context.SendJson(new CurtainMessage
        {
            show = show
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<CurtainMessage>();
        if (msg.show)
        {
            curtainAnimator.SetTrigger("Show");
        }
        else
        {
            curtainAnimator.SetTrigger("Hide");
        }
    }
}
