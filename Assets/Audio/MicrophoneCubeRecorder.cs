using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Whisper.Utils;
using Newtonsoft.Json;
using static SkinConstants;

namespace Whisper.Samples
{
    public class MicrophoneCubeRecorder : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;

        [Header("Cube Interaction")]
        public TextMeshPro textMesh; // World-space text above cube
        public TextMeshPro resultTextMesh; // Text mesh for showing the chosen skin
        public TexturedModelAvatar texturedModelAvatar;

        public string ip_address;

        private bool isRecording = false;
        private string _buffer;

        private HttpClient httpClient = new HttpClient();

        private void Start()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;
        }

        // Function triggered by clicking the cube in VR
        public void ToggleRecording()
        {
            if (!isRecording)
            {
                microphoneRecord.StartRecord();
                isRecording = true;
                textMesh.text = "Recording...";
            }
            else
            {
                microphoneRecord.StopRecord();
                isRecording = false;
                textMesh.text = "Processing...";
            }
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            textMesh.text = "Processing...";
            _buffer = "";

            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null) return;

            var speechResult = res.Result;
            //if (printLanguage)
            //    speechResult += $"\n(Language: {res.Language})";

            textMesh.text = speechResult; // Display speech-to-text result

            await SendSkinSelectionRequest(res.Result);
        }

        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments) return;
            _buffer += segment.Text;
            textMesh.text = _buffer + "...";
        }

        private void OnProgressHandler(int progress)
        {
            textMesh.text = $"Progress: {progress}%";
        }

        private async Task SendSkinSelectionRequest(string query)
        {
            var requestUrl = "http://" + ip_address + ":8000/select_skin";

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
                UnityEngine.Debug.LogError($"HTTP Request Failed: {e.Message}");
                resultTextMesh.text = "Request Failed";
            }
        }

        [Serializable]
        private class SkinResponse
        {
            public string chosen_skin;
        }
    }
}
