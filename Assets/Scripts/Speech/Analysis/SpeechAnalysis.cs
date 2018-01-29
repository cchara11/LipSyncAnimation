using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

///-----------------------------------------------------------------
///   Class:        SpeechAnalysis.cs
///   Description:  Class responsible for audio analysis 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public class SpeechAnalysis
{
    string inputFileUrl, textGridFile, pitchFile, decibelFile, frequencyFile, frequencyAverageFile, decibelAverageFile;
    string currentClipName, audioName;
    List<WordInformation> currentWords;
    public List<PhonemeInformation> phonemeTimings;
    private MyLipSync[] lipSyncComponents;
    float slidingWindow;
    float totalAudioDuration;

    // frequencies and decibels
    Dictionary<float, float> frequencies;
    Dictionary<float, float> decibels;

    enum AudioFeature
    {
        Pitch,
        Intensity
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="currentClipName"></param>
    /// <param name="phonemeTimings"></param>
    public SpeechAnalysis(string currentClipName, List<PhonemeInformation> phonemeTimings, float slidingWindow)
    {
        this.currentClipName = currentClipName;
        lipSyncComponents = (MyLipSync[])GameObject.FindObjectsOfType(typeof(MyLipSync));
        this.phonemeTimings = phonemeTimings;
        this.slidingWindow = slidingWindow;

        // files for debugging
        frequencyFile = PathManager.GetResourcesPath("frequenciesOpen.txt");
        frequencyAverageFile = PathManager.GetResourcesPath("frequenciesAverageOpen.txt");
        decibelFile = PathManager.GetResourcesPath("decibelsOpen.txt");
        decibelAverageFile = PathManager.GetResourcesPath("decibelsAverageOpen.txt");
    }

    void Initialize()
    {
        frequencies = new Dictionary<float, float>();
        decibels = new Dictionary<float, float>();
    }
    /// <summary>
    /// configure arguments for ttscallback process
    /// </summary>
    List<string> ConfigureArguments()
    {
        List<string> cmd_arguments = new List<string>();
        // add command arguments for tts_callback proces
        cmd_arguments = new List<string>();
        cmd_arguments.Add("-C"); // configuration argument
        cmd_arguments.Add(PathManager.GetOpenSmileConfigPath("prosodyAcf.conf")); // prosody config path
        cmd_arguments.Add("-I"); // input argument
        cmd_arguments.Add(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + audioName + ".wav")); // input audio path
        cmd_arguments.Add("-csvoutput"); // output argument
        cmd_arguments.Add(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + "prosody_" + audioName + ".csv")); // output file path
        return cmd_arguments;
    }

    /// <summary>
    /// analyzes audio using OpenSmile process (SMILExtract_Release.exe), and produces prosodic features
    /// </summary>
    /// <param name="args"></param>
    public void AnalyzeAudio()
    {
        audioName = currentClipName.Replace(".wav", "");
        inputFileUrl = PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + "prosody_" + audioName + ".csv");
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

    /// <summary>
    /// On process exit continue with feature calculation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProcessExited(object sender, System.EventArgs e)
    {
        Initialize();
        CalculateProsodicFeatures();
        GetIndividualFrequencies();
        GetIndividualDecibels();
        SetPhonemeTimings();
        foreach (MyLipSync mls in lipSyncComponents)
        {
            mls.RearrangePhonemeTimings();
        }
    }

    /// <summary>
    /// Calculate prosodic features (frequency, intensity) from OpenSmile output file
    /// </summary>
    public void CalculateProsodicFeatures()
    {
        float timestamp, F0, loudness;
        //float voicingProbability;
        String line;

        try
        {
            using (StreamReader sr = new StreamReader(inputFileUrl))
            {
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
                    //voicingProbability = float.Parse(lineContents[2]);
                    F0 = float.Parse(lineContents[3]);
                    loudness = float.Parse(lineContents[4]);

                    try
                    {
                        frequencies.Add((float)Math.Round(timestamp,2), F0);
                        decibels.Add((float)Math.Round(timestamp, 2), loudness);
                    }
                    catch(ArgumentException e)
                    {}
                }
            }

            MyLipSync.frequencies = frequencies;
            MyLipSync.decibels = decibels;
            MyLipSync.slidingWindow = slidingWindow;
            
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("ERROR: Could not read file " + inputFileUrl);
            UnityEngine.Debug.Log(e);
        }

        
    }

    /// <summary>
    /// Get Individual mean frequency values for each phoneme
    /// </summary>
    public void GetIndividualFrequencies()
    {
        float individualFrequency, individualCount, frequency, meanVowelPitch, meanConsonantPitch;
        float minVowelPitch = float.MaxValue, maxVowelPitch = float.MinValue, minConsonantPitch = float.MaxValue, maxConsonantPitch = float.MinValue;
        totalAudioDuration = frequencies.Keys.Max();

        // calculation of frequency per sliding window
        List<float>[] pitchPerWindowVowel = new List<float>[((int)totalAudioDuration * 1000) / (int)slidingWindow + 1];
        List<float>[] pitchPerWindowConsonant = new List<float>[((int)totalAudioDuration * 1000) / (int)slidingWindow + 1];
        List<float> meanVowelPitches = new List<float>();
        List<float> meanConsonantPitches = new List<float>();
        for (int i = 0; i < pitchPerWindowVowel.Length; i++)
        {
            pitchPerWindowVowel[i] = new List<float>();
            pitchPerWindowConsonant[i] = new List<float>();
        }

        using (StreamWriter sw = new StreamWriter(frequencyFile))
        {
            sw.WriteLine("time frequency phoneme");

            foreach (PhonemeInformation pi in phonemeTimings)
            {
                if (Enum.IsDefined(typeof(Vowel), pi.text.ToString()))
                {
                    individualFrequency = 0;
                    individualCount = 0;

                    var frequencyElements = frequencies.Where(s => s.Key >= pi.startingInterval && s.Key <= pi.endingInterval).ToDictionary(dict => dict.Key, dict => dict.Value);

                    foreach (KeyValuePair<float, float> el in frequencyElements)
                    {
                        frequency = el.Value;
                        sw.WriteLine(el.Key + " " + frequency + " " + pi.text);
                        if (frequency == 0)
                        {
                            continue;
                        }

                        int windowIndex = (int)(el.Key * 1000) / (int)slidingWindow;
                        pitchPerWindowVowel[windowIndex].Add(frequency);
                        
                        individualFrequency += frequency;
                        individualCount += 1.0f;

                        // find max and min vowel frequency 
                        if (maxVowelPitch < frequency)
                        {
                            maxVowelPitch = frequency;
                        }
                        if (minVowelPitch > frequency)
                        {
                            minVowelPitch = frequency;
                        }
                    }

                    if (individualFrequency > 0)
                    {
                        pi.meanPitch = (individualFrequency / individualCount);
                    }
                }

                if (Enum.IsDefined(typeof(PlosiveFricative), pi.text.ToString()))
                {
                    individualFrequency = 0;
                    individualCount = 0;

                    var frequencyElements = frequencies.Where(s => s.Key >= pi.startingInterval && s.Key <= pi.endingInterval).ToDictionary(dict => dict.Key, dict => dict.Value);

                    foreach (KeyValuePair<float, float> el in frequencyElements)
                    {
                        frequency = el.Value;
                        sw.WriteLine(el.Key + " " + frequency + " " + pi.text);
                        if (frequency == 0)
                        {
                            continue;
                        }

                        int windowIndex = (int)(el.Key * 1000) / (int)slidingWindow;
                        pitchPerWindowConsonant[windowIndex].Add(frequency);

                        individualFrequency += frequency;
                        individualCount += 1.0f;

                        // find max and min vowel frequency 
                        if (maxConsonantPitch < frequency)
                        {
                            maxConsonantPitch = frequency;
                        }
                        if (minConsonantPitch > frequency)
                        {
                            minConsonantPitch = frequency;
                        }
                    }

                    if (individualFrequency > 0)
                    {
                        pi.meanPitch = (individualFrequency / individualCount);
                    }
                }
            }
        }

        foreach (List<float> meanFrequency in pitchPerWindowVowel)
        {
            float mean = meanFrequency.Sum() / meanFrequency.Count;
            if (!float.IsNaN(mean))
            {
                meanVowelPitches.Add(meanFrequency.Sum() / meanFrequency.Count);
            }
            else
            {
                meanVowelPitches.Add(0);
            }
        }
        foreach (List<float> meanFrequency in pitchPerWindowConsonant)
        {
            float mean = meanFrequency.Sum() / meanFrequency.Count;
            if (!float.IsNaN(mean))
            {
                meanConsonantPitches.Add(meanFrequency.Sum() / meanFrequency.Count);
            }
            else
            {
                meanConsonantPitches.Add(0);
            }
        }

        // mean pitch for all audio - splitted into custom sliding windows - new implementation
        MyLipSync.SetMeanPitches(meanVowelPitches, meanConsonantPitches);

        // mean pitch for all audio - old implementation
        meanVowelPitch = GetMeanValues(phonemeTimings, true, AudioFeature.Pitch);
        meanConsonantPitch = GetMeanValues(phonemeTimings, false, AudioFeature.Pitch);
        MyLipSync.SetVowelPitches(minVowelPitch, meanVowelPitch, maxVowelPitch); 
        MyLipSync.SetConsonantPitches(minConsonantPitch, meanConsonantPitch, maxConsonantPitch);

        // Debug purposes
        PrintToFile(AudioFeature.Pitch);
    }

    /// <summary>
    /// Get Individual mean intensity values for each phoneme
    /// </summary>
    void GetIndividualDecibels()
    {
        float individualIntensity, individualCount, intensity, meanVowelIntensity, meanConsonantIntensity;
        float minVowelIntensity = float.MaxValue, maxVowelIntensity = float.MinValue, minConsonantIntensity = float.MaxValue, maxConsonantIntensity = float.MinValue;
        totalAudioDuration = frequencies.Keys.Max();

        // calculation of frequency per sliding window
        List<float>[] intensityPerWindowVowel = new List<float>[((int)totalAudioDuration * 1000) / (int)slidingWindow + 1];
        List<float>[] intensityPerWindowConsonant = new List<float>[((int)totalAudioDuration * 1000) / (int)slidingWindow + 1];
        List<float> meanVowelIntensities = new List<float>();
        List<float> meanConsonantIntensities = new List<float>();
        for (int i = 0; i < intensityPerWindowVowel.Length; i++)
        {
            intensityPerWindowVowel[i] = new List<float>();
            intensityPerWindowConsonant[i] = new List<float>();
        }

        using (StreamWriter sw = new StreamWriter(decibelFile))
        {
            sw.WriteLine("time intensity phoneme");
            foreach (PhonemeInformation pi in phonemeTimings)
            {
                if (Enum.IsDefined(typeof(Vowel), pi.text.ToString()))
                {
                    individualIntensity = 0;
                    individualCount = 0;

                    var intensityElements = decibels.Where(s => s.Key >= pi.startingInterval && s.Key <= pi.endingInterval).ToDictionary(dict => dict.Key, dict => dict.Value);

                    foreach (KeyValuePair<float, float> el in intensityElements)
                    {
                        intensity = el.Value;
                        sw.WriteLine(el.Key + " " + intensity + " " + pi.text);
                        if (intensity == 0)
                        {
                            continue;
                        }
                        int windowIndex = (int)(el.Key * 1000) / (int)slidingWindow;
                        intensityPerWindowVowel[windowIndex].Add(intensity);

                        individualIntensity += intensity;
                        individualCount += 1.0f;

                        // find max and min vowel intensity 
                        if (maxVowelIntensity < intensity)
                        {
                            maxVowelIntensity = intensity;
                        }
                        if (minVowelIntensity > intensity)
                        {
                            minVowelIntensity = intensity;
                        }
                    }

                    if (individualIntensity > 0)
                    {
                        pi.meanIntensity = (individualIntensity / individualCount);
                    }
                }

                if (Enum.IsDefined(typeof(PlosiveFricative), pi.text.ToString()))
                {
                    individualIntensity = 0;
                    individualCount = 0;

                    var frequencyElements = frequencies.Where(s => s.Key >= pi.startingInterval && s.Key <= pi.endingInterval).ToDictionary(dict => dict.Key, dict => dict.Value);

                    foreach (KeyValuePair<float, float> el in frequencyElements)
                    {
                        intensity = el.Value;
                        sw.WriteLine(el.Key + " " + intensity + " " + pi.text);
                        if (intensity == 0)
                        {
                            continue;
                        }
                        int windowIndex = (int)(el.Key * 1000) / (int)slidingWindow;
                        intensityPerWindowConsonant[windowIndex].Add(intensity);

                        individualIntensity += intensity;
                        individualCount += 1.0f;

                        // find max and min vowel intensity 
                        if (maxConsonantIntensity < intensity)
                        {
                            maxConsonantIntensity = intensity;
                        }
                        if (minConsonantIntensity > intensity)
                        {
                            minConsonantIntensity = intensity;
                        }
                    }

                    if (individualIntensity > 0)
                    {
                        pi.meanIntensity = (individualIntensity / individualCount);
                    }
                }
            }
            sw.Close();
        }

        foreach (List<float> meanIntensity in intensityPerWindowVowel)
        {
            float mean = meanIntensity.Sum() / meanIntensity.Count;
            if (!float.IsNaN(mean))
            {
                meanVowelIntensities.Add(meanIntensity.Sum() / meanIntensity.Count);
            }
            else
            {
                meanVowelIntensities.Add(0);
            }
        }
        foreach (List<float> meanIntensity in intensityPerWindowConsonant)
        {
            float mean = meanIntensity.Sum() / meanIntensity.Count;
            if (!float.IsNaN(mean))
            {
                meanConsonantIntensities.Add(meanIntensity.Sum() / meanIntensity.Count);
            }
            else
            {
                meanConsonantIntensities.Add(0);
            }
        }

        // mean pitch for all audio - splitted into custom sliding windows - new implementation
        MyLipSync.SetMeanIntensities(meanVowelIntensities, meanConsonantIntensities);

        meanVowelIntensity = GetMeanValues(phonemeTimings, true, AudioFeature.Intensity);
        meanConsonantIntensity = GetMeanValues(phonemeTimings, false, AudioFeature.Intensity);

        MyLipSync.setVowelIntensities(minVowelIntensity, meanVowelIntensity, maxVowelIntensity);
        MyLipSync.SetConsonantIntensities(minConsonantIntensity, meanConsonantIntensity, maxConsonantIntensity);

        // Debug purposes
        PrintToFile(AudioFeature.Intensity);
    }

    /// <summary>
    /// Set phoneme timings of the main LipSync script
    /// </summary>
    void SetPhonemeTimings()
    {
        foreach (MyLipSync mls in lipSyncComponents)
        {
            mls.phonemeInformation = phonemeTimings;

            if (mls.phonemeInformation.Count > 0)
            {
                mls.currentPhoneme = mls.phonemeInformation[0];
            }
            else
            {
                mls.currentPhoneme = null;
            }
        }
        
    }

    /// <summary>
    /// Return mean value of the respective audiofeature (feature)
    /// OLD IMPLEMENTATION (Considered the whole audio)
    /// </summary>
    /// <param name="phonemeTimings"></param>
    /// <param name="isVowel"></param>
    /// <param name="feature"></param>
    /// <returns></returns>
    float GetMeanValues(List<PhonemeInformation> phonemeTimings, bool isVowel, AudioFeature feature)
    {
        float meanSum = 0;
        float count = 0;

        foreach (PhonemeInformation pi in phonemeTimings)
        {
            if (isVowel)
            {
                if (Enum.IsDefined(typeof(Vowel), pi.text.ToString()))
                {
                    if (feature.Equals(AudioFeature.Pitch))
                    {
                        if (pi.meanPitch > 0)
                        {
                            meanSum += pi.meanPitch;
                            count += 1f;
                        }
                    }
                    else
                    {
                        if (pi.meanIntensity > 0)
                        {
                            meanSum += pi.meanIntensity;
                            count += 1f;
                        }
                    }

                }
            }
            else
            {
                if (Enum.IsDefined(typeof(PlosiveFricative), pi.text.ToString()))
                {
                    if (feature.Equals(AudioFeature.Pitch))
                    {
                        if (pi.meanPitch > 0)
                        {
                            meanSum += pi.meanPitch;
                            count += 1f;
                        }
                    }
                    else
                    {
                        if (pi.meanIntensity > 0)
                        {
                            meanSum += pi.meanIntensity;
                            count += 1f;
                        }
                    }
                }
            }

        }

        return meanSum / count;
    }

    /// <summary>
    /// Prints audio feature information to file
    /// </summary>
    /// <param name="feature"></param>
    void PrintToFile(AudioFeature feature)
    {
        if (feature.Equals(AudioFeature.Pitch))
        {
            using (StreamWriter sw = new StreamWriter(frequencyAverageFile))
            {
                foreach (PhonemeInformation pi in phonemeTimings)
                {
                    sw.WriteLine((pi.startingInterval + pi.endingInterval) / 2.0f + " " + pi.meanPitch + " " + pi.text);
                }
                sw.Close();
            }
        }
        else
        {
            using (StreamWriter sw = new StreamWriter(decibelAverageFile))
            {
                foreach (PhonemeInformation pi in phonemeTimings)
                {
                    sw.WriteLine((pi.startingInterval + pi.endingInterval) / 2.0f + " " + pi.meanIntensity + " " + pi.text);
                }
                sw.Close();
            }
        }
    }
}
