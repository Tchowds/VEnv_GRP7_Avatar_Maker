using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IpMenuSelector : MonoBehaviour
{
    private List<GameObject> keypadObjects;
    private List<UnityAction> keypadListeners;
    private List<UnityAction<SelectEnterEventArgs>> keypadEventListeners;

    private Button recordButton;
    private TMP_Text ipInputField;
    private ApiRequestHandler apiRequestHandler;
    
    // Add cube references
    private GameObject statusCube;
    private Renderer cubeRenderer;

    void Start()
    {
        keypadObjects = new List<GameObject>();
        keypadListeners = new List<UnityAction>();
        keypadEventListeners = new List<UnityAction<SelectEnterEventArgs>>();

        recordButton = transform.Find("RecordButton").GetComponent<Button>();
        ipInputField = transform.Find("IpInputField").GetComponent<TMP_Text>();

        var keypad = transform.Find("Keypad");
        for (int i = 0; i < keypad.childCount; i++)
        {
            var child = keypad.GetChild(i).gameObject;
            keypadObjects.Add(child);

            UnityAction listener = () => Interactable_SelectEntered_Enter_Key(child.name);
            UnityAction<SelectEnterEventArgs> eventListener = (arg0) => Interactable_SelectEntered_Enter_Key(child.name);

            keypadListeners.Add(listener);
            keypadEventListeners.Add(eventListener);
            
            child.GetComponent<Button>().onClick.AddListener(listener);
            child.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(eventListener);
        }
        apiRequestHandler = FindObjectOfType<ApiRequestHandler>();
        recordButton.onClick.AddListener(Interactable_SelectEntered_Record_Button);
        recordButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => Interactable_SelectEntered_Record_Button());
        
        // Create or find the status cube
        statusCube = transform.Find("StatusCube")?.gameObject;
        if (statusCube == null)
        {
            statusCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            statusCube.name = "StatusCube";
            statusCube.transform.SetParent(transform);
            statusCube.transform.localPosition = new Vector3(1.0f, 0.5f, 0); // Position to the right of menu
            statusCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Make it smaller
        }
        cubeRenderer = statusCube.GetComponent<Renderer>();
        cubeRenderer.sharedMaterial.color = Color.gray; // Default color

        StartCoroutine(PingServerPeriodically());
    }

    private IEnumerator PingServerPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f); 

            _ = PingAndUpdateColor();
        }
    }

    // Separate async method for pinging
    private async Task PingAndUpdateColor()
    {
        bool pingResult = await apiRequestHandler.PingServer();
        if (cubeRenderer != null)
        {
            cubeRenderer.sharedMaterial.color = pingResult ? Color.green : Color.red;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < keypadObjects.Count; i++)
        {
            keypadObjects[i].GetComponent<Button>().onClick.RemoveListener(keypadListeners[i]);
            keypadObjects[i].GetComponent<XRSimpleInteractable>().selectEntered.RemoveListener(keypadEventListeners[i]);
        }
        recordButton.onClick.RemoveListener(Interactable_SelectEntered_Record_Button);
    }

    public void Interactable_SelectEntered_Enter_Key(string field)
    {
        string currentText = ipInputField.text;
        if (field == "<-" && currentText.Length > 0) 
        {
            ipInputField.text = currentText.Substring(0, currentText.Length - 1);
        } else if  (field == "<-" && currentText.Length == 0){
            ipInputField.text = "";
        } 
        else {
            ipInputField.text = currentText + field;
        }
        Debug.Log("Record:" + field);
    }

    public async void Interactable_SelectEntered_Record_Button()
    {
        apiRequestHandler.SetIp(ipInputField.text);
        
        bool pingResult = await apiRequestHandler.PingServer();
        if (cubeRenderer != null)
        {
            cubeRenderer.sharedMaterial.color = pingResult ? Color.green : Color.red;
        }
    }
}
