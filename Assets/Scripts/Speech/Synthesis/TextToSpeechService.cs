using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextToSpeechService : MonoBehaviour {

    string text;

    public void SetText(string text)
    {
        this.text = text;
    }

	public void GenerateAudio()
    {
        TextToSpeechAPIv2 tts = new TextToSpeechAPIv2();
        tts.SetAudioMode(AudioMode.CereVoice);
        tts.SetCurrentRecording("audio");
        tts.GenerateAudio(text);
    }
}
