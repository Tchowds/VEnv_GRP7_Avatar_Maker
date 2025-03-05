using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit; // XR input support
using Whisper.Utils;

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
        private bool isRecording = false;
        private string _buffer;

        private void Start()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;
        }

        // Function that gets triggered when you click the cube in VR
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

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null) return;

            var text = res.Result;
            if (printLanguage)
                text += $"\n(Language: {res.Language})";

            textMesh.text = text; // Update world-space text
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
    }
}
