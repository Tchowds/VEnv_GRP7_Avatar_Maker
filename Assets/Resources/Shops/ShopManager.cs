using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class ShopManager : MonoBehaviour
{
    public static ShopManager ActiveShop { get; private set; } // Tracks the currently active shop

    public AudioMixer globalAudioMixer; // One global mixer for all shops
    public string shopMusicParameter;  // Unique mixer parameter for this shop's music

    public PlayerExperienceController playerExperienceController;

    private NetworkContext context;

    private RoomClient roomClient;

    [SerializeField] protected float crowdVolumeInside = -50f;
    [SerializeField] protected float crowdVolumeOutside = -20f;
    [SerializeField] protected float shopVolumeInside = -20f;
    [SerializeField] protected float shopVolumeOutside = -50f;

    protected virtual void Start()
    {
        context = NetworkScene.Register(this);
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();

        if (ActiveShop == null)
        {
            globalAudioMixer.SetFloat("Crowd_Volume", crowdVolumeOutside);
            globalAudioMixer.SetFloat(shopMusicParameter, shopVolumeOutside); // Ensure this shop starts muted
            Debug.Log($"Muted shop audio: {shopMusicParameter}");
        }
    }

    public virtual void EnterShop()
    {

        // If another shop is active, reset it first
        if (ActiveShop != null && ActiveShop != this)
        {
            ActiveShop.ExitShop();  
        }

        ActiveShop = this;
        // Fade in shop music, fade out global crowd noise
        StartCoroutine(FadeSound("Crowd_Volume", crowdVolumeInside, 1f)); 
        StartCoroutine(FadeSound(shopMusicParameter, shopVolumeInside, 1f));
        Debug.Log($"Player {roomClient.Me.uuid} entered the shop: {gameObject.name}");
        playerExperienceController.UpdatePlayerLocation(new PlayerLocationMessage { playerID = roomClient.Me.uuid, shopName = gameObject.name, inShop=true });
        context.SendJson(new PlayerLocationMessage { playerID = roomClient.Me.uuid, shopName = gameObject.name, inShop=true });
    }

    public virtual void ExitShop()
    {
        Debug.Log($"{gameObject.name} - Exiting Shop");

        if (ActiveShop == this)
        {
            ActiveShop = null;
            StartCoroutine(FadeSound("Crowd_Volume", crowdVolumeOutside, 1f));
            StartCoroutine(FadeSound(shopMusicParameter, shopVolumeOutside, 1f));
        }
        //Debug.Log($"Player {roomClient.Me.uuid} exited the shop: {gameObject.name}");
        playerExperienceController.UpdatePlayerLocation(new PlayerLocationMessage { playerID = roomClient.Me.uuid, shopName = gameObject.name, inShop=false });
        context.SendJson(new PlayerLocationMessage { playerID = roomClient.Me.uuid, shopName = gameObject.name, inShop=false });
    }

    protected IEnumerator FadeSound(string parameterName, float targetVolume, float duration)
    {
        globalAudioMixer.GetFloat(parameterName, out float currentVolume);
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            globalAudioMixer.SetFloat(parameterName, Mathf.Lerp(currentVolume, targetVolume, t));
            yield return null;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<PlayerLocationMessage>();
        playerExperienceController.UpdatePlayerLocation(m);
    }
}
