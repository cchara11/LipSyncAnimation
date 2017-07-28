using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine;
using System.Text;
using System;

///-----------------------------------------------------------------
///   Class:        SpeechRecognizer.cs
///   Description:  This class is used to recognize speech from 
///                 audio input. The recognized text will be used
///                 as input for the tts engine, and therefore for
///                 the lip sync component.
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Recognize speech from audio input
///-----------------------------------------------------------------

public class SpeechRecognizer : MonoBehaviour {

    KeywordRecognizer keywordRecognizer;

    [SerializeField]
    private string[] m_Keywords;

    // Use this for initialization
    void Start () {
        keywordRecognizer = new KeywordRecognizer(m_Keywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        keywordRecognizer.Start();
    }
	
	// Update is called once per frame
	void Update () {
	}

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        string pronunciation = "";
        //phoneticEngine.getPronunciation(args.text);
        builder.AppendFormat("\tPronunciation: {0}", pronunciation, Environment.NewLine);
        Debug.Log(builder.ToString());

        //args.semanticmeaning => assign phonemesin SRGS format
    }
}
