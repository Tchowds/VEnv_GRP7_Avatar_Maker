using UnityEngine;

public class CrowdNoiseManager : MonoBehaviour
{
    public AudioSource crowdNoise; // Reference to the AudioSource for the music

    private bool isOutside = true; // Track whether the player is outside

    void Start()
    {
        // Ensure the music is not playing when the game starts
        crowdNoise.Play();
    }

    void Update()
    {
        // If the player is inside the shop, play the music
        if (isOutside && !crowdNoise.isPlaying)
        {
            Debug.Log("Playing crowd noise");
            crowdNoise.Play();  // Play the music if it's not already playing
        }
        // If the player is outside the shop, stop the music
        else if (!isOutside && crowdNoise.isPlaying)
        {
            Debug.Log("Stopping shop music");
            crowdNoise.Stop();  // Stop the music if the player is outside the shop
        }
    }

    // Method to call when the player enters the shop
    public void EnterOutside()
    {
        isOutside = true;
    }

    // Method to call when the player exits the shop
    public void ExitOutside()
    {
        isOutside = false;
    }
}
