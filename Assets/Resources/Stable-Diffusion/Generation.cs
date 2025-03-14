using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using System.IO;

public class Generation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private string apiKey = "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";
    private RoomClient roomClient;
    private AvatarManager avatarManager;
    private Renderer headRenderer;
    private Renderer torsoRenderer;
    private Renderer leftHandRenderer;
    private Renderer rightHandRenderer;

    public CustomAvatarTextureCatalogue textureCatalogue; 

    // private XRSimpleInteractable interactable;

    void Start()
{   var networkScene = NetworkScene.Find(this);
    roomClient = networkScene.GetComponentInChildren<RoomClient>();
    avatarManager = networkScene.GetComponentInChildren<AvatarManager>();

    // interactable = gameObject.GetComponent<XRSimpleInteractable>();
    // Texture2D originalTexture = playerTexture.GetTexture(AvatarPart.TORSO);


    string filename = $"{Application.persistentDataPath}/DALLE_Generated.png";
    Debug.Log("Starting generation");
    byte[] fileData = File.ReadAllBytes(filename);
    Debug.Log(fileData[0]);
    Texture2D newTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
    newTexture.LoadImage(fileData);
    Debug.Log("Texture loaded");
    onRoomJoined(newTexture);
}

private void onRoomJoined(Texture2D newTexture){
    StartCoroutine(update_texture_map(newTexture));
}
private IEnumerator update_texture_map(Texture2D newTexture)
{   
    yield return new WaitForSeconds(0.5f);
    var playerAvatar = avatarManager.FindAvatar(roomClient.Me);
    if (playerAvatar == null)
    {
        Debug.LogError("Player avatar not found");
    }

    // Get player's current texture
    var playerTexture = playerAvatar.GetComponent<TexturedAvatar>();
    if (playerTexture == null)
    {
        Debug.LogError("TexturedAvatar component not found on player avatar");
    }

    // Get the current texture
    Texture2D originalTexture = playerTexture.GetTexture();
    if (originalTexture == null)
    {
        Debug.LogError("Original texture not found");
    }

    var floatingAvatar = playerAvatar.GetComponentInChildren<FloatingAvatarSeparatedTextures>();

    // Get the player's textures
    Texture2D headTex = floatingAvatar.headRenderer.material.mainTexture as Texture2D;
    Texture2D torsoTex = floatingAvatar.torsoRenderer.material.mainTexture as Texture2D;
    Texture2D leftHandTex = floatingAvatar.leftHandRenderer.material.mainTexture as Texture2D;
    Texture2D rightHandTex = floatingAvatar.rightHandRenderer.material.mainTexture as Texture2D;

    floatingAvatar.headRenderer.material.mainTexture = newTexture;
    floatingAvatar.torsoRenderer.material.mainTexture = newTexture;
    floatingAvatar.leftHandRenderer.material.mainTexture = newTexture;
    floatingAvatar.rightHandRenderer.material.mainTexture = newTexture;

    textureCatalogue.AddDynamicTexture(newTexture);

    Debug.Log("Texture successfully applied to the player!");
    // playerTexture.SetTexture(newTexture, AvatarPart.TORSO);
    // Debug.Log("Texture successfully applied to the player!");

}

public IEnumerator PostRequest(string prompt)
{
    string url = "https://api.openai.com/v1/images/generations";

    // Corrected JSON structure for the chat completions endpoint
    string jsonData = "{\"model\": \"dall-e-2\", \"prompt\": \"" + prompt + "\", \"n\": 1, \"size\": \"1024x1024\"}";

    var request = new UnityWebRequest(url, "POST");
    byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    {
       
        Debug.Log("Error: " + request.error);
        Debug.LogError(request.error);
    }
    else
    {
        // Parse the response and log it
        string responseText = request.downloadHandler.text;
        Debug.Log("Response: " + responseText);

        // Parse the JSON response to extract the image URL
        var responseJson = JsonUtility.FromJson<DALLEImageResponse>(responseText);
        if (responseJson != null && responseJson.data.Length > 0)
        {
            string imageUrl = responseJson.data[0].url;
            Debug.Log("Image URL:*" + imageUrl + "*");

            yield return StartCoroutine(DownloadImage(imageUrl));
        }
    }
}

private IEnumerator DownloadImage(string imageUrl)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            // Send request and wait
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading texture: " + webRequest.error);
            }
            else
            {
                // Get the downloaded texture
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                Debug.Log("Texture downloaded successfully: " + downloadedTexture.width + "x" + downloadedTexture.height);
                
                // Save the texture (optional)
                SaveTextureToFile(downloadedTexture, "DALLE_Generated");
                
                // Apply to avatar or other renderer
                // ApplyTextureToAvatar(downloadedTexture);
            }
        }
    }

    private void SaveTextureToFile(Texture2D texture, string fileName)
    {
        try
        {
            byte[] pngBytes = texture.EncodeToPNG();
            
            // Create directory if it doesn't exist
            string directory = Application.persistentDataPath + "/GeneratedTextures/";
            
            // Save with timestamp
            string filePath = $"{Application.persistentDataPath}/{fileName}.png";
            File.WriteAllBytes(filePath, pngBytes);
            Debug.Log("Texture saved to: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save texture: " + e.Message);
        }
    }
}

[System.Serializable]
public class DALLEImageResponse
{
    public DALLEImageData[] data;
}

[System.Serializable]
public class DALLEImageData
{
    public string url;
}
