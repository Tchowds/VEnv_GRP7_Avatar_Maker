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
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public PromptHelper promptHelper;
        
        private bool isRecording = false;
        private string _buffer;
        private Renderer cubeRenderer;  // For color change
        private Color initialColor;

        [SerializeField] private TextMeshPro resultText;

        private void Start()
        {
            cubeRenderer = GetComponent<Renderer>();  // Get the renderer component

            // Subscribe to events
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;

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
                Debug.Log("Stopped recording...");
                
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


            Debug.Log("Processing...");

            _buffer = "";
            

            if (resultText!=null){
                resultText.gameObject.SetActive(true);
            }   
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            
            sw.Stop();

            if (res == null)
            {
                Debug.Log("Recognition failed");
            }

            string output = res.Result;
            if(promptHelper != null) promptHelper.SetPrompt(output);
            
            
            long time = sw.ElapsedMilliseconds;
            float rate = recordedAudio.Length / (time * 0.001f);
            Debug.Log($"Time: {time} ms, Rate: {rate:F1}x");
            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }
        }

        private void OnProgressHandler(int progress)
        {
            Debug.Log($"Progress: {progress}%");
        }
    }
}
