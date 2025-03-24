    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using Ubiq.Messaging;
    using Ubiq.Rooms;

    public class PlayerExperienceController : MonoBehaviour
    {

        public ExperienceState currentState = ExperienceState.WaitingForPlayersToEnterShopFirstTime;
        
        public enum ExperienceState
        {
            WaitingForPlayersToEnterShopFirstTime,
            BothPlayersEnteredShopGetStarted,
            BothPlayersReturnedToShopFirstTime,
            BothPlayersAppliedSkinsToTheirMannequins
        }

        [Header("Audio/Dialogue")]
        public AudioSource tailorAudioSource;       // Assign via Inspector if you want voice lines
        public AudioClip waitingClip;              // “Oii, over here!” 
        public AudioClip bothPlayersInShopClip;    // Next line after both are inside

        public AudioClip endingClip;              // “You both did a great job!”

        private NetworkContext context;

        private string player1UID;
        private string player2UID;

        private List<PlayerState> playerStates = new List<PlayerState>();

        public int minPlayersForExperience = 2;

        // create a list of four elements
        private List<int> mannequinChanged = new List<int> { 0, 0, 0, 0 };

        void Start()
        {
            context = NetworkScene.Register(this);
            StartCoroutine(WaitingForPlayersToEnterFirstTimeCoroutine());
            Debug.Log("Tailor: Oii, over here! (waiting for both players to enter)");
        }

        // Update is called once per frame
        void Update()
        {
            if (currentState == ExperienceState.WaitingForPlayersToEnterShopFirstTime)
            {
                if (CheckMinNumPlayersInShop("TinkerTailorShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersEnteredShopGetStarted;
                    Debug.Log("Both players are in the shop, let's get started!");
                    if (tailorAudioSource && bothPlayersInShopClip)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(bothPlayersInShopClip);
                    }
                } else
                {
                    Debug.Log("Waiting for both players to enter the shop...");
                }
            } else if (currentState == ExperienceState.BothPlayersEnteredShopGetStarted)
            {
                if (checkIfBothPlayersAppliedSkinsToMannequins()){
                    currentState = ExperienceState.BothPlayersAppliedSkinsToTheirMannequins;
                    if (tailorAudioSource && endingClip){
                        Debug.Log("Both players have applied skins to their mannequins, experience is over!");
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(endingClip);
                    }
                }
                else{
                    Debug.Log("Waiting for both players to apply skins to their mannequins...");
                }
            }
        }

        public bool checkIfBothPlayersAppliedSkinsToMannequins()
        {
            bool bothPlayersAppliedSkins = true;
            
            for (int i = 0; i < mannequinChanged.Count; i++)
            {
                if (mannequinChanged[i] == 1)
                {
                    bothPlayersAppliedSkins = bothPlayersAppliedSkins && true;
                }
                else
                {
                    bothPlayersAppliedSkins = false;
                    break;
                }
            }
            return bothPlayersAppliedSkins;
        }

        public void UpdatePlayerLocation(PlayerLocationMessage locationMessage)
        {
            Debug.Log("PlayerExperienceController: ShopLocationMessage received - playerID: " + locationMessage.playerID + " shopName: " + locationMessage.shopName + " enterShop: " + locationMessage.inShop);
            bool found = false;
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].playerID == locationMessage.playerID)
                {
                    playerStates[i].location = locationMessage;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                playerStates.Add(new PlayerState { playerID = locationMessage.playerID, location = locationMessage, playerNum = -1, skinsSavedOnMannequins = 0 });
            }
            PrintPlayerStates();
        }

        public void UpdateMannequinSkin(int mannequinId)
        {   
            if (mannequinChanged[mannequinId] == 0)
            {
                Debug.Log("Mannequin " + mannequinId + " has been updated");
                mannequinChanged[mannequinId]++;
            }
            
        }
        public void SkinSavedOnMannequin(string playerID, int playerNum)
        {
            Debug.Log("Skin saved on mannequin for playerID: " + playerID + " playerNum: " + playerNum);
            var playerState = getPlayerState(playerID);
            playerState.skinsSavedOnMannequins++;

            if (playerState.playerNum == -1)
            {
                playerState.playerNum = playerNum;
                // Set the other player's playerNum to the opposite
                foreach (var otherPlayer in playerStates)
                {
                    if (otherPlayer.playerID != playerID && otherPlayer.playerNum == -1)
                    {
                        otherPlayer.playerNum = (playerNum == 1) ? 2 : 1;
                    }
                }
            }
            PrintPlayerStates();
        }

        public PlayerState getPlayerState(string playerID)
        {
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].playerID == playerID)
                {
                    return playerStates[i];
                }
            }
            return null;
        }



        private bool CheckMinNumPlayersInShop(string shop, int minNumPlayers)
        {
            Debug.Log("There are "+playerStates.Count+" players registered");

            int countPlayersInStore = 0;
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].location.shopName == shop && playerStates[i].location.inShop)
                {
                    countPlayersInStore++;
                }
            }
            return countPlayersInStore >= minNumPlayers;
        }

        private void PrintPlayerStates()
        {
            Debug.Log("Player States:");
            for (int i = 0; i < playerStates.Count; i++)
            {
                Debug.Log("PlayerID: " + playerStates[i].playerID + " inShop: " + playerStates[i].location.inShop + " shopName: " + playerStates[i].location.shopName + " playerNum: " + playerStates[i].playerNum + " skinsSavedOnMannequins: " + playerStates[i].skinsSavedOnMannequins);
            }
        }





        // State couroutine loops

        private IEnumerator WaitingForPlayersToEnterFirstTimeCoroutine()
        {
            // Continue looping until we exit the waiting state
            while (currentState == ExperienceState.WaitingForPlayersToEnterShopFirstTime)
            {
                // If we have an AudioSource and a waiting clip, play it
                if (tailorAudioSource && waitingClip)
                {
                    tailorAudioSource.PlayOneShot(waitingClip);
                }

                // Wait 20 seconds before playing again
                yield return new WaitForSeconds(20f);
            }

            Debug.Log("Exited waiting state, stopping waitingClip loop.");
        }

        // private IEnumerator WaitFor(float seconds)
        // {
        //     yield return new WaitForSeconds(seconds);
        // }
    }
