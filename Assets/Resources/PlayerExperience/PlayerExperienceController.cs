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
        public AudioClip tailorInitialWaitingClip;              // “Oii, over here!” 
        public AudioClip tailorInstructionsOnceBothIn;    // Next line after both are inside
        public AudioClip swapStudioInstructions;   
        public AudioClip bothPlayersReturnTinkerTailorFirstTime;   
        public AudioClip diffusionDesignInstructions;
        public AudioClip bothPlayersReturnTinkerTailorSecondTime;   
        public AudioClip tailorEndingClip;              // “You both did a great job!”

        private NetworkContext context;

        private float updateInterval = 0.5f; // seconds
        private float nextUpdateTime = 0f;

        //private string player1UID;
        //private string player2UID;

        private List<PlayerState> playerStates = new List<PlayerState>();

        public int minPlayersForExperience = 2;


        void Start()
        {
            StartCoroutine(WaitingForPlayersToEnterFirstTimeCoroutine());
            Debug.Log("Tailor: Oii, over here! (waiting for both players to enter)");
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time < nextUpdateTime)
                return; 

            nextUpdateTime = Time.time + updateInterval;
            Debug.Log("Update!");

            if (currentState == ExperienceState.WaitingForPlayersToEnterShopFirstTime)
            {
                if (CheckMinNumPlayersInShop("TinkerTailorShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersEnteredShopGetStarted;
                    Debug.Log("Both players are in the shop, let's get started!");
                    if (tailorAudioSource && tailorInstructionsOnceBothIn)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(tailorInstructionsOnceBothIn);
                    }
                } else
                {
                    Debug.Log("Waiting for both players to enter the shop...");
                }
            } else if (currentState == ExperienceState.BothPlayersEnteredShopGetStarted)
            {
                if (checkIfBothPlayersAppliedSkinsToMannequins()){
                    currentState = ExperienceState.BothPlayersAppliedSkinsToTheirMannequins;
                    if (tailorAudioSource && tailorEndingClip){
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(tailorEndingClip);
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
            int numPlayersAppliedSkins = 0;
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].skinsSavedOnMannequins >= 2)
                {
                    numPlayersAppliedSkins++;
                }
            }

            if (numPlayersAppliedSkins < minPlayersForExperience){
                bothPlayersAppliedSkins = false;
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

            while (currentState == ExperienceState.WaitingForPlayersToEnterShopFirstTime)
            {
    
                if (tailorAudioSource && tailorInitialWaitingClip)
                {
                    tailorAudioSource.PlayOneShot(tailorInitialWaitingClip);
                }

                // Wait 20 seconds before playing again
                yield return new WaitForSeconds(20f);
            }

            Debug.Log("Exited waiting state, stopping waitingClip loop.");
        }

    }
