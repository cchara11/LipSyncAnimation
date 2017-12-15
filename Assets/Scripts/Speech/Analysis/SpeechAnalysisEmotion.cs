using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

///--------------------------------------------------------------------
///   Class:        SpeechAnalysisEmotion.cs
///   Description:  Class responsible for emotion analysis
///                 Can be use for future research
///   Author:       Constantinos Charalambous     Date: 13/11/2017
///   Notes:        Lip Sync Animation
///--------------------------------------------------------------------

public class SpeechAnalysisEmotion : MonoBehaviour
{
    string inputFileUrl;
    string currentClipName, audioName;
    List<WordInformation> currentWords;
    public static List<PhonemeInformation> phonemeTimings;

    /// <summary>
    /// configure arguments for ttscallback process
    /// </summary>
    List<string> ConfigureArguments()
    {
        List<string> cmd_arguments = new List<string>();
        // add command arguments for tts_callback proces
        cmd_arguments = new List<string>();
        cmd_arguments.Add("-C"); // configuration argument
        cmd_arguments.Add(PathManager.GetOpenSmileConfigPath("IS09_emotion.conf")); // prosody config path
        cmd_arguments.Add("-I"); // input argument
        cmd_arguments.Add(PathManager.GetResourcesPath(audioName + ".wav")); // input audio path
        cmd_arguments.Add("-csvoutput"); // output argument
        cmd_arguments.Add(PathManager.GetResourcesPath("testing123.csv")); // output file path
        return cmd_arguments;
    }

    /// <summary>
    /// analyzes audio using OpenSmile process (SMILExtract_Release.exe), and produces prosodic features
    /// </summary>
    /// <param name="args"></param>
    public void AnalyzeAudio()
    {
        audioName = currentClipName.Replace(".wav", "");
        //inputFileUrl = PathManager.GetResourcesPath("prosody_" + audioName + ".csv");
        List<string> args = ConfigureArguments();

        // build input arguments into a single string
        string arguments = "";
        foreach (string s in args)
        {
            arguments += s;
            arguments += " ";
        }

        try
        {
            // create a new process
            var open_smile_prosody = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = PathManager.GetOpenSmilePath("SMILExtract_Release.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            // call open_smile process
            open_smile_prosody.EnableRaisingEvents = true;
            open_smile_prosody.Exited += new EventHandler(ProcessExited);
            open_smile_prosody.Start();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }

    public void GetMouthPositionsIEMOCAP()
    {
        List<string> mouthValues = new List<string>();
        using (StreamReader sr = new StreamReader(PathManager.GetResourcesPath("Ses01F_script02_1.txt")))
        {
            string[] firstTitles = sr.ReadLine().Split();
            int firstElementIndex = Array.IndexOf(firstTitles, "Mou1");
            string line = sr.ReadLine();
            while ((line = sr.ReadLine()) != null)
            {
                string[] elements = line.Split();
                string empty = elements[1];
                for (int i = firstElementIndex; i < firstElementIndex + 8; i++)
                {
                    empty += " " + elements[i];
                }
                mouthValues.Add(empty);
            }
        }

        foreach(string s in mouthValues)
        {
            print(s);
        }
    }

    public void SetAudioName(int index)
    {
        currentClipName = AudioDatabase.GetAudioByName(index);
    }

    private void ProcessExited(object sender, System.EventArgs e)
    {
        print("ended");
        //CalculateProsodicFeatures();
    }

    //public void CalculateProsodicFeatures()
    //{
    //    float timestamp, F0, voicingProbability, loudness;
    //    String line;

    //    try
    //    {
    //        using (StreamReader sr = new StreamReader(inputFileUrl))
    //        {
    //            if (!sr.EndOfStream)
    //            {
    //                // skip first line containing text information
    //                sr.ReadLine();
    //            }

    //            while (!sr.EndOfStream)
    //            {
    //                line = sr.ReadLine();
    //                String[] lineContents = line.Split(';');
    //                timestamp = float.Parse(lineContents[1]);
    //                voicingProbability = float.Parse(lineContents[2]);
    //                F0 = float.Parse(lineContents[3]);
    //                loudness = float.Parse(lineContents[4]);
    //            }
    //        }

    //    }

    //    catch (Exception e)
    //    {
    //        UnityEngine.Debug.Log("ERROR: Could not read file " + inputFileUrl);
    //        UnityEngine.Debug.Log(e);
    //    }

        
    //}
}
