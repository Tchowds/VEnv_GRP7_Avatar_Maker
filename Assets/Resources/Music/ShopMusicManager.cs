using UnityEngine;

public class ShopMusicManager : MonoBehaviour
{
    public AudioSource shopMusic; // Reference to the AudioSource for the music

    private bool isInsideShop = false; // Track whether the player is inside the shop

    void Start()
    {
        // Ensure the music is not playing when the game starts
        shopMusic.Stop();
    }

    void Update()
    {
        // If the player is inside the shop, play the music
        if (isInsideShop && !shopMusic.isPlaying)
        {
            Debug.Log("Playing shop music");
            shopMusic.Play();  // Play the music if it's not already playing
        }
        // If the player is outside the shop, stop the music
        else if (!isInsideShop && shopMusic.isPlaying)
        {
            Debug.Log("Stopping shop music");
            shopMusic.Stop();  // Stop the music if the player is outside the shop
        }
    }

    // Method to call when the player enters the shop
    public void EnterShop()
    {
        isInsideShop = true;
    }

    // Method to call when the player exits the shop
    public void ExitShop()
    {
        isInsideShop = false;
    }
}
