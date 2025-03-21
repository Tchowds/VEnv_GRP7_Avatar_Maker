
using UnityEngine;
using TMPro;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using static SkinConstants;
using Ubiq.Messaging;
using Newtonsoft.Json.Linq; 

public class ApiRequestHandler : MonoBehaviour
{
    public string ipAddress;

    public string webServerAddress;

    private string serverURL;

    [Header("Dependencies")]
    public TexturedModelAvatar texturedModelAvatar;
    public CustomAvatarTextureCatalogue customAvatarTextureCatalogue;
    public DiffuseSkinToMannequinApplier skinManager;

    private HttpClient httpClient = new HttpClient();

    public CurtainManager curtainManager;

    private NetworkContext context;

    private void Start() {
        httpClient.Timeout = TimeSpan.FromSeconds(1800);
        context = NetworkScene.Register(this);
        // TODO - Ping the web server address to check if it is active, if not, drop down to the the IP address one
        if (string.IsNullOrEmpty(webServerAddress))
        {
            serverURL = $"http://{ipAddress}:8000";
        } else {
            serverURL = webServerAddress;
        }

        Debug.Log($"Server URL: {serverURL}");
    }

    public async void HandleRequest(List<string> recognizedText, RequestMode requestMode)
    {
        switch (requestMode)
        {
            case RequestMode.SelectSkin:
                await SendSkinSelectionRequest(recognizedText[0]);
                break;
            case RequestMode.GenerateSkin:
                await SendGenerateSkinRequest(recognizedText[0],recognizedText[1]);
                break;
            default:
                Debug.Log("No request mode selected.");
                break;
        }
    }

    public async Task<bool> PingServer()
    {
        try
        {
            var requestUrl = $"{serverURL}/ping";
            var response = await httpClient.PostAsync(requestUrl, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ping failed: {e.Message}");
            return false;
        }
    }

    private async Task SendSkinSelectionRequest(string query)
    {
        var requestUrl = $"{serverURL}/select_skin";

        var requestBody = new
        {
            skin_names = skinDescriptionToSkinFile.Keys,
            query = query
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SkinResponse>(responseString);

            if (result != null && skinDescriptionToSkinFile.ContainsKey(result.chosen_skin))
            {
                string filename = skinDescriptionToSkinFile[result.chosen_skin];

                if (SkinConstants.skinFileToID.TryGetValue(filename, out int skinId))
                {
                    texturedModelAvatar.SetTexture(texturedModelAvatar.Textures.Get(skinId));
                    Debug.Log($"Applied Skin: {filename}");
                }
                else
                {
                    Debug.Log("Skin ID not found");
                }
            }
            else
            {
                Debug.Log("Skin not found in map");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Skin Selection Request Failed: {e.Message}");
        }
        // Update skin search UI
        if (GameObject.Find("SkinSearchText") != null && GameObject.Find("SkinSearchText").GetComponent<TMP_Text>() != null)
        {
            GameObject.Find("SkinSearchText").GetComponent<TMP_Text>().text = "SKIN\nSEARCH";
        }
    }

       private async Task SendGenerateSkinRequest(string headPrompt, string torsoPrompt)
    {
        bool headPromptExists = !string.IsNullOrEmpty(headPrompt);
        bool torsoPromptExists = !string.IsNullOrEmpty(torsoPrompt);

        if (!headPromptExists && !torsoPromptExists)
        {
            Debug.LogWarning("No confirmed head or torso prompt available.");
            return;
        }

        curtainManager.showCurtain();

        try
        {
            if(headPromptExists && torsoPromptExists)
            {
                // For Both, call both endpoints

                // Request for Head:
                string endpointFace = $"{serverURL}/generate_skin_image_face";
                Debug.Log("Face endpoint: " + endpointFace);
                var requestBodyFace = new { prompt_face = headPrompt, num_images = 4 };
                string jsonBodyFace = JsonConvert.SerializeObject(requestBodyFace);
                var contentFace = new StringContent(jsonBodyFace, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpointFace} with parameters: {jsonBodyFace}");
                var responseFace = await httpClient.PostAsync(endpointFace, contentFace);
                responseFace.EnsureSuccessStatusCode();
                var responseStringFace = await responseFace.Content.ReadAsStringAsync();
                var resultFace = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseStringFace);

                // Request for Torso:
                string endpointTorso = $"{serverURL}/generate_skin_image_torso";
                var requestBodyTorso = new { prompt_torso = torsoPrompt, num_images = 4 };
                string jsonBodyTorso = JsonConvert.SerializeObject(requestBodyTorso);
                var contentTorso = new StringContent(jsonBodyTorso, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpointTorso} with parameters: {jsonBodyTorso}");
                var responseTorso = await httpClient.PostAsync(endpointTorso, contentTorso);
                responseTorso.EnsureSuccessStatusCode();
                var responseStringTorso = await responseTorso.Content.ReadAsStringAsync();
                var resultTorso = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseStringTorso);

                bool headSuccess = resultFace?.images_base64 != null;
                bool torsoSuccess = resultTorso?.images_base64 != null;

                if(headSuccess)
                {
                    List<string> textureUIDs = generateTextureUIDs(resultFace.images_base64.Count);
                    skinManager.DistributeAndApplySkins(resultFace.images_base64, textureUIDs, "face");
                }
                if(torsoSuccess)
                {
                    List<string> textureUIDs = generateTextureUIDs(resultTorso.images_base64.Count);
                    skinManager.DistributeAndApplySkins(resultTorso.images_base64, textureUIDs, "body");
                }
                
            }
            else if(headPromptExists)
            {
                // For Head, use the /generate_skin_image_face endpoint
                string endpoint = $"{serverURL}/generate_skin_image_face";
                var requestBody = new { prompt_face = headPrompt, num_images = 4 };
                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpoint} with parameters: {jsonBody}");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseString);

                if(result?.images_base64 != null)
                {
                    List<string> textureUIDs = generateTextureUIDs(result.images_base64.Count);
                    skinManager.DistributeAndApplySkins(result.images_base64, textureUIDs, "face");
                    Debug.Log("{result.images_base64.Count} head textures generated!");
                }
                else
                {
                    Debug.Log("No head images received.");
                }
            }
            else
            {
                // For Torso, use the /generate_skin_image_torso endpoint
                string endpoint = $"{serverURL}/generate_skin_image_torso";
                var requestBody = new { prompt_torso = torsoPrompt, num_images = 4 };
                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpoint} with parameters: {jsonBody}");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseString);

                if(result?.images_base64 != null)
                {
                    List<string> textureUIDs = generateTextureUIDs(result.images_base64.Count);
                    skinManager.DistributeAndApplySkins(result.images_base64, textureUIDs, "body");
                    Debug.Log($"{result.images_base64.Count} torso textures generated!");
                }
                else
                {
                    Debug.Log("No torso images received.");
                }
            }
            
        }
        catch(Exception e)
        {
            Debug.LogError($"Generate Skin Request Failed: {e.Message}");
        }
        curtainManager.hideCurtain();
    }

    private List<string> generateTextureUIDs(int count){
        List<string> textureUIDs = new List<string>();
        for (int i = 0; i < count; i++)
        {
            textureUIDs.Add(Guid.NewGuid().ToString());
        }
        return textureUIDs;
    }


    public void SetIp(string ip)
    {
        ipAddress = ip;
        sendMessage();
    }

    [Serializable]
    private class SkinResponse
    {
        public string chosen_skin;
    }

    [Serializable]
    private class GeneratedImagesResponse
    {
        public List<string> images_base64;
    }

    private struct IpMessage
    {
        public string ip;
    }


    public void sendMessage()
    {
        context.SendJson(new IpMessage
        {
            ip = ipAddress
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        //var m = message.FromJson<IpMessage>();
        //ipAddress = m.ip;
        JObject jsonMessage = JObject.Parse(message.ToString());
        if (jsonMessage.ContainsKey("ip"))
        {
            ipAddress = jsonMessage["ip"].ToString();
            Debug.Log($"Updated IP Address: {ipAddress}");

        }
    }
}
