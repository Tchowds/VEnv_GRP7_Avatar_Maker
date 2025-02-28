using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class ShopMusicManager : MonoBehaviour
{
    public AudioMixer audioMixer; 


    void Start()
    {
        // Set the crowd sound as the default
        audioMixer.SetFloat("CrowdVolume", 0);  // Loudest (adjust to your needs)
        audioMixer.SetFloat("ShopVolume", -30); // Mute or set to a very low level

       
    }

    public void EnterShop()
    {
        // Fade in shop sound and fade out crowd sound
        StartCoroutine(FadeSound("CrowdVolume", -20, 1f)); // Fade out crowd
        StartCoroutine(FadeSound("ShopVolume", 0, 1f));   // Fade in shop music
    }

    public void ExitShop()
    {
        // Fade in crowd sound and fade out shop music
        StartCoroutine(FadeSound("CrowdVolume", 0, 1f));   // Fade in crowd
        StartCoroutine(FadeSound("ShopVolume", -30, 1f));  // Fade out shop music
    }

    private IEnumerator FadeSound(string parameterName, float targetVolume, float duration)
    {
        float currentVolume = 0;
        audioMixer.GetFloat(parameterName, out currentVolume);

        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            audioMixer.SetFloat(parameterName, Mathf.Lerp(currentVolume, targetVolume, t));
            yield return null;
        }
    }
}
