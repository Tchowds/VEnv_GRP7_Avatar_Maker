using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;

public class PlayerExperienceController : MonoBehaviour
{

    public ExperienceState currentState = ExperienceState.WaitingForPlayersToEnterShopFirstTIme;
    
    public enum ExperienceState
    {
        WaitingForPlayersToEnterShopFirstTIme,
        BothPlayersEnteredShopGetStarted,
        BothPlayersReturnedToShopFirstTime,
        BothPlayersAppliedSkinsToTheirMannequins
    }

    [Header("Audio/Dialogue")]
    public AudioSource tailorAudioSource;       // Assign via Inspector if you want voice lines
    public AudioClip waitingClip;              // “Oii, over here!” 
    public AudioClip bothPlayersInShopClip;    // Next line after both are inside

    private NetworkContext context;

    private string player1UID;
    private string player2UID;

    private List<PlayerLocationMessage> playerLocations = new List<PlayerLocationsMessage>();


    public struct PlayerLocationMessage
    {
        public string playerID;
        public bool inShop;
        public string shopName;
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        // if (tailorAudioSource && waitingClip)
        // {
        //     tailorAudioSource.PlayOneShot(waitingClip);
        // }

        Debug.Log("Tailor: Oii, over here! (waiting for both players to enter)");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdatePlayerLocations(PlayerLocationMessage locationMessage)
    {
        Debug.Log("PlayerExperienceController: ShopLocationMessage received - playerID: " + locationMessage.playerID + " shopName: " + locationMessage.shopName + " enterShop: " + locationMessage.enterShop);
        bool found = false;
        for (int i = 0; i < playerLocations.Count; i++)
        {
            if (playerLocations[i].playerID == locationMessage.playerID)
            {
                playerLocations[i] = locationMessage;
                found = true;
                break;
            }
        }
        if (!found)
        {
            playerLocations.Add(locationMessage);
        }
        PrintPlayerLocations();
    }

    void PrintPlayerLocations()
    {
        for (int i = 0; i < playerLocations.Count; i++)
        {
            Debug.Log("PlayerID: " + playerLocations[i].playerID + " inShop: " + playerLocations[i].inShop + " shopName: " + playerLocations[i].shopName);
        }
    }
}
