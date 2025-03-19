
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

    [Header("Dependencies")]
    public TexturedModelAvatar texturedModelAvatar;
    public CustomAvatarTextureCatalogue customAvatarTextureCatalogue;
    public DiffuseSkinToMannequinApplier skinManager;
    public SkinPart selectedSkinPart = SkinPart.Head;

    private HttpClient httpClient = new HttpClient();

    public RequestMode CurrentMode { get; set; } = RequestMode.GenerateSkin;
    public Animator curtainAnimator;

    private NetworkContext context;

    private void Start() {
        httpClient.Timeout = TimeSpan.FromSeconds(1800);
        context = NetworkScene.Register(this);
    }

    public async void HandleRequest(string recognizedText)
    {
        switch (CurrentMode)
        {
            case RequestMode.SelectSkin:
                await SendSkinSelectionRequest(recognizedText);
                break;
            case RequestMode.GenerateSkin:
                await SendGenerateSkinRequest(recognizedText);
                break;
            default:
                Debug.Log("No request mode selected.");
                break;
        }
    }

    private async Task SendSkinSelectionRequest(string query)
    {
        var requestUrl = $"http://{ipAddress}:8000/select_skin";

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
    }

       private async Task SendGenerateSkinRequest(string prompt)
    {
        curtainAnimator.SetTrigger("Show");
        SendCurtainState(true);

        try
        {
            if(selectedSkinPart == SkinConstants.SkinPart.Head)
            {
                // For Head, use the /generate_skin_image_face endpoint
                string endpoint = $"http://{ipAddress}:8000/generate_skin_image_face";
                var requestBody = new { prompt_face = prompt, num_images = 4 };
                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpoint} with parameters: {jsonBody}");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseString);

                if(result?.images_base64 != null)
                {
                    skinManager.DistributeAndApplySkins(result.images_base64, "face");
                    Debug.Log("{result.images_base64.Count} head textures generated!");
                }
                else
                {
                    Debug.Log("No head images received.");
                }
            }
            else if(selectedSkinPart == SkinConstants.SkinPart.Torso)
            {
                // For Torso, use the /generate_skin_image_torso endpoint
                string endpoint = $"http://{ipAddress}:8000/generate_skin_image_torso";
                var requestBody = new { prompt_torso = prompt, num_images = 4 };
                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpoint} with parameters: {jsonBody}");
                var response = await httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseString);

                if(result?.images_base64 != null)
                {
                    skinManager.DistributeAndApplySkins(result.images_base64, "body");
                    Debug.Log($"{result.images_base64.Count} torso textures generated!");
                }
                else
                {
                    Debug.Log("No torso images received.");
                }
            }
            else if(selectedSkinPart == SkinConstants.SkinPart.Both)
            {
                // For Both, call both endpoints

                // Request for Head:
                string endpointFace = $"http://{ipAddress}:8000/generate_skin_image_face";
                var requestBodyFace = new { prompt_face = prompt, num_images = 4 };
                string jsonBodyFace = JsonConvert.SerializeObject(requestBodyFace);
                var contentFace = new StringContent(jsonBodyFace, Encoding.UTF8, "application/json");

                Debug.Log($"Making request to: {endpointFace} with parameters: {jsonBodyFace}");
                var responseFace = await httpClient.PostAsync(endpointFace, contentFace);
                responseFace.EnsureSuccessStatusCode();
                var responseStringFace = await responseFace.Content.ReadAsStringAsync();
                var resultFace = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseStringFace);

                // Request for Torso:
                string endpointTorso = $"http://{ipAddress}:8000/generate_skin_image_torso";
                var requestBodyTorso = new { prompt_torso = prompt, num_images = 4 };
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
                    skinManager.DistributeAndApplySkins(resultFace.images_base64, "face");
                }
                if(torsoSuccess)
                {
                    skinManager.DistributeAndApplySkins(resultTorso.images_base64, "body");
                }
                
            }
            
        }
        catch(Exception e)
        {
            Debug.LogError($"Generate Skin Request Failed: {e.Message}");
        }
        curtainAnimator.SetTrigger("Hide");
        SendCurtainState(false);
    }

    private void SendCurtainState(bool show)
    {
        context.SendJson(new CurtainMessage
        {
            show = show
        });
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

    private struct CurtainMessage
{
    public bool show;
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

        } else {
            bool show = jsonMessage["show"].ToObject<bool>();
            if (show)
            {
                curtainAnimator.SetTrigger("Show");
                Debug.Log("Curtain dropped across all clients.");
            }
            else
            {
                curtainAnimator.SetTrigger("Hide");
                Debug.Log("Curtain raised across all clients.");
            }       
        }
    }
}
