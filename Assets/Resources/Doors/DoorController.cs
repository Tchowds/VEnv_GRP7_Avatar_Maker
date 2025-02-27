using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System

public class DoorController : MonoBehaviour
{
    public Transform door;  // Reference to the door object
    [SerializeField] private float openAngle = 90f;  // Maximum door open angle (can be set in the Inspector)
    [SerializeField] private float closedAngle = 0f; // Default closed angle (can be set in the Inspector)
    [SerializeField] private float speed = 90f;  // Speed at which the door opens and closes

    private float currentAngle = 0f;  // Current angle of the door

    private bool isOpening = false;   // Whether the door is in the process of opening
    private float targetAngle = 0f;   // The target angle for the door to reach

    public InputActionReference openCloseAction; // Input Action Reference for the controller button (A button)

    void OnEnable()
    {
        // Enable the input action when the script is enabled
        openCloseAction.action.Enable();
    }

    void OnDisable()
    {
        // Disable the input action when the script is disabled
        openCloseAction.action.Disable();
    }

    void Update()
    {
        // Check for either keyboard input (E key) or controller input (Action button)
        bool isButtonPressed = Keyboard.current.eKey.wasPressedThisFrame || openCloseAction.action.triggered;

        if (isButtonPressed) // If either input was triggered
        {
            Debug.Log("Button Pressed!");
            isOpening = !isOpening;  // Toggle the door open/close state
            targetAngle = isOpening ? openAngle : closedAngle;  // Set the target angle based on state
        }

        // Animate the door opening or closing
        if (isOpening && currentAngle < targetAngle)
        {
            currentAngle += speed * Time.deltaTime;  // Door speed (adjust this number)
            currentAngle = Mathf.Min(currentAngle, targetAngle);  // Clamp to target
        }
        else if (!isOpening && currentAngle > targetAngle)
        {
            currentAngle -= speed * Time.deltaTime;  // Door speed (adjust this number)
            currentAngle = Mathf.Max(currentAngle, targetAngle);  // Clamp to target
        }

        Debug.Log("Current Door Angle: " + currentAngle);
        // Apply the rotation to the door based on the current angle
        door.rotation = Quaternion.Euler(0, currentAngle, 0);  // Rotate the door along the Y-axis
    }
}
