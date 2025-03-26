    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using Ubiq.Messaging;
    using Ubiq.Rooms;
    using static SkinConstants;

    public class PlayerExperienceController : MonoBehaviour
    {

        public ExperienceState currentState = ExperienceState.WaitingForPlayersToEnterShopFirstTime;
        
        public enum ExperienceState
        {
            WaitingForPlayersToEnterShopFirstTime,
            BothPlayersEnteredShopGetStarted,
            BothPlayersEnteredSwapStudio,
            BothPlayersReturnedTinkerTailorFirstTime,
            bothPlayersAddedFirstSkinToMannequin,
            BothPlayersEnteredDiffusionDesign,
            BothPlayersReturnedTinkerTailorSecondTime,
            BothPlayersAppliedSkinsToBothTheirMannequinsEnding,

        }

        [Header("Audio Dialogue Clips")]
        public AudioSource tailorAudioSource;       // Assign via Inspector if you want voice lines
        public AudioClip tailorInitialWaitingClip;              // “Oii, over here!” 
        public AudioClip tailorInstructionsOnceBothIn;    // Next line after both are inside
        public AudioClip swapStudioInstructions;   
        public AudioClip bothPlayersReturnTinkerTailorFirstTime;   
        public AudioClip bothPlayersAddedFirstSkinToMannequin;
        public AudioClip diffusionDesignInstructions;
        public AudioClip bothPlayersReturnTinkerTailorSecondTime;   
        public AudioClip tailorEndingClip;              // “You both did a great job!”

        private NetworkContext context;
        private float updateInterval = 0.5f; // seconds
        private float nextUpdateTime = 0f;
        private RoomClient roomClient;

        private List<PlayerState> playerStates = new List<PlayerState>();

        public int minPlayersForExperience = 2;

        // create a list of four elements
        private List<int> mannequinChanged = new List<int> { 0, 0, 0, 0 };

        void Start()
        {
            roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
            SetTailorAudio(1f, 1f);
            StartCoroutine(WaitingForPlayersToEnterFirstTimeCoroutine());
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time < nextUpdateTime)
                return; 

            nextUpdateTime = Time.time + updateInterval;

            if (currentState == ExperienceState.WaitingForPlayersToEnterShopFirstTime)
            {
                if (CheckMinNumPlayersInShop("TinkerTailorShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersEnteredShopGetStarted;
                    Debug.Log("Both players are in the shop, let's get started!");
                    SetHeadTorsoSelectionForPlayers();
                    if (tailorAudioSource && tailorInstructionsOnceBothIn)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(tailorInstructionsOnceBothIn);
                    }
                } 
            } else if (currentState == ExperienceState.BothPlayersEnteredShopGetStarted)
            {
                if(CheckMinNumPlayersInShop("SwapStudioShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersEnteredSwapStudio;
                    Debug.Log("Both players are in the Swap Studio");
                    SetTailorAudio(0.2f, 0.25f);
                    if (tailorAudioSource && swapStudioInstructions)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(swapStudioInstructions);
                    }
                }
                else
                {
                    Debug.Log("Waiting for both players to enter the Swap Studio...");
                }
            } else if (currentState == ExperienceState.BothPlayersEnteredSwapStudio) {
                if (CheckMinNumPlayersInShop("TinkerTailorShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersReturnedTinkerTailorFirstTime;
                    Debug.Log("Both players are back in the Tinker Tailor shop return first time");
                    SetTailorAudio(1f, 1f);
                    if (tailorAudioSource && bothPlayersReturnTinkerTailorFirstTime)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(bothPlayersReturnTinkerTailorFirstTime);
                    }
                }
                else
                {
                    Debug.Log("Waiting for both players to return to the Tinker Tailor shop for first time...");
                }
            } if (currentState == ExperienceState.BothPlayersReturnedTinkerTailorFirstTime) {
                if (checkIfPlayersAppliedSkinsToFirstMannequins())
                {
                    currentState = ExperienceState.bothPlayersAddedFirstSkinToMannequin;
                    Debug.Log("Both players have added a skin to their mannequin");
                    SetTailorAudio(1f, 1f);
                    if (tailorAudioSource && bothPlayersAddedFirstSkinToMannequin)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(bothPlayersAddedFirstSkinToMannequin);
                    }
                }
                else
                {
                    Debug.Log("Waiting for both players to add their first skin to their mannequins");
                }
            }
            else if (currentState == ExperienceState.bothPlayersAddedFirstSkinToMannequin) 
            {
                if (CheckMinNumPlayersInShop("DiffusionDesignShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersEnteredDiffusionDesign;
                    Debug.Log("Both players are in the Diffusion Design shop");
                    SetTailorAudio(0.2f, 0.25f);
                    if (tailorAudioSource && diffusionDesignInstructions)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(diffusionDesignInstructions);
                    }
                }
                else
                {
                    Debug.Log("Waiting for both players to enter the Diffusion Design shop...");
                }
            } else if (currentState == ExperienceState.BothPlayersEnteredDiffusionDesign) {
                if (CheckMinNumPlayersInShop("TinkerTailorShopManager", minPlayersForExperience))
                {
                    currentState = ExperienceState.BothPlayersReturnedTinkerTailorSecondTime;
                    Debug.Log("Both players are back in the Tinker Tailor shop return second time");
                    SetTailorAudio(1f, 1f);
                    if (tailorAudioSource && bothPlayersReturnTinkerTailorSecondTime)
                    {
                        tailorAudioSource.Stop();
                        tailorAudioSource.PlayOneShot(bothPlayersReturnTinkerTailorSecondTime);
                    }
                }
                else
                {
                    Debug.Log("Waiting for both players to return to the Tinker Tailor shop for second time...");
                }
            } else if (currentState == ExperienceState.BothPlayersReturnedTinkerTailorSecondTime)
            {
                if (checkIfPlayersAppliedSkinsToBothMannequins()){
                    currentState = ExperienceState.BothPlayersAppliedSkinsToBothTheirMannequinsEnding;
                    Debug.Log("Both players have applied skins to both their mannequins, experience complete!");
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

        public bool checkIfPlayersAppliedSkinsToFirstMannequins()
        {
            bool player1Updated = mannequinChanged[0] == 1 || mannequinChanged[1] == 1;
            bool player2Updated = mannequinChanged[2] == 1 || mannequinChanged[3] == 1;

            if (playerStates.Count == 1)
            {
                // If only one player, allow either player 1 OR player 2 mannequin to be updated
                return player1Updated || player2Updated;
            }
            else if (playerStates.Count >= 2)
            {
                // Both players must have updated at least one of their mannequins
                return player1Updated && player2Updated;
            }

            return false;
        }

        public bool checkIfPlayersAppliedSkinsToBothMannequins()
        {
            bool player1Updated = mannequinChanged[0] == 1 && mannequinChanged[1] == 1;
            bool player2Updated = mannequinChanged[2] == 1 && mannequinChanged[3] == 1;

            if (playerStates.Count == 1)
            {
                // If only one player, allow either player 1 OR player 2 mannequin to be updated
                return player1Updated || player2Updated;
            }
            else if (playerStates.Count >= 2)
            {
                // Both players must have updated at least one of their mannequins
                return player1Updated && player2Updated;
            }

            return false;
        }

        public void UpdatePlayerLocation(PlayerLocationMessage locationMessage)
        {
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
        }

        public void UpdateMannequinSkin(int mannequinId)
        {   
            if (mannequinChanged[mannequinId] == 0)
            {
                mannequinChanged[mannequinId]++;
            }
        }
        public void SkinSavedOnMannequin(string playerID, int playerNum)
        {
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



        // When in tinker tailor or before we start, use spatial audio, when in other shops, dont.
        private void SetTailorAudio(float blendValue, float volume)
        {
            if (tailorAudioSource != null)
            {
                tailorAudioSource.spatialBlend = Mathf.Clamp01(blendValue); 
                tailorAudioSource.volume = Mathf.Clamp01(volume);
            }
        }


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

        public void SetHeadTorsoSelectionForPlayers() {
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].playerID == roomClient.Me.uuid)
                {
                    var skinPartSelector = Object.FindFirstObjectByType<SkinPartSelector>();
                    if (i % 2 == 0)
                    {
                        // player selection set to the head
                        skinPartSelector.InitialSetBodyPart(SkinConstants.SkinPart.Head);
                    } else {
                        // player selection set to the torso
                        skinPartSelector.InitialSetBodyPart(SkinConstants.SkinPart.Torso);
                    }
                }
            }
            
        }
    }
