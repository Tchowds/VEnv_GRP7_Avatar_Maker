using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;

public class IpMenuSelector : MonoBehaviour
{
    private List<GameObject> keypadObjects;
    private List<UnityAction> keypadListeners;
    private Button recordButton;
    private TMP_Text ipInputField;
    private ApiRequestHandler apiRequestHandler;

    void Start()
    {
        keypadObjects = new List<GameObject>();
        keypadListeners = new List<UnityAction>();

        recordButton = transform.Find("RecordButton").GetComponent<Button>();
        ipInputField = transform.Find("IpInputField").GetComponent<TMP_Text>();

        var keypad = transform.Find("Keypad");
        for (int i = 0; i < keypad.childCount; i++)
        {
            var child = keypad.GetChild(i).gameObject;
            keypadObjects.Add(child);

            UnityAction listener = () => Interactable_SelectEntered_Enter_Key(child.name);
            keypadListeners.Add(listener);
            child.GetComponent<Button>().onClick.AddListener(listener);
        }
        apiRequestHandler = FindObjectOfType<ApiRequestHandler>();
        recordButton.onClick.AddListener(Interactable_SelectEntered_Record_Button);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < keypadObjects.Count; i++)
        {
            keypadObjects[i].GetComponent<Button>().onClick.RemoveListener(keypadListeners[i]);
        }
        recordButton.onClick.RemoveListener(Interactable_SelectEntered_Record_Button);
    }

    private void Interactable_SelectEntered_Enter_Key(string field)
    {
        string currentText = ipInputField.text;
        if (field == "<-" && currentText.Length > 0) ipInputField.text = currentText.Substring(0, currentText.Length - 1);
        else ipInputField.text = currentText + field;
    }

    private void Interactable_SelectEntered_Record_Button()
    {
        apiRequestHandler.SetIp(ipInputField.text);
    }
}
