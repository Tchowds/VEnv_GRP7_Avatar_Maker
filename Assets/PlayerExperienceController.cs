using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    private List<PlayerLocationMessage> playerLocations = new List<PlayerLocationMessage>();


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
        Debug.Log($"There are enough players in store to continue: {CheckMinNumPlayersInShop("TinkerTailor", 2)}");
    }

    public void UpdatePlayerLocation(PlayerLocationMessage locationMessage)
    {
        Debug.Log("PlayerExperienceController: ShopLocationMessage received - playerID: " + locationMessage.playerID + " shopName: " + locationMessage.shopName + " enterShop: " + locationMessage.inShop);
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

    private bool CheckMinNumPlayersInShop(string shop, int minNumPlayers)
    {
        int countPlayersInStore = 0;
        for (int i = 0; i < playerLocations.Count; i++)
        {
            if (playerLocations[i].shopName == shop && playerLocations[i].inShop)
            {
                countPlayersInStore++;
            }
        }
        return countPlayersInStore >= minNumPlayers;
    }

    private void PrintPlayerLocations()
    {
        Debug.Log("Player Locations:");
        for (int i = 0; i < playerLocations.Count; i++)
        {
            Debug.Log("PlayerID: " + playerLocations[i].playerID + " inShop: " + playerLocations[i].inShop + " shopName: " + playerLocations[i].shopName);
        }
    }
}
