using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class DisappearBarrier : MonoBehaviour
{

    private XRSimpleInteractable interactable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactable = GetComponentInChildren<XRSimpleInteractable>();
        if (interactable) interactable.selectEntered.AddListener(Interactable_SelectEntered_Disappear);

    }

    private void Interactable_SelectEntered_Disappear(SelectEnterEventArgs arg0)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (interactable) interactable.selectEntered.RemoveListener(Interactable_SelectEntered_Disappear);
    }
}
