using UnityEngine;

public class boppingScript : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f; // How high the object will bob
    [SerializeField] private float frequency = 1.0f; // Complete cycle per second
    
    private Vector3 startPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store the initial position
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + amplitude * Mathf.Sin(Time.time * frequency * 2 * Mathf.PI);
        
        // Update the position
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
