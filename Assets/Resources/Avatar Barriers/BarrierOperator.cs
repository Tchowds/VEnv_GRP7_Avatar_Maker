using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Avatars;
using Newtonsoft.Json;


// This class matches the JSON structure.
[System.Serializable]
public class JsonDataWrapper 
{
    public int[][] head;
    public int[][] torso;
    public int[][] hands;
}

public class BarrierOperator : MonoBehaviour
{

    public TextAsset jsonFile;
    public bool dualAvatars = false;
    public float countdown = 5.0f;
    public int[] primaryAvatarParts = new int[4];
    public int[] secondaryAvatarParts = new int[4];


    private Dictionary<string, List<HashSet<int>>> overrides;
    private GameObject primaryAvatar;
    private GameObject SecondaryAvatar;
    private XRSimpleInteractable interactable;
    private NetworkContext context;
    private float lastDownTime = 0.0f;


    private struct BarrierMessage
    {
        public bool primaryActive;
        public bool secondaryActive;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactable = GetComponentInChildren<XRSimpleInteractable>();
        if (interactable) interactable.selectEntered.AddListener(Interactable_SelectEntered_Match_Avatar);
        
        primaryAvatar = transform.Find("PrimaryAvatar")?.gameObject;
        SecondaryAvatar = transform.Find("SecondaryAvatar")?.gameObject;

        if (dualAvatars)
        {
            primaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, -0.6f);
            SecondaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, 0.6f);
            setupAvatarTextures(SecondaryAvatar, secondaryAvatarParts);
        }
        else
        {
            SecondaryAvatar.SetActive(false);
            primaryAvatar.transform.localPosition = new Vector3(-0.2f, 0, 0);
        }
        setupAvatarTextures(primaryAvatar, primaryAvatarParts);

        parseJson();

        context = NetworkScene.Register(this);
    }

    public void parseJson()
    {
        // Use Newtonsoft.Json to deserialize the JSON
        JsonDataWrapper data = JsonConvert.DeserializeObject<JsonDataWrapper>(jsonFile.text);
        Debug.Log(data.head);

        overrides = new Dictionary<string, List<HashSet<int>>>();
        overrides.Add("head", new List<HashSet<int>>());
        overrides.Add("torso", new List<HashSet<int>>());
        overrides.Add("hands", new List<HashSet<int>>());

        foreach (int[] array in data.head)
        {
            HashSet<int> set = new HashSet<int>();
            foreach (int i in array)
                set.Add(i);
            overrides["head"].Add(set);
        }

        foreach (int[] array in data.torso)
        {
            HashSet<int> set = new HashSet<int>();
            foreach (int i in array)
                set.Add(i);
            overrides["torso"].Add(set);
        }

        foreach (int[] array in data.hands)
        {
            HashSet<int> set = new HashSet<int>();
            foreach (int i in array)
                set.Add(i);
            overrides["hands"].Add(set);
        }
    }

    void Update()
    {
        if (dualAvatars && (!primaryAvatar.activeSelf || !SecondaryAvatar.activeSelf))
        {
            if (Time.time - lastDownTime > countdown) setActiveAndSendMessage(true, true);
        }
    }

    void setupAvatarTextures(GameObject avatar, int[] avatarParts)
    {
    // Retrieve the required components
    var floating = avatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();
    var textured = avatar.GetComponent<TexturedAvatar>();

    // Define the avatar parts in the desired order.
    AvatarPart[] parts = { AvatarPart.HEAD, AvatarPart.TORSO, AvatarPart.LEFTHAND, AvatarPart.RIGHTHAND };

    // Loop through each part and set the corresponding texture.
    for (int i = 0; i < parts.Length; i++)
    {
        floating.avatarPart = parts[i];
        textured.SetTexture(avatarParts[i].ToString());
    }
    }

    private void Interactable_SelectEntered_Match_Avatar(SelectEnterEventArgs arg0)
    {
        var networkScene = NetworkScene.Find(this);
        var roomClient = networkScene.GetComponentInChildren<RoomClient>();
        var avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        var playerAvatar = avatarManager.FindAvatar(roomClient.Me);
        
        
        var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();
        var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

        var avatarCatalogue = playerTexture.Textures;

        if (dualAvatars)
        {
            if (primaryAvatar.activeSelf && SecondaryAvatar.activeSelf)
            {
                if (isSameAvatar(avatarCatalogue, secondaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(true, false);
                else if (isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, true);
            }
            else if (primaryAvatar.activeSelf && isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, false);
            else if (SecondaryAvatar.activeSelf && isSameAvatar(avatarCatalogue, secondaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, false);
            else setActiveAndSendMessage(false, false);
        }
        else
        {
            if (isSameAvatar(avatarCatalogue, primaryAvatarParts, floatingAvatar)) setActiveAndSendMessage(false, false);
        }
    }

    private void setActiveAndSendMessage(bool primary, bool secondary)
    {
        gameObject.SetActive(primary || secondary);
        primaryAvatar.SetActive(primary);
        SecondaryAvatar.SetActive(secondary);
        lastDownTime = Time.time;

        context.SendJson(new BarrierMessage()
        {
            primaryActive = primary,
            secondaryActive = secondary
        });
    }

    private bool isSameAvatar(AvatarTextureCatalogue catalogue, int[] modelParts, FloatingAvatarSeparatedTextures avatar)
    {
        Texture[] avatarTextures = { avatar.headRenderer.material.mainTexture,
                                     avatar.torsoRenderer.material.mainTexture,
                                     avatar.leftHandRenderer.material.mainTexture,
                                     avatar.rightHandRenderer.material.mainTexture };
        string[] order = { "head", "torso", "hands", "hands"};

        for (int i = 0; i < modelParts.Length; i++)
        {
            if (catalogue.Get(modelParts[i]) != avatarTextures[i] && !matchOverrides(catalogue, catalogue.Get(modelParts[i]), avatarTextures[i], overrides[order[i]])) return false;
        }

        return true;
    }

    private bool matchOverrides(AvatarTextureCatalogue catalogue, Texture modelTexture, Texture avatarTecture, List<HashSet<int>> overrideSets)
    {
        {
            int modelIndex = -1;
            int avatarIndex = -1;
            
            // Assuming catalogue.Textures is an array or list of Texture objects.
            for (int i = 0; i < catalogue.Textures.Count; i++)
            {
                if (catalogue.Textures[i] == modelTexture) modelIndex = i;
                if (catalogue.Textures[i] == avatarTecture) avatarIndex = i;
            };
            if (modelIndex == avatarIndex) return true;
            if (modelIndex == -1 || avatarIndex == -1) return false;

            foreach (HashSet<int> set in overrideSets)
            {
                if (set.Contains(modelIndex) && set.Contains(avatarIndex)) return true;
            }
            
            return false;
        }
    }

    private void OnDestroy()
    {
        if (interactable) interactable.selectEntered.RemoveListener(Interactable_SelectEntered_Match_Avatar);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<BarrierMessage>();
        bool remotePrimaryActive = m.primaryActive;
        bool remoteSecondaryActive = m.secondaryActive;
        if(!remotePrimaryActive && !remoteSecondaryActive) gameObject.SetActive(false);
        else
        {
            primaryAvatar.SetActive(remotePrimaryActive);
            SecondaryAvatar.SetActive(remoteSecondaryActive);
        }
    }
}
