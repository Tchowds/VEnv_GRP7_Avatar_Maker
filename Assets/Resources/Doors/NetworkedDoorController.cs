using System.Collections;
using UnityEngine;
using Ubiq.Messaging;

public class NetworkedDoorController : MonoBehaviour
{
    public Transform door;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float speed = 90f;
    [SerializeField] private float closeDelay = 3f;

    

    private float currentAngle;
    private bool isOpening = false;
    private float targetAngle = 0f;
    private bool doorOpen = false;

    private NetworkContext context;

    void Start()
    {
        context = NetworkScene.Register(this);
        currentAngle = closedAngle;
        if (context.Equals(default(NetworkContext)))
        {
            Debug.LogError($"{gameObject.name} failed to register with Ubiq!");
            return;
        }

        Debug.Log($"{gameObject.name} successfully registered with Ubiq. ");
    }

    void Update()
    {
        if (isOpening && currentAngle < targetAngle)
        {
            currentAngle += speed * Time.deltaTime;
            currentAngle = Mathf.Min(currentAngle, targetAngle);
        }
        else if (!isOpening && currentAngle > targetAngle)
        {
            currentAngle -= speed * Time.deltaTime;
            currentAngle = Mathf.Max(currentAngle, targetAngle);
        }

        door.rotation = Quaternion.Euler(0, currentAngle, 0);
    }

    public void ToggleDoorState()
    {
        isOpening = !isOpening;
        targetAngle = isOpening ? openAngle : closedAngle;
        doorOpen = isOpening;

        context.SendJson(new DoorMessage { isOpen = doorOpen });
        Debug.Log("Toggled");
        StopAllCoroutines();
        if (doorOpen)
        {
            StartCoroutine(AutoCloseDoor());
        }
    }

    private IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(closeDelay);

        if (doorOpen)
        {
            isOpening = false;
            targetAngle = closedAngle;
            doorOpen = false;

            context.SendJson(new DoorMessage { isOpen = false });
        }
    }

    private struct DoorMessage
    {
        public bool isOpen;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<DoorMessage>();

        isOpening = m.isOpen;
        targetAngle = isOpening ? openAngle : closedAngle;
        doorOpen = isOpening;

        Debug.Log($"🔄 Network Update: Door {(doorOpen ? "Opened" : "Closed")}");
    }
}

