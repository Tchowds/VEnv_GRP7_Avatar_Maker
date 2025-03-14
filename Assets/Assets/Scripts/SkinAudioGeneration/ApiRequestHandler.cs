
using UnityEngine;
using TMPro;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using static SkinConstants;

public class ApiRequestHandler : MonoBehaviour
{
    public string ipAddress;

    [Header("Dependencies")]
    public TexturedModelAvatar texturedModelAvatar;
    public CustomAvatarTextureCatalogue customAvatarTextureCatalogue;
    public TextMeshPro resultTextMesh;
    public DiffuseSkinToMannequinApplier skinManager;

    private HttpClient httpClient = new HttpClient();

    public RequestMode CurrentMode { get; set; } = RequestMode.None;

    private void Start() {
        httpClient.Timeout = TimeSpan.FromSeconds(180);
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
                resultTextMesh.text = "No request mode selected.";
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
                    resultTextMesh.text = $"Applied Skin: {filename}";
                }
                else
                {
                    resultTextMesh.text = "Skin ID not found";
                }
            }
            else
            {
                resultTextMesh.text = "Skin not found in map";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Skin Selection Request Failed: {e.Message}");
            resultTextMesh.text = "Skin Selection Failed";
        }
    }

    private async Task SendGenerateSkinRequest(string prompt)
    {
        var requestUrl = $"http://{ipAddress}:8000/generate_skin_image";

        var body_part = "face";
        var promptLower = prompt.ToLower();
        if (!promptLower.Contains("face") && !promptLower.Contains("head") && !promptLower.Contains("mask"))
        {
            body_part = "body";
        }
        Debug.Log($"Body Part: {body_part}");
        var requestBody = new { prompt = prompt, num_images = 4, body_part = body_part};
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GeneratedImagesResponse>(responseString);

            if (result?.images_base64 != null)
            {
                skinManager.ApplyGeneratedSkins(result.images_base64, body_part);
                resultTextMesh.text = $"{result.images_base64.Count} textures generated!";
            }
            else
            {
                resultTextMesh.text = "No images received.";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Generate Skin Request Failed: {e.Message}");
            resultTextMesh.text = "Generation Request Failed";
        }
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
}
