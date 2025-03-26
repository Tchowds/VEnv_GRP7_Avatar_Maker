using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// This class is responsible for allowing users to set the generation API endpoint IP address
/// It also pings the server to check if it is online and updates the status cube accordingly
/// </summary>
public class IpMenuSelector : MonoBehaviour
{
    // Keypad objects
    private List<GameObject> keypadObjects;
    private List<UnityAction> keypadListeners;
    private List<UnityAction<SelectEnterEventArgs>> keypadEventListeners;

    private Button recordButton;
    private TMP_Text ipInputField;
    private ApiRequestHandler apiRequestHandler;
    
    // Status cube
    public GameObject statusCube;
    public float pingTimePeriod = 5.0f;
    private Renderer cubeRenderer;

    void Start()
    {
        keypadObjects = new List<GameObject>();
        keypadListeners = new List<UnityAction>();
        keypadEventListeners = new List<UnityAction<SelectEnterEventArgs>>();

        recordButton = transform.Find("RecordButton").GetComponent<Button>();
        ipInputField = transform.Find("IpInputField").GetComponent<TMP_Text>();

        // Add listeners to all the keypad buttons
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

        apiRequestHandler = FindFirstObjectByType<ApiRequestHandler>();
        recordButton.onClick.AddListener(Interactable_SelectEntered_Record_Button);
        recordButton.transform.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((arg0) => Interactable_SelectEntered_Record_Button());
        
        // Initialize the status cube
        statusCube = transform.Find("StatusCube")?.gameObject;
        if (statusCube == null)
        {
            statusCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            statusCube.name = "StatusCube";
            statusCube.transform.SetParent(transform);
            statusCube.transform.localPosition = new Vector3(1.0f, 0.5f, 0);
            statusCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
        cubeRenderer = statusCube.GetComponent<Renderer>();
        cubeRenderer.sharedMaterial.color = Color.gray; // Default color

        StartCoroutine(PingServerPeriodically());
    }

    private IEnumerator PingServerPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(pingTimePeriod); 

            _ = PingAndUpdateColor();
        }
    }

    // Separate async method for pinging the server and updating the color based on the result
    private async Task PingAndUpdateColor()
    {
        bool pingResult = await apiRequestHandler.PingServer();
        if (cubeRenderer != null)
        {
            cubeRenderer.sharedMaterial.color = pingResult ? Color.green : Color.red;
            var indicator = statusCube.GetComponent<StatusIndicator>();
            if (indicator != null)
            {
                indicator.status = pingResult ? "Online" : "Offline";
            }
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

    // Keypad listener to input the IP address
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

    // Keypad listener for the record button to store the IP address
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
