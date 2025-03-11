using UnityEngine;
using Whisper.Utils;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit;

namespace Whisper.Samples
{
    public class MicrophoneCubeRecorder : MonoBehaviour
    {
        public ApiRequestHandler apiRequestHandler;

        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;

        [Header("UI")]
        public TextMeshPro statusText;

        public event Action<string> OnSpeechRecognized;

        private bool isRecording = false;
        private string _buffer;

        private void Start()
        {
            whisper.OnNewSegment += OnNewSegment;
            microphoneRecord.OnRecordStop += OnRecordStop;
            OnSpeechRecognized += apiRequestHandler.HandleRequest;
        }

        public void ToggleRecording()
        {
            if (!isRecording)
            {
                microphoneRecord.StartRecord();
                isRecording = true;
                statusText.text = "Recording...";
            }
            else
            {
                microphoneRecord.StopRecord();
                isRecording = false;
                statusText.text = "Processing...";
            }
        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            statusText.text = "Processing...";
            _buffer = "";

            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null)
            {
                statusText.text = "Recognition failed";
                return;
            }

            statusText.text = res.Result;
            OnSpeechRecognized?.Invoke(res.Result);
        }

        private void OnNewSegment(WhisperSegment segment)
        {
            _buffer += segment.Text;
            statusText.text = _buffer + "...";
        }
    }
}
