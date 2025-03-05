using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;


public class NetworkedBarrier : MonoBehaviour
{
    NetworkContext context;
    
    private bool lastActive;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        context = NetworkScene.Register(this);
        lastActive = gameObject.activeSelf;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastActive != gameObject.activeSelf)
        {
            lastActive = gameObject.activeSelf;
            context.SendJson(new BarrierMessage { barrierActive = lastActive });
        }
    }

    private struct BarrierMessage
    {
        public bool barrierActive;
    }



    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<BarrierMessage>();
        gameObject.SetActive(m.barrierActive);
    }
}
