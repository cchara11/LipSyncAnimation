using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


///-----------------------------------------------------------------
///   Class:        AudioDatabase.cs
///   Description:  Class used to load audio files
///   Author:       Constantinos Charalambous     Date: 23/09/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public class AudioDatabase : MonoBehaviour
{
    public AudioClip[] waveFiles;
    public static Dictionary<int, AudioClip> audioClips;
    public static UnityEngine.Object[] clips;

    private void Awake()
    {
        clips = Resources.LoadAll("Audio/" + SceneManager.GetActiveScene().name, typeof(AudioClip));
    }

    internal static string GetAudioByName(int index)
    {
        return clips[index - 1].name + ".wav";
    }

    internal static AudioClip GetAudioByClip(int index)
    {
        return (AudioClip)clips[index - 1];
    }
}
