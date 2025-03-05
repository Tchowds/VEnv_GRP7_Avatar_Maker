using UnityEngine;
using System.Collections;


public class DoorController : MonoBehaviour
{
    public Transform door;  // Reference to the door object
    [SerializeField] private float openAngle = 90f;  // Maximum door open angle (can be set in the Inspector)
    [SerializeField] private float closedAngle = 0f; // Default closed angle (can be set in the Inspector)
    [SerializeField] private float speed = 90f;  // Speed at which the door opens and closes
    [SerializeField] private float closeDelay = 3f;  // Time to wait before closing the door automatically

    private float currentAngle = 0f;  // Current angle of the door
    private bool isOpening = false;   // Whether the door is in the process of opening
    private float targetAngle = 0f;   // The target angle for the door to reach
    private bool doorOpen = false;    // To track if the door is currently open

    void Start()
    {
        // Set the initial door angle to the closed angle
        currentAngle = closedAngle;
    }

    void Update()
    {
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

        // Apply the rotation to the door based on the current angle
        door.rotation = Quaternion.Euler(0, currentAngle, 0);
    }

    // Toggle door state between open and closed
    public void ToggleDoorState()
    {
        Debug.Log("Toggling door state!");
        isOpening = !isOpening;
        targetAngle = isOpening ? openAngle : closedAngle;  // Toggle between open and closed
        doorOpen = isOpening;

        // Stop any existing coroutine and start the auto-close coroutine
        StopAllCoroutines();
        if (doorOpen)
        {
            StartCoroutine(AutoCloseDoor());
        }
    }

    // Coroutine to automatically close the door after a set delay
    private IEnumerator AutoCloseDoor()
    {
        // Wait for the specified duration (closeDelay)
        yield return new WaitForSeconds(closeDelay);

        // Automatically close the door if it hasn't been manually closed yet
        if (doorOpen)
        {
            Debug.Log("Automatically closing the door.");
            isOpening = false;
            targetAngle = closedAngle;
            doorOpen = false;
        }
    }
}
