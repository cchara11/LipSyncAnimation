using System.Diagnostics;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SpeechAnalysis : MonoBehaviour
{
    string inputFileUrl, textGridFile, pitchFile;
    string currentClipName;
    List<Word> currentWords;

    // pitch
    public static float pitchMedian;

    /// <summary>
    /// configure arguments for ttscallback process
    /// </summary>
    List<string> ConfigureArguments(AudioClip clip)
    {
        List<string> cmd_arguments = new List<string>();
        // add command arguments for tts_callback proces
        cmd_arguments = new List<string>();
        cmd_arguments.Add("-C"); // configuration argument
        cmd_arguments.Add(PathManager.getOpenSmileConfigPath("prosodyAcf.conf")); // prosody config path
        cmd_arguments.Add("-I"); // input argument
        cmd_arguments.Add(PathManager.getAudioPath(clip.name + ".wav")); // input audio path
        cmd_arguments.Add("-csvoutput"); // output argument
        cmd_arguments.Add(PathManager.getOpenSmileDataPath("prosody_" + clip.name + ".csv")); // output file path
        return cmd_arguments;
    }

    /// <summary>
    /// analyzes audio using OpenSmile process (SMILExtract_Release.exe), and produces prosodic features
    /// </summary>
    /// <param name="args"></param>
    public void AnalyzeAudio(AudioClip clip)
    {
        // set current audio clip
        currentClipName = clip.name;

        // play audio at first
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = clip;
        audio.Play();

        List<string> args = ConfigureArguments(clip);

        // check if an old version of the output file exists
        inputFileUrl = PathManager.getOpenSmileDataPath("prosody_" + clip.name + ".csv");
        textGridFile = PathManager.getAudioPath(currentClipName + ".TextGrid");
        pitchFile = PathManager.getAudioPath(currentClipName + "_pitch.txt");

        if (File.Exists(inputFileUrl))
        {
            File.Delete(inputFileUrl);
        }

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
                    FileName = PathManager.getOpenSmilePath("SMILExtract_Release.exe"),
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
            print(e);
        }
    }

    private void ProcessExited(object sender, System.EventArgs e)
    {
        //CalculateProsodicFeatures();
        GetWordBoundaries();
        GetPitchInformation();
        pitchMedian = GetPitchMedian();
    }

    public void CalculateProsodicFeatures()
    {
        List<ProsodyComponent> prosodicFeatures;
        float timestamp, F0, voicingProbability, loudness;
        String line;

        try
        {
            using (StreamReader sr = new StreamReader(inputFileUrl))
            {
                prosodicFeatures = new List<ProsodyComponent>();
                if (!sr.EndOfStream)
                {
                    // skip first line containing text information
                    sr.ReadLine();
                }

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    String[] lineContents = line.Split(';');
                    timestamp = float.Parse(lineContents[1]);
                    voicingProbability = float.Parse(lineContents[2]);
                    F0 = float.Parse(lineContents[3]);
                    loudness = float.Parse(lineContents[4]);

                    ProsodyComponent prosody = new ProsodyComponent(timestamp, voicingProbability, F0, loudness);
                    prosodicFeatures.Add(prosody);
                }
            }

            MyLipSync.prosody = prosodicFeatures;

        }

        catch (Exception e)
        {
            print("ERROR: Could not read file " + inputFileUrl);
            print(e);
        }

        print("AUDIO: " + inputFileUrl);
        print("MAX: " + GetMaximumFrequency());
        print("MIN: " + GetMinimumFrequency());
        print("AVERAGE: " + GetAverageFrequency());

    }

    /// <summary>
    /// Retrieve word boundaries from praat generated textgrid
    /// </summary>
    public void GetWordBoundaries()
    {
        currentWords = new List<Word>();
        String line;

        try
        {
            using (StreamReader sr = new StreamReader(textGridFile))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    // parse textgrid file
                    if (line.Contains("intervals") && line.Contains("["))
                    {
                        String[] wordInfo = new String[3];
                        for (int i = 0; i < wordInfo.Length; i++)
                        {
                            line = sr.ReadLine();
                            String[] lineContents = line.Split('=');
                            wordInfo[i] = lineContents[1];
                        }

                        string text = wordInfo[2].Trim().Replace("\"", "");

                        if (Regex.IsMatch(text, @"^[a-zA-Z]+$"))
                        {
                            Word word = new Word(float.Parse(wordInfo[0]), float.Parse(wordInfo[1]), text);
                            currentWords.Add(word);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            print("ERROR: Could not read textgrid file " + textGridFile);
            print(e);
        }

    }

    public void GetPitchInformation()
    {
        String line;

        try
        {
            using (StreamReader sr = new StreamReader(pitchFile))
            {
                if (!sr.EndOfStream)
                {
                    // skip first line containing text information
                    sr.ReadLine();
                }

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    String[] lineContents = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    float interval = float.Parse(lineContents[0]);

                    foreach (Word w in currentWords)
                    {
                        if (interval >= w.startingInterval && interval <= w.endingInterval)
                        {
                            float frequency;
                            bool isNumeric = float.TryParse(lineContents[1], out frequency);

                            if (isNumeric)
                            {
                                w.intervals.Add(interval);
                                w.frequencies.Add(frequency);
                            }
                        }
                    }
                    
                }
            }
        }
        catch (Exception e)
        {
            print("ERROR: Could not read textgrid file " + pitchFile);
            print(e);
        }

    }

    public float GetMaximumFrequency()
    {
        float maxFrequency = float.MinValue;
        foreach (ProsodyComponent pc in MyLipSync.prosody)
        {
            if (maxFrequency < pc.F0)
            {
                maxFrequency = pc.F0;
            }
        }

        return maxFrequency;
    }

    public float GetMinimumFrequency()
    {
        float minFrequency = float.MaxValue;
        foreach (ProsodyComponent pc in MyLipSync.prosody)
        {
            if (minFrequency > pc.F0 && pc.F0 != 0)
            {
                minFrequency = pc.F0;
            }
        }

        return minFrequency;
    }

    public float GetAverageFrequency()
    {
        float sum = 0;
        int count = 0;
        foreach (ProsodyComponent pc in MyLipSync.prosody)
        {
            sum += pc.F0;
            count++;
        }

        return sum / (float)count;
    }

    public float GetPitchMedian()
    {
        float sum = 0;
        int count = 0;
        foreach (Word w in currentWords)
        {
            foreach (float frequency in w.frequencies)
            {
                sum += frequency;
                count++;
            }
        }

        return sum / (float)count;
    }

}
