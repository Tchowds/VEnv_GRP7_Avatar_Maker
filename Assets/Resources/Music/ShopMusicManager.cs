using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class ShopMusicManager : MonoBehaviour
{
    public AudioMixer audioMixer; 


    void Start()
    {
        // Set the crowd sound as the default
        audioMixer.SetFloat("CrowdVolume", -15);  // Loudest (adjust to your needs)
        audioMixer.SetFloat("ShopVolume", -50); // Mute or set to a very low level

       
    }

    public void EnterShop()
    {
        // Fade in shop sound and fade out crowd sound
        StartCoroutine(FadeSound("CrowdVolume", -40, 1f)); // Fade out crowd
        StartCoroutine(FadeSound("ShopVolume", -15, 1f));   // Fade in shop music
    }

    public void ExitShop()
    {
        // Fade in crowd sound and fade out shop music
        StartCoroutine(FadeSound("CrowdVolume", -15, 1f));   // Fade in crowd
        StartCoroutine(FadeSound("ShopVolume", -50, 1f));  // Fade out shop music
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
