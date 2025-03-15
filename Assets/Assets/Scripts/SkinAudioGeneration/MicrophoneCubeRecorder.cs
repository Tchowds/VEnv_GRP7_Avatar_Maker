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

        // Optional UI: assign a TextMeshPro element in the Inspector if desired.
        public TextMeshPro statusText;

        public event Action<string> OnSpeechRecognized;

        private bool isRecording = false;
        private string _buffer;

        private void Start()
        {
            // Subscribe to events
            //whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;
            OnSpeechRecognized += apiRequestHandler.HandleRequest;
        }

        public void ToggleRecording()
        {
            if (!isRecording)
            {
                microphoneRecord.StartRecord();
                isRecording = true;
                if (statusText != null)
                    statusText.text = "Recording...";
                else
                    Debug.Log("Recording...");
            }
            else
            {
                microphoneRecord.StopRecord();
                isRecording = false;
                if (statusText != null)
                    statusText.text = "Processing...";
                else
                    Debug.Log("Processing...");
            }
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            // Start measuring processing time
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

            // Prepare the output string (including language info)
            string output = res.Result;
            //output += $"\nLanguage: {res.Language}";

            if (statusText != null)
                statusText.text = output;
            else
                Debug.Log(output);

            // Log processing time and rate
            long time = sw.ElapsedMilliseconds;
            float rate = recordedAudio.Length / (time * 0.001f);
            Debug.Log($"Time: {time} ms, Rate: {rate:F1}x");

            // Raise the event for further processing
            OnSpeechRecognized?.Invoke(res.Result);
        }

        private void OnNewSegment(WhisperSegment segment)
        {
            _buffer += segment.Text;
            if (statusText != null)
                statusText.text = _buffer + "...";
            else
                Debug.Log(_buffer + "...");
        }

        private void OnProgressHandler(int progress)
        {
            Debug.Log($"Progress: {progress}%");
        }
    }
}

