using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

///-----------------------------------------------------------------------
///   Class:        PhonemeAnalyzer.cs
///   Description:  Class responsible for phonemic breakdown and analysis 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------------

public class PhonemeAnalyzer
{
    enum ReadMode
    {
        Word,
        Pronunciation,
        Phoneme
    }

    public static List<WordInformation> words;
    public List<WordInformation> generatedWords;
    public AudioMode audioMode;
    Dictionary<Phoneme, List<Phoneme>> diphonePhonemes;
    ReadMode readMode;
    private MyLipSync[] lipSyncComponents;
    CoarticulationEnhancement coarticulation;
    PhonemeReader phonemeReader;
    AudioClip currentClip;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="audioMode"></param>
    /// <param name="clip"></param>
    public PhonemeAnalyzer(AudioMode audioMode, AudioClip clip)
    {
        this.audioMode = audioMode;
        if (clip != null)
        {
            currentClip = clip;
        }
        
        lipSyncComponents = (MyLipSync[])GameObject.FindObjectsOfType(typeof(MyLipSync));
        coarticulation = new CoarticulationEnhancement();
        phonemeReader = new PhonemeReader();
    }

    /// <summary>
    /// Get phoneme timings - Synthetic speech, analyze them further
    /// </summary>
    /// <param name="isPraat"></param>
    /// <param name="currentRecording"></param>
    public void ManagePhonemeTimings(bool isPraat, string currentRecording)
    {
        List<PhonemeInformation> phonemeInformation = new List<PhonemeInformation>();
        words = new List<WordInformation>();
        float partAudioDuration = 0;
        
        using (StreamReader sr = new StreamReader(PathManager.GetDataPath("phonemes.txt")))
        {
            string line;
            int audioParts = 0;
            float audioDuration = 0;

            while ((line = sr.ReadLine()) != null)
            {
                // wav details
                if (line.Contains("wav"))
                {
                    float prevDuration = audioDuration;
                    string[] line_contents = line.Split(null);
                    int audioSampleDuration = int.Parse(line_contents[4]);
                    audioDuration = AudioInfo.getExactDuration(audioSampleDuration);
                    audioParts += 1;
                    partAudioDuration += prevDuration;
                }

                // word details
                if (line.Contains("word"))
                {
                    string[] line_contents = line.Split(':');
                    string[] word_info = line_contents[2].Split(null);

                    // parse phoneme timings for multi audio parts
                    float startOffset = float.Parse(word_info[1], CultureInfo.InvariantCulture.NumberFormat);
                    float endOffset = float.Parse(word_info[2], CultureInfo.InvariantCulture.NumberFormat);

                    if (audioParts > 1)
                    {
                        startOffset += partAudioDuration;
                        endOffset += partAudioDuration;
                    }

                    WordInformation wordInfo = new WordInformation(startOffset, endOffset, word_info[3]);
                    words.Add(wordInfo);
                }

                // phoneme details
                if (line.Contains("phoneme"))
                {
                    string[] line_contents = line.Split(':');
                    string[] phoneme_info = line_contents[2].Split(null);

                    // parse phoneme timings for multi audio parts
                    float startOffset = float.Parse(phoneme_info[1], CultureInfo.InvariantCulture.NumberFormat);
                    float endOffset = float.Parse(phoneme_info[2], CultureInfo.InvariantCulture.NumberFormat);

                    if (audioParts > 1)
                    {
                        startOffset += partAudioDuration;
                        endOffset += partAudioDuration;
                    }

                    PhonemeInformation phonemeInfo = new PhonemeInformation(startOffset, endOffset, phoneme_info[3]);
                    phonemeInformation.Add(phonemeInfo);
                }    
            }

            sr.Close();
        }

        DiphoneMapping();
        
        if (audioMode.Equals(AudioMode.Natural))
        {
            if (isPraat)
            {
                generatedWords = ReadWordTimingsPraat(currentRecording);
            }
            else
            {
                generatedWords = ReadWordTimingsWatson(currentRecording);
            }

            AdjustPhonemeTimings(phonemeInformation);

        }

        phonemeInformation = coarticulation.AddDiphoneVisemes(phonemeInformation, diphonePhonemes);
        phonemeInformation = coarticulation.RemoveDuplicates(phonemeInformation);

        using (StreamWriter sw = new StreamWriter(PathManager.GetDataPath("phonemesParsed.txt")))
        {
            foreach (PhonemeInformation pi in phonemeInformation)
            {
                sw.WriteLine(pi.text + " " + pi.startingInterval + " " + pi.endingInterval + " " + pi.apex + " " + pi.influence);
            }

            sw.Close();
        }

        foreach (MyLipSync mls in lipSyncComponents)
        {
            mls.wordInfo = words;
            mls.generatedWordInfo = generatedWords;
            Debug.Log(mls.generatedWordInfo);

            mls.phonemeInformation = phonemeReader.ReadPhonemeTimings(PathManager.GetDataPath("phonemesParsed.txt"));

            if (phonemeInformation.Count > 0)
            {
                mls.currentPhoneme = phonemeInformation[0];
            }
            else
            {
                mls.currentPhoneme = null;
            }
        }
    }

    /// <summary>
    /// Get phoneme timings - Natural speech, analyze them further
    /// </summary>
    /// <param name="isPraat"></param>
    /// <param name="currentRecording"></param>
    public List<PhonemeInformation> ManageTextGridInfo(string currentFile)
    {
        List<WordInformation> words = new List<WordInformation>();
        List<PhonemeInformation> phonemes = new List<PhonemeInformation>();
        string line;
        using (StreamReader sr = new StreamReader(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + currentFile)))
        {
            while ((line = sr.ReadLine()) != null)
            {
                // abbreviation for word information
                if (line.Contains("ORT"))
                {
                    readMode = ReadMode.Word;
                }
                // abbreviation for pronunciation information
                else if (line.Contains("KAN"))
                {
                    readMode = ReadMode.Pronunciation;
                }
                // abbreviation for phoneme information
                else if (line.Contains("MAU"))
                {
                    readMode = ReadMode.Phoneme;
                }

                if (line.Contains("intervals "))
                {
                    float start = float.Parse(sr.ReadLine().Trim().Split()[2]);
                    float end = float.Parse(sr.ReadLine().Trim().Split()[2]);
                    string text;
                    if (readMode.Equals(ReadMode.Pronunciation))
                    {
                        text = sr.ReadLine().Trim().Split('"')[1];
                    }
                    else
                    {
                        text = sr.ReadLine().Trim().Split()[2].Replace("\"", string.Empty);
                    }

                    if (!text.Equals(""))
                    {
                        if (readMode.Equals(ReadMode.Word))
                        {
                            words.Add(new WordInformation(start, end, text.ToLower()));
                        }
                        else if (readMode.Equals(ReadMode.Pronunciation))
                        {
                            int wordIndex = words.FindIndex(x => (x.startingInterval == start) && (x.endingInterval == end));
                            if (wordIndex > -1)
                            {
                                words[wordIndex].setPronunciation(text);
                            }
                        }
                        else if (readMode.Equals(ReadMode.Phoneme))
                        {
                            // consider phoneme duration for faster speech rates
                            //Phoneme phoneme = CoarticulationEnhancement.GetPhonemeText((end - start), text);
                            PhonemeInformation pi = new PhonemeInformation(start, end, text);
                            phonemes.Add(pi);
                            int wordIndex = words.FindIndex(x => (x.startingInterval <= pi.startingInterval) && (x.endingInterval >= pi.endingInterval));
                            if (wordIndex > -1)
                            {
                                words[wordIndex].AddPhoneme(pi);
                            }
                        }
                    }
                }
            }

            sr.Close();
        }

        // add coarticulation enhancements
        DiphoneMapping();
        phonemes = coarticulation.AddDiphoneVisemes(phonemes, diphonePhonemes);

        XMLRogo rogoXML = new XMLRogo(currentClip);
        rogoXML.GenerateRogoXML(words, phonemes);
        phonemes = coarticulation.RemoveDuplicates(phonemes);
        CalculateInfluence(phonemes);

        foreach (MyLipSync mls in lipSyncComponents)
        {
            mls.wordInfo = words;
            mls.generatedWordInfo = words;
        }

        List<PhonemeInformation> alveorals = GetAlveoralInformation(phonemes);
        phonemes = coarticulation.RemoveDuplicates(phonemes);

        foreach (PhonemeInformation pi in phonemes)
        {
            try
            {
                WordInformation word = words.SingleOrDefault(x => (x.startingInterval <= pi.startingInterval && x.endingInterval >= pi.endingInterval));
                if (word.phonemes.Count == 0)
                {
                    pi.onset_offset = 0.12f;
                }
                else
                {
                    pi.onset_offset = (word.endingInterval - word.startingInterval) / word.phonemes.Count;
                }
                
            }
            catch (NullReferenceException) { }
        }

        foreach (PhonemeInformation pi in alveorals)
        {
            try
            {
                WordInformation word = words.SingleOrDefault(x => (x.startingInterval <= pi.startingInterval && x.endingInterval >= pi.endingInterval));
                if (word.phonemes.Count == 0)
                {
                    pi.onset_offset = 0.12f;
                }
                else
                {
                    pi.onset_offset = (word.endingInterval - word.startingInterval) / word.phonemes.Count;
                }
            }
            catch (NullReferenceException) { }
        }

        foreach (MyLipSync mls in lipSyncComponents)
        {
            mls.alveoralInformation = alveorals;
        }

        return phonemes;
        
    }

    /// <summary>
    /// Maps each diphone to its corresponding phonemes based on 
    /// either the XML or the inspector configuration 
    /// </summary>
    void DiphoneMapping()
    {
        diphonePhonemes = new Dictionary<Phoneme, List<Phoneme>>();
        Phoneme diphone;

        // read the XML configuration file
        XDocument config = XDocument.Load(PathManager.GetXMLDataPath("diphoneMapping.xml"));
        var attributes = config.Root.Descendants("ATTR");
        foreach (var attr in attributes)
        {
            diphone = (Phoneme)Enum.Parse(typeof(Phoneme), attr.Element("Diphone").Value);
            diphonePhonemes.Add(diphone, new List<Phoneme>());

            var phonemes = attr.Elements("Phoneme");
            foreach (var p in phonemes)
            {
                diphonePhonemes[diphone].Add((Phoneme)Enum.Parse(typeof(Phoneme), p.Element("Type").Value));
            }
        }
    }

    /// <summary>
    /// Get phoneme timings for alveorals (RRR, LLL, GK)
    /// </summary>
    /// <param name="phonemes"></param>
    /// <returns></returns>
    List<PhonemeInformation> GetAlveoralInformation(List<PhonemeInformation> phonemes)
    {
        List<PhonemeInformation> alveorals = new List<PhonemeInformation>();
        alveorals.Add(new PhonemeInformation(0, 0, Phoneme.Rest));
        for (int i = 0; i < phonemes.Count; i++)
        {
            if (Enum.IsDefined(typeof(Alveorals), phonemes[i].text.ToString()))
            {
                PhonemeInformation pi = new PhonemeInformation(phonemes[i].startingInterval, phonemes[i].endingInterval, (Phoneme)phonemes[i].text);

                // add onset_offset and influence values
                pi.startingInterval = pi.startingInterval - (pi.onset_offset * pi.backwardInfluence);
                pi.endingInterval = pi.endingInterval + (pi.onset_offset * pi.forwardInfluence);

                alveorals.Add(pi);
                if (i == 0)
                {
                    if (Enum.IsDefined(typeof(Vowel), phonemes[i + 1].text.ToString()))
                    {
                        SubstitutePhoneme(phonemes[i], phonemes[i + 1]);
                    }
                    else
                    {
                        ChangeToSchwa(phonemes[i]);
                    }
                }
                else if (i == phonemes.Count - 1)
                {
                    if (Enum.IsDefined(typeof(Vowel), phonemes[i - 1].text.ToString()))
                    {
                        SubstitutePhoneme(phonemes[i], phonemes[i - 1]);
                    }
                    else
                    {
                        ChangeToSchwa(phonemes[i]);
                    }
                }
                else
                {
                    if (Enum.IsDefined(typeof(Vowel), phonemes[i - 1].text.ToString()) && Enum.IsDefined(typeof(Vowel), phonemes[i + 1].text.ToString()))
                    {
                        SubstitutePhoneme(phonemes[i], phonemes[i - 1]);
                    }
                    else if (Enum.IsDefined(typeof(Vowel), phonemes[i - 1].text.ToString()) && !Enum.IsDefined(typeof(Vowel), phonemes[i + 1].text.ToString()))
                    {
                        SubstitutePhoneme(phonemes[i], phonemes[i - 1]);
                    }
                    else if (!Enum.IsDefined(typeof(Vowel), phonemes[i - 1].text.ToString()) && Enum.IsDefined(typeof(Vowel), phonemes[i + 1].text.ToString()))
                    {
                        SubstitutePhoneme(phonemes[i], phonemes[i + 1]);
                    }
                    else
                    {
                        ChangeToSchwa(phonemes[i]);
                    }
                }
            }
        }

        return alveorals;
    }

    /// <summary>
    /// Substitutes phoneme value from oldPhoneme to newPhoneme
    /// </summary>
    /// <param name="oldPhoneme"></param>
    /// <param name="newPhoneme"></param>
    void SubstitutePhoneme(PhonemeInformation oldPhoneme, PhonemeInformation newPhoneme)
    {
        oldPhoneme.text = newPhoneme.text;
        oldPhoneme.influence = newPhoneme.influence;
        oldPhoneme.meanIntensity = newPhoneme.meanIntensity;
        oldPhoneme.meanPitch = newPhoneme.meanPitch;
    }

    /// <summary>
    /// Substitutes phoneme with phoneme Schwa
    /// </summary>
    /// <param name="oldPhoneme"></param>
    void ChangeToSchwa(PhonemeInformation oldPhoneme)
    {
        PhonemeInformation pi = new PhonemeInformation(oldPhoneme.startingInterval, oldPhoneme.endingInterval, Phoneme.Schwa);
        oldPhoneme.text = pi.text;
        oldPhoneme.influence = pi.influence;
    }
     
    /// <summary>
    /// Calculates backward and forward influence
    /// </summary>
    /// <param name="phonemes"></param>
    void CalculateInfluence(List<PhonemeInformation> phonemes)
    {
        for (int i = 0; i < phonemes.Count; i++)
        {
            phonemes[i].phonemicInfluenceBack = Influence.NA;
            phonemes[i].phonemicInfluenceForw = Influence.NA;

            if (i > 0)
            {
                phonemes[i].phonemicInfluenceBack = GetInfluence(phonemes[i], phonemes[i - 1]);
            }
            if (i < phonemes.Count - 1)
            {                
                phonemes[i].phonemicInfluenceForw = GetInfluence(phonemes[i], phonemes[i + 1]);
            }
        }
    }

    /// <summary>
    /// Returns influence indicator based on neihgboring phonemes
    /// </summary>
    /// <param name="phoneme"></param>
    /// <param name="neighboringPhoneme"></param>
    /// <returns></returns>
    Influence GetInfluence(PhonemeInformation phoneme, PhonemeInformation neighboringPhoneme)
    {
        if (Enum.IsDefined(typeof(Vowel), phoneme.text.ToString()))
        {
            if (Enum.IsDefined(typeof(Consonant), neighboringPhoneme.text.ToString()))
            {
                return Influence.VC;
            }
            else if (Enum.IsDefined(typeof(Vowel), neighboringPhoneme.text.ToString()))
            {
                return Influence.VV;
            }
        }
        if (Enum.IsDefined(typeof(Consonant), phoneme.text.ToString()))
        {
            if (Enum.IsDefined(typeof(Consonant), neighboringPhoneme.text.ToString()))
            {
                return Influence.CC;
            }
            else if (Enum.IsDefined(typeof(Vowel), neighboringPhoneme.text.ToString()))
            {
                return Influence.CV;
            }
        }
        return Influence.NA;

    }
    
    /// <summary>
    /// Returns word timings based on praat information
    /// Used for a previous version implementation
    /// </summary>
    /// <param name="currentRecording"></param>
    /// <returns></returns>
    List<WordInformation> ReadWordTimingsPraat(string currentRecording)
    {
        List<WordInformation> words = new List<WordInformation>();
        string line;

        using (StreamReader sr = new StreamReader(SceneManager.GetActiveScene().name + "/" + PathManager.GetAudioPath(currentRecording + ".TextGrid")))
        {
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("intervals "))
                {
                    float start = float.Parse(sr.ReadLine().Trim().Split()[2]);
                    float end = float.Parse(sr.ReadLine().Trim().Split()[2]);
                    string text = sr.ReadLine().Trim().Split()[2].Replace("\"", string.Empty);
                    if (!text.Equals(""))
                    {
                        words.Add(new WordInformation(start, end, text.ToLower()));
                    }
                }
            }

            sr.Close();
        }

        return words;
    }

    /// <summary>
    /// Returns word timings based on watson information
    /// Used for a previous version implementation
    /// </summary>
    /// <param name="currentRecording"></param>
    /// <returns></returns>
    List<WordInformation> ReadWordTimingsWatson(string currentRecording)
    {
        List<WordInformation> words = new List<WordInformation>();
        string line;

        using (StreamReader sr = new StreamReader(PathManager.GetAudioPath(currentRecording + "_metadata.txt")))
        {
            while ((line = sr.ReadLine()) != null)
            {
                string[] lineContents = line.Split();
                string word = lineContents[0];
                float start = float.Parse(lineContents[1]);
                float end = float.Parse(lineContents[2]);

                words.Add(new WordInformation(start, end, word.ToLower()));
            }

            sr.Close();
        }

        return words;
    }

    /// <summary>
    /// Adjusts phoneme timings
    /// </summary>
    /// <param name="phonemeInfo"></param>
    void AdjustPhonemeTimings(List<PhonemeInformation> phonemeInfo)
    {
        phonemeInfo.RemoveAll(x => x.text.Equals(Phoneme.Rest));

        // adjust phoneme timings
        foreach (PhonemeInformation pi in phonemeInfo)
        {
            // find word that corresponds to the phoneme's intervals
            int index = words.IndexOf(words.Find(x => (x.startingInterval <= pi.startingInterval) && (x.endingInterval >= pi.endingInterval)));

            if (index >= 0)
            {
                pi.startingInterval = RangeTransformation.Transform(pi.startingInterval, words[index].startingInterval, words[index].endingInterval, generatedWords[index].startingInterval, generatedWords[index].endingInterval);
                pi.endingInterval = RangeTransformation.Transform(pi.endingInterval, words[index].startingInterval, words[index].endingInterval, generatedWords[index].startingInterval, generatedWords[index].endingInterval);
            }
        }
    }
}
