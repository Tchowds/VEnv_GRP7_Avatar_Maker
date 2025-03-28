using UnityEngine;
using Whisper.Utils;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using System.Diagnostics;
using Ubiq.Messaging;
using Debug = UnityEngine.Debug;
using static SkinConstants;
using System.Collections.Generic;

namespace Whisper.Samples
{
    public class VoiceRecorder : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public PromptHelper promptHelper;
        public SkinConstants.RequestMode requestMode;
        public ApiRequestHandler apiRequestHandler;
        
        private bool isRecording = false;
        private string _buffer;
        private Renderer cubeRenderer;  // For color change
        private Color initialColor;
        private NetworkContext context;

        [SerializeField] private TextMeshPro resultText;
        private const string DEFAULT_TEXT = "Press and hold to record";
        private const string PROCESSING_TEXT = "Processing...";
        private bool wasActiveRecorder = false;

        private void Start()
        {
            cubeRenderer = GetComponent<Renderer>();  // Get the renderer component
            context = NetworkScene.Register(this);
            
            // Subscribe to events
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;

            cubeRenderer = GetComponent<Renderer>();
            // Ensure there are at least 2 materials
            if(cubeRenderer.materials.Length > 1)
            {
                initialColor = cubeRenderer.materials[1].color;
            }
            
            // Set default text
            if (resultText != null)
            {
                resultText.text = DEFAULT_TEXT;
                resultText.gameObject.SetActive(true);
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
                wasActiveRecorder = true;
                context.SendJson(new IsRecordingMessage { 
                    isRecording = true,
                    requestMode = this.requestMode,
                    isProcessing = false
                });
                
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
                        context.SendJson(new IsRecordingMessage { 
                    isRecording = false,
                    requestMode = this.requestMode,
                    isProcessing = false
                });
                
                // Revert the cube's color
                Material[] mats = cubeRenderer.materials;
                if (mats.Length > 1)
                {
                    mats[1].color = initialColor;
                }
                cubeRenderer.materials = mats;
            }
        }

        private struct IsRecordingMessage {
            public bool isRecording;
            public SkinConstants.RequestMode requestMode;
            public bool isProcessing;
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            if (!wasActiveRecorder) return;
            wasActiveRecorder = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Debug.Log("Processing...");

            _buffer = "";
            
            if (resultText != null)
            {
                resultText.text = PROCESSING_TEXT;
            }

            context.SendJson(new IsRecordingMessage { 
                    isRecording = false,
                    requestMode = this.requestMode,
                    isProcessing = true
            });
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            
            sw.Stop();

            if (res == null)
            {
                Debug.Log("Recognition failed");
            }

            string output = res.Result;

            if (requestMode == RequestMode.SelectSkin)
            {
                var skinSearchText = GameObject.Find("SkinSearchText")?.GetComponent<TextMeshPro>();
                if (skinSearchText != null)
                {
                    skinSearchText.text = output.ToUpper().Replace(" ", "\n") + "\n";
                }
                List<string> skinDescription = new List<string>{output};
                apiRequestHandler.HandleRequest(skinDescription, RequestMode.SelectSkin);
            }
            else if (requestMode == RequestMode.GenerateSkin)
            {
                if(promptHelper != null) promptHelper.SetPrompt(output);
            }
            
            
            long time = sw.ElapsedMilliseconds;
            float rate = recordedAudio.Length / (time * 0.001f);
            Debug.Log($"Time: {time} ms, Rate: {rate:F1}x");
            if (resultText != null)
            {
                resultText.text = DEFAULT_TEXT;
            }
            context.SendJson(new IsRecordingMessage { 
                    isRecording = false,
                    requestMode = this.requestMode,
                    isProcessing = false
            });
        }

        private void OnProgressHandler(int progress)
        {
            Debug.Log($"Progress: {progress}%");
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var m = message.FromJson<IsRecordingMessage>();

            if (m.requestMode == this.requestMode)
            {
                    if (m.isRecording)
                    {
                        Material[] mats = cubeRenderer.materials;
                        if (mats.Length > 1)
                        {
                            mats[1].color = Color.red;
                        }
                        cubeRenderer.materials = mats;
                    }
                    else
                    {   
                        if (m.isProcessing){
                            if (resultText != null)
                            {
                                resultText.text = PROCESSING_TEXT;
                            }
                        }
                        else{
                            if (resultText != null)
                            {
                                resultText.text = DEFAULT_TEXT;
                            }
                        }
                        // Revert the cube's color to original
                        Material[] mats = cubeRenderer.materials;
                        if (mats.Length > 1)
                        {
                            mats[1].color = initialColor;
                        }
                        cubeRenderer.materials = mats;
                    }
            }
        }
    }
}
