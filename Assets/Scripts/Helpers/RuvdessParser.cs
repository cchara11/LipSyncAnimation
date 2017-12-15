using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;

///-----------------------------------------------------------------
///   Class:        RuvdessParser.cs
///   Description:  Class responsible for ravdess dataset parse 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public class RuvdessParser : MonoBehaviour {
    
    public Dropdown audioDropDown;
    public Text soundInfo;
    int currentIndex;
    bool generateTranscripts = false;
    UnityEngine.Object[] clips;
    List<string> clipInfo;
    string vocalContext;

    // Use this for initialization
    void Start () {
        clips = AudioDatabase.clips;
        clipInfo = new List<string>();

        audioDropDown.options.Clear();

        audioDropDown.options.Add(new Dropdown.OptionData() { text = "" });
        foreach (UnityEngine.Object obj in clips)
        {
            string plainName = obj.name.Replace(".wav", "");
            string audioInfo = ParseSoundFile(plainName);
            audioDropDown.options.Add(new Dropdown.OptionData() { text = audioInfo });
            if (generateTranscripts)
            {
                clipInfo.Add(audioInfo);
                using (StreamWriter sw = new StreamWriter(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + plainName + ".txt")))
                {
                    if (vocalContext.Equals("kids"))
                    {
                        sw.Write("kids are talking by the door");
                    }
                    else
                    {
                        sw.Write("dogs are sitting by the door");
                    }
                    sw.Close();
                }
            }
        }
        
    }

    /// <summary>
    /// Sets current audio according to user selection
    /// </summary>
    /// <param name="fileIndex"></param>
    public void SetCurrentFile(int fileIndex)
    {
        currentIndex = fileIndex;
        soundInfo.text = clips[currentIndex - 1].name.Replace(".wav","");
    }

    /// <summary>
    /// Parse sound file based on information given by the name of the file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    string ParseSoundFile(string file)
    {
        string[] soundData = file.Split('-');
        if (soundData.Length < 2)
        {
            return file;
        }
        string vocalType = GetVocalChannel(Int32.Parse(soundData[1]));
        string emotionType = GetEmotion(Int32.Parse(soundData[2]));
        string emotionalIntensity = GetEmotionalIntensity(Int32.Parse(soundData[3]));
        string vocalContext = GetVocalContext(Int32.Parse(soundData[4]));
        string repetition = GetRepetition(Int32.Parse(soundData[5]));
        return (vocalType + " " + emotionType + " " + emotionalIntensity + " " + vocalContext + " " + repetition);
    }

    /// <summary>
    /// Vocal channel
    /// </summary>
    /// <param name="vocalIndex"></param>
    /// <returns></returns>
    string GetVocalChannel(int vocalIndex)
    {
        switch (vocalIndex)
        {
            case 1:
                return "speech";
            default:
                return "song";
        }
    }

    /// <summary>
    /// Emotion type
    /// </summary>
    /// <param name="emotionIndex"></param>
    /// <returns></returns>
    string GetEmotion(int emotionIndex)
    {
        switch(emotionIndex)
        {
            case 1:
                return "neutral";
            case 2:
                return "calm";
            case 3:
                return "happy";
            case 4:
                return "sad";
            case 5:
                return "angry";
            case 6:
                return "fearful";
            case 7:
                return "disgust";
            default:
                return "surprised";
        }
    }

    /// <summary>
    /// Emotional intensity
    /// </summary>
    /// <param name="emotionalIntensityIndex"></param>
    /// <returns></returns>
    string GetEmotionalIntensity(int emotionalIntensityIndex)
    {
        switch (emotionalIntensityIndex)
        {
            case 1:
                return "normal";
            default:
                return "strong";
        }
    }

    /// <summary>
    /// Vocal context
    /// </summary>
    /// <param name="vocalContextIndex"></param>
    /// <returns></returns>
    string GetVocalContext(int vocalContextIndex)
    {
        switch (vocalContextIndex)
        {
            case 1:
                vocalContext = "kids";
                return "kids";
            default:
                vocalContext = "dogs";
                return "dogs";
        }
    }

    /// <summary>
    /// Repetition indicator
    /// </summary>
    /// <param name="repetitionIndex"></param>
    /// <returns></returns>
    string GetRepetition(int repetitionIndex)
    {
        switch (repetitionIndex)
        {
            case 1:
                return "1st rep";
            default:
                return "2nd rep";
        }
    }

}
