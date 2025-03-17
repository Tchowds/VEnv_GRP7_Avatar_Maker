using UnityEngine;
using Whisper.Utils;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Whisper.Samples
{
    public class VoiceRecorder : MonoBehaviour
    {
        public ApiRequestHandler apiRequestHandler;
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public PromptHelper promptHelper;
        public TextMeshPro statusText;
        
        private bool isRecording = false;
        private string _buffer;
        private Renderer cubeRenderer;  // For color change
        private Color initialColor;
        
        public event Action<string> OnSpeechRecognized;

        private void Start()
        {
            cubeRenderer = GetComponent<Renderer>();  // Get the renderer component

            // Subscribe to events
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;
            OnSpeechRecognized += apiRequestHandler.HandleRequest;

            cubeRenderer = GetComponent<Renderer>();
            // Ensure there are at least 2 materials
            if(cubeRenderer.materials.Length > 1)
            {
                initialColor = cubeRenderer.materials[1].color;
            }
        }

        // Called on Select Entered event (press and hold)
        public void StartRecording()
        {
            if (!isRecording)
            {
                microphoneRecord.StartRecord();
                isRecording = true;
                if (statusText != null)
                    statusText.text = "Recording...";
                else
                    Debug.Log("Recording...");
                
                // Change the cube's color to green to indicate recording
                Material[] mats = cubeRenderer.materials;
                if (mats.Length > 1)
                {
                    mats[1].color = Color.green;
                }
                cubeRenderer.materials = mats;
            }
        }

        // Called on Select Exited event (release)
        public void StopRecording()
        {
            if (isRecording)
            {
                microphoneRecord.StopRecord();
                isRecording = false;
                if (statusText != null)
                    statusText.text = "Processing...";
                else
                    Debug.Log("Processing...");
                
                // Revert the cube's color
                Material[] mats = cubeRenderer.materials;
                if (mats.Length > 1)
                {
                    mats[1].color = initialColor;
                }
                cubeRenderer.materials = mats;
            }
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (statusText != null)
                statusText.text = "Processing...";
            else
                Debug.Log("Processing...");

            _buffer = "";

            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            sw.Stop();

            if (res == null)
            {
                if (statusText != null)
                    statusText.text = "Recognition failed";
                else
                    Debug.Log("Recognition failed");
                return;
            }

            string output = res.Result;
            if(promptHelper != null) promptHelper.SetPrompt(output);

            if (statusText != null)
                statusText.text = output;
            else
                Debug.Log(output);

            long time = sw.ElapsedMilliseconds;
            float rate = recordedAudio.Length / (time * 0.001f);
            Debug.Log($"Time: {time} ms, Rate: {rate:F1}x");

            OnSpeechRecognized?.Invoke(res.Result);
        }

        private void OnProgressHandler(int progress)
        {
            Debug.Log($"Progress: {progress}%");
        }
    }
}
