using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

///-----------------------------------------------------------------
///   Class:        LipSync.cs
///   Description:  The base class of the lip sync component 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

[Serializable]
public class MyLipSync : MonoBehaviour {
    public enum Gender
    {
        Male,
        Female
    }

    public enum ReadMode
    {
        Inspector,
        XML
    }

    public enum CurveMode
    {
        Exponential,
        Quadratic
    }

    [Header("Model")]
    public SkinnedMeshRenderer characterMesh; // character on which lip sync will be performed
    public Gender gender;

    // Time
    float audioElapsedTimer = 0.0f;
    float onset_offset_Synthetic;

    [Header("Read Mode")]
    public AudioMode audioMode;
    public ReadMode readMode;

    // BlendShapes
    List<BlendShape> blendShapes; // all blendShapes of the model
    public Dictionary<Phoneme, List<BlendShape>> phonemeBlendShapes; // map phonemes to blendshapes
    public Dictionary<Phoneme, List<Phoneme>> diphonePhonemes; // map phonemes to diphones (dynamic visemes)
    public Dictionary<Emotion, List<BlendShape>> emotionBlendShapes; // map emotions to blendshapes

    // Audio
    private const int sampleSize = 1024;
    private const float rmsValue = 0.001f;
    private const float threshold = 0.02f; // minimum amplitude
    new AudioSource audio;
    Source audioSource;
    string currentRecording;
    float timedPitch;
    float timedIntensity;
    float intensityWeight;
    public static Dictionary<float, float> frequencies;
    public static Dictionary<float, float> decibels;

    // pitch and intensity
    public static float slidingWindow;
    static float minVowelPitch, meanVowelPitch, maxVowelPitch;
    static float minVowelIntensity, meanVowelIntensity, maxVowelIntensity;
    static float minConsonantPitch, meanConsonantPitch, maxConsonantPitch;
    static float minConsonantIntensity, meanConsonantIntensity, maxConsonantIntensity;
    static float minFrequencyRange; 
    static float maxFrequencyRange;
    static float vowelPitchRatio, vowelIntensityRatio, consonantPitchRatio;
    static List<float> vowelPitchPerWindow, consonantPitchPerWindow;
    static List<float> vowelIntensityPerWindow, consonantIntensityPerWindow;

    // Transcript
    public List<WordInformation> wordInfo;
    public List<WordInformation> generatedWordInfo;
    Text transcriptText;
    string transcript;
    WordInformation currentWord;
    int maxTranscriptLength = 55;

    // Phonemes
    [HideInInspector]
    public List<PhonemeBlendShape> staticVisemes;
    [HideInInspector]
    public List<DiphonePhonemes> dynamicVisemes;
    public List<PhonemeInformation> phonemeInformation;
    public List<PhonemeInformation> finalPhonemeInformation;
    public List<PhonemeInformation> alveoralInformation;
    public PhonemeInformation currentPhoneme;
    public PhonemeInformation currentAlveoral;
    List<PhonemeInformation> currentPhonemes;
    List<PhonemeInformation> currentAlveorals;
    int currentIndex = 0; // current phoneme index
    int currentAlveoralIndex = 0;

    // Animation
    [Header("Animation Parameters")]
    public CurveMode curve;
    public bool considerFrequency;
    [Range(0.0f, 1.0f)]
    public float vowelOverVowel;
    [Range(0.0f, 1.0f)]
    public float vowelOverConsonant;
    [Range(0.0f, 1.0f)]
    public float consonantOverConsonant;
    [Range(0.0f, 1.0f)]
    public float consonantOverVowel;
    public float onsetThreshold = 0.05f;

    // Curve Information
    public List<PhonemeInformation> indexInformation;
    public List<GraphInfo> graphInformation;
    List<string> timingsInformation;

    // Emotions - for future research
    [HideInInspector]
    public List<EmotionBlendShape> emotions;
    public static List<EmotionInformation> emotionInformation;
    public static EmotionInformation currentEmotion;
    List<EmotionInformation> currentEmotions;
    int currentEmotionIndex = 0;

    /// <summary>
    /// Awake function used for initialization
    /// </summary>
    void Awake()
    {
        // check if there is a character available for lip-sync
        if (characterMesh == null)
        {
            Debug.Log("No character mesh is present");
            return;
        }
        // populate blendShapes List using the current model
        blendShapes = BlendShape.populateBlendShapeList(characterMesh);

        // check readMode
        if (readMode.Equals(ReadMode.XML))
        {
            PhonemeBlendShapeMapping();
            DiphoneMapping();
            //EmotionBlendShapeMapping();
        }
        else
        {
            ListToDictionary(staticVisemes);
            //ListToDictionary(emotions);
        }

        finalPhonemeInformation = new List<PhonemeInformation>();
        wordInfo = new List<WordInformation>();
        generatedWordInfo = new List<WordInformation>();
    }

    /// <summary>
    /// Start function used for initialization after Awake() function
    /// </summary>
    void Start()
    {
        // initialize 3D model
        InitializeModel();
        SetMinMaxFrequencyValues();

        // audio
        audio = GetComponent<AudioSource>();
        audioSource = Source.Listener;
        
        // onset_offset for synthetic speech
        if (audioMode.Equals(AudioMode.CereVoice))
        {
            onset_offset_Synthetic = 0.065f;
        }

        // transcript
        transcriptText = GameObject.Find("MainTranscript").GetComponent<Text>();
        transcript = "";
        currentWord = null;

        if (staticVisemes.Count == 0)
        {
            return;
        }

        // phonemes
        currentPhonemes = new List<PhonemeInformation>();
        SetCurrentPhoneme(new PhonemeInformation(0, 0, Phoneme.Rest));
        currentAlveorals = new List<PhonemeInformation>();
        SetCurrentAlveoral(new PhonemeInformation(0, 0, Phoneme.Rest));

        // testing
        indexInformation = new List<PhonemeInformation>();
        graphInformation = new List<GraphInfo>();
        timingsInformation = new List<string>();
        
    }

    /// <summary>
    /// Plays audio from the generated audio source
    /// </summary>
    public void PlayAudio()
    {
        // check if phonemic information exists
        string plainFileName = currentRecording.Replace(".wav", "");
        if (!File.Exists(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + plainFileName + ".TextGrid")))
        {
            Debug.Log("No phonemic information is present for audio " + plainFileName + ".wav. Please use AnalyzeAudio option to extract phonemic information from audio");
            return;
        }

        // change source to listener
        audioSource = Source.Speaker;

        string audioUrl;

        if (audioMode.Equals(AudioMode.CereVoice))
        {
            audioUrl = PathManager.GetAudioPath("audio.wav");
        }
        else
        {
            audioUrl = PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + currentRecording);
        }

        StartCoroutine(WaitForAudio(audioUrl));
    }

    /// <summary>
    /// Couroutine that waits for the wav audio file to be loaded
    /// </summary>
    /// <param name="audioUrl">The url of the generated audio</param>
    /// <returns></returns>
    IEnumerator WaitForAudio(string audioUrl)
    {
        WWW audioRequest = new WWW("file://" + audioUrl);
        yield return audioRequest;

        if (string.IsNullOrEmpty(audioRequest.error) == false)
        {
            print("ERROR: Audio Url Error");
            yield break;
        }

        audio.clip = audioRequest.GetAudioClip(false, false, AudioType.WAV);

        // change source to speaker
        if (String.IsNullOrEmpty(audio.clip.name))
        {
            audioSource = Source.Speaker;
        }
        else
        {
            audioSource = Source.Listener;
        }

        audio.PlayScheduled(0);
    }

    /// <summary>
    /// Fixed Update function which animates the character as long as the audio is playing
    /// </summary>
    void FixedUpdate()
    {
        if (audio.clip != null && audioSource.Equals(Source.Speaker))
        {
            if (audio.isPlaying)
            {
                // Audio timer
                audioElapsedTimer += Time.fixedDeltaTime;
                AnimatePhoneme(audioElapsedTimer);
                AnimateAlveoral(audioElapsedTimer);

                foreach (PhonemeInformation pi in currentPhonemes)
                {
                    if (!pi.apex)
                    {
                        PhonemeRise(pi, audioElapsedTimer);
                    }
                    else
                    {
                        PhonemeDecay(pi, audioElapsedTimer);
                    }
                }

                // Alveorals animation process 
                foreach (PhonemeInformation pi in currentAlveorals)
                {
                    if (!pi.apex)
                    {
                        PhonemeRise(pi, audioElapsedTimer);
                    }
                    else
                    {
                        PhonemeDecay(pi, audioElapsedTimer);
                    }
                }

                RefreshCurrentPhonemes();
                //RefreshGraphInformation();

                // Pitch and Intensity
                GetPitch();

                VisualizeTranscript(audioElapsedTimer);
            }
            else
            {
                // phonemes initializations
                currentIndex = 0;
                currentAlveoralIndex = 0;
                currentPhoneme = phonemeInformation[currentIndex];
                currentAlveoral = alveoralInformation[currentAlveoralIndex];

                // initialize phoneme apex
                foreach (PhonemeInformation pi in phonemeInformation)
                {
                    pi.apex = false;
                    pi.animationEnded = false;
                }

                foreach (PhonemeInformation pi in alveoralInformation)
                {
                    pi.apex = false;
                    pi.animationEnded = false;
                }

                // change source to listener
                audioSource = Source.Listener;

                // initialize audio timer
                audioElapsedTimer = 0.0f;

                // empty transcript visualization
                transcriptText.text = "";
                transcript = "";

                // testing
                //WriteDataToFile();
                timingsInformation.Clear();
            }
        }
    }

    /// <summary>
    /// Changes the current phoneme in the relative timestamp
    /// </summary>
    /// <param name="timeStamp"></param>
    void AnimatePhoneme(float timeStamp)
    {
        float nextPhonemeStart = 0;
        if (currentIndex < phonemeInformation.Count - 1)
        {
            nextPhonemeStart = phonemeInformation[currentIndex + 1].startingInterval;
        }
        
        if (timeStamp <= nextPhonemeStart)
        {
            return;
        }

        currentIndex += 1;

        if (currentIndex >= phonemeInformation.Count || phonemeInformation[currentIndex].text.Equals(Phoneme.Rest))
        {
            return;
        }

        SetCurrentPhoneme(phonemeInformation[currentIndex]);
        currentPhonemes.Add(currentPhoneme);
    }

    /// <summary>
    /// Changes the current alveoral (RRR, LLL, GK) in the relative timestamp
    /// </summary>
    /// <param name="timeStamp"></param>
    void AnimateAlveoral(float timeStamp)
    {
        float nextPhonemeStart = 0;
        if (currentAlveoralIndex < alveoralInformation.Count - 1)
        {
            nextPhonemeStart = alveoralInformation[currentAlveoralIndex + 1].startingInterval;
        }

        if (timeStamp <= nextPhonemeStart)
        {
            return;
        }

        currentAlveoralIndex += 1;

        if (currentAlveoralIndex >= alveoralInformation.Count || alveoralInformation[currentAlveoralIndex].text.Equals(Phoneme.Rest))
        {
            return;
        }

        SetCurrentAlveoral(alveoralInformation[currentAlveoralIndex]);
        currentAlveorals.Add(currentAlveoral);
    }

    /// <summary>
    /// Animates the viseme of the current phoneme: cPhoneme, within the current timeStamp
    /// Responsible for phoneme rise, from the starting phoneme timestamp till the phoneme's apex
    /// </summary>
    /// <param name="cPhoneme"></param>
    /// <param name="timeStamp"></param>
    void PhonemeRise(PhonemeInformation cPhoneme, float timeStamp)
    {
        List<BlendShape> blendShapes = phonemeBlendShapes[(Phoneme)cPhoneme.text];
        float duration, currentTimeStamp, timeStep, blendWeight, initialWeight, targetWeight;
        
        float riseStartTime = cPhoneme.startingInterval;
        float riseEndTime = cPhoneme.startingInterval + (cPhoneme.endingInterval - cPhoneme.startingInterval) * 0.75f;
        duration = riseEndTime - riseStartTime;
        currentTimeStamp = timeStamp - riseStartTime;
        currentTimeStamp = (float)Math.Truncate(currentTimeStamp * 1000) / 1000f;
        duration = (float)Math.Truncate(duration * 1000) / 1000f;

        timeStep = currentTimeStamp / duration;
        timeStep = EaseIn(timeStep);

        float sumWeight = 0;
        foreach (BlendShape bs in blendShapes)
        {
            if (considerFrequency)
            {
                // get sliding window in which the phoneme start time falls
                int windowIndex = (int)cPhoneme.startingInterval * 1000 / (int)slidingWindow;
                float meanPitch, pitchRatio, meanIntensity;

                if (Enum.IsDefined(typeof(Vowel), cPhoneme.text.ToString()))
                {
                    meanPitch = vowelPitchPerWindow[windowIndex];
                    pitchRatio = GetRatio(minVowelPitch, meanPitch, maxVowelPitch);
                    initialWeight = bs.weight * vowelPitchRatio;
                    meanIntensity = vowelIntensityPerWindow[windowIndex];
                    targetWeight = GetTargetWeightIntensity(initialWeight, cPhoneme, meanIntensity); 
                    float newWeight = GetTargetWeightPitch(initialWeight, cPhoneme, meanPitch);
                    targetWeight = (targetWeight + newWeight) / 2.0f;
                }
                else if (Enum.IsDefined(typeof(PlosiveFricative), cPhoneme.text.ToString()))
                {
                    meanPitch = consonantPitchPerWindow[windowIndex];
                    pitchRatio = GetRatio(minConsonantPitch, meanPitch, maxConsonantPitch);
                    initialWeight = bs.weight * consonantPitchRatio;
                    targetWeight = GetTargetWeightPitch(initialWeight, cPhoneme, meanPitch);
                }
                else
                {
                    targetWeight = bs.weight;
                }
            }
            else
            {
                targetWeight = bs.weight;
            }

            blendWeight = (timeStep < 1) ? targetWeight * timeStep : targetWeight;
            characterMesh.SetBlendShapeWeight(bs.index, blendWeight);
            bs.currentWeight = blendWeight;
            sumWeight += blendWeight;
        }

        // for curve creation
        //int phonemeIndex = indexInformation.IndexOf(indexInformation.SingleOrDefault(x => x.endingInterval == cPhoneme.endingInterval && x.startingInterval == cPhoneme.startingInterval));
        //graphInformation[phonemeIndex].weight = sumWeight / blendShapes.Count;

        if (timeStamp >= riseEndTime)
        {
            cPhoneme.apex = true;
        }

    }

    /// <summary>
    /// Animates the viseme of the current phoneme: cPhoneme, within the current timeStamp
    /// Responsible for phoneme decay, from the phoneme's apex till the ending phoneme timestamp
    /// </summary>
    /// <param name="cPhoneme"></param>
    /// <param name="timeStamp"></param>
    void PhonemeDecay(PhonemeInformation cPhoneme, float timeStamp)
    {
        List<BlendShape> blendShapes = phonemeBlendShapes[(Phoneme)cPhoneme.text];
        float duration, currentTimeStamp, timeStep, blendWeight;

        float decayStartTime = cPhoneme.startingInterval + (cPhoneme.endingInterval - cPhoneme.startingInterval) * 0.75f;
        float decayEndTime = cPhoneme.endingInterval;
        duration = decayEndTime - decayStartTime;
        currentTimeStamp = timeStamp - decayStartTime;
        currentTimeStamp = (float)Math.Truncate(currentTimeStamp * 1000) / 1000f;
        duration = (float)Math.Truncate(duration * 1000) / 1000f;

        timeStep = currentTimeStamp / duration;
        timeStep = EaseOut(timeStep);

        float sumWeight = 0;
        foreach (BlendShape bs in blendShapes)
        {
            blendWeight = (timeStep > 0) ? bs.currentWeight * timeStep : 0;
            characterMesh.SetBlendShapeWeight(bs.index, blendWeight);
            sumWeight += blendWeight;
        }

        // for curve creation
        //int phonemeIndex = indexInformation.IndexOf(indexInformation.SingleOrDefault(x => x.endingInterval == cPhoneme.endingInterval && x.startingInterval == cPhoneme.startingInterval));
        //graphInformation[phonemeIndex].weight = sumWeight / blendShapes.Count;

        if (timeStamp >= decayEndTime)
        {
            cPhoneme.animationEnded = true;
        }
    }

    /// <summary>
    /// Ease in function responsible for phoneme's rise visualization
    /// Could be either quadratic or exponential depending on user's parameters
    /// </summary>
    /// <param name="timeStep"></param>
    /// <returns></returns>
    float EaseIn(float timeStep)
    {
        if (curve.Equals(CurveMode.Quadratic))
        {
            return timeStep * timeStep;
        }
        else
        {
            float val = (float)Math.Exp(timeStep);
            val = RangeTransformation.Transform(val, (float)Math.Exp(0), (float)Math.Exp(1), 0f, 1f);
            return val;
        }
    }

    /// <summary>
    /// Ease out function responsible for phoneme's decay visualization
    /// Could be either quadratic or exponential depending on user's parameters
    /// </summary>
    /// <param name="timeStep"></param>
    /// <returns></returns>
    float EaseOut(float timeStep)
    {
        if (curve.Equals(CurveMode.Quadratic))
        {
            return (1-timeStep) * (2 - (1-timeStep));
        }
        else
        {
            float val = (float)Math.Exp(1 - timeStep);
            val = RangeTransformation.Transform(val, (float)Math.Exp(0), (float)Math.Exp(1), 0f, 1f);
            return val;
        }
        
    }

    /// <summary>
    /// Returns the new target weight depending on the pitch of the audio (frequency)
    /// </summary>
    /// <param name="weight"></param>
    /// <returns></returns>
    float GetTargetWeightPitch(float weight, PhonemeInformation phoneme, float meanPitch)
    {
        float newWeight;
        if (Enum.IsDefined(typeof(Vowel), phoneme.text.ToString()))
        {
            if (phoneme.meanPitch == 0)
            {
                return weight;
            }
            else if (phoneme.meanPitch < meanPitch)
            {
                newWeight = RangeTransformation.Transform(phoneme.meanPitch, minVowelPitch, meanPitch, 0.0f, weight);
            }
            else
            {
                newWeight = RangeTransformation.Transform(phoneme.meanPitch, meanPitch, maxVowelPitch, weight, 100.0f);
            }
        }
        else if(Enum.IsDefined(typeof(PlosiveFricative), phoneme.text.ToString()))
        {
            if (phoneme.meanPitch == 0)
            {
                return weight;
            }
            else if (phoneme.meanPitch < meanPitch)
            {
                newWeight = RangeTransformation.Transform(phoneme.meanPitch, minConsonantPitch, meanPitch, 0.0f, weight);
            }
            else
            {
                newWeight = RangeTransformation.Transform(phoneme.meanPitch, meanPitch, maxConsonantPitch, weight, 100.0f);
            }
        }
        else
        {
            return weight;
        }
        return newWeight;
    }

    /// <summary>
    /// Returns the new target weight depending on the intensity of the audio 
    /// </summary>
    /// <param name="weight"></param>
    /// <returns></returns>
    float GetTargetWeightIntensity(float weight, PhonemeInformation phoneme, float meanIntensity)
    {
        float newWeight;
        if (Enum.IsDefined(typeof(Vowel), phoneme.text.ToString()))
        {
            if (phoneme.meanIntensity == 0)
            {
                return weight;
            }
            else if (phoneme.meanIntensity < meanIntensity)
            {
                newWeight = RangeTransformation.Transform(phoneme.meanIntensity, minVowelIntensity, meanIntensity, 0.0f, weight);
            }
            else
            {
                newWeight = RangeTransformation.Transform(phoneme.meanIntensity, meanIntensity, maxVowelIntensity, weight, 100.0f);
            }
        }
        else if (Enum.IsDefined(typeof(PlosiveFricative), phoneme.text.ToString()))
        {
            if (phoneme.meanIntensity == 0)
            {
                return weight;
            }
            else if (phoneme.meanIntensity < meanIntensity)
            {
                newWeight = RangeTransformation.Transform(phoneme.meanIntensity, minConsonantIntensity, meanIntensity, 0.0f, weight);
            }
            else
            {
                newWeight = RangeTransformation.Transform(phoneme.meanIntensity, meanIntensity, maxConsonantIntensity, weight, 100.0f);
            }
        }
        else
        {
            return weight;
        }
        return newWeight;
    }

    /// <summary>
    /// Calculates new onset and offset values for each phoneme depending on 
    /// - User's input (phoneme over phoneme influence)
    /// - Mean duration of each word that includes the respective phoneme
    /// </summary>
    public void RearrangePhonemeTimings()
    {
        float offset;
        foreach (PhonemeInformation pi in phonemeInformation)
        {
            if (pi.onset_offset > onsetThreshold)
            {
                if (Enum.IsDefined(typeof(LipHeavy), pi.text.ToString()))
                {
                    offset = 0.15f;
                }
                else
                {
                    offset = 0.12f;
                }

                pi.startingInterval = pi.startingInterval - offset;
                pi.endingInterval = pi.endingInterval + offset;
            }
            else
            {
                pi.backwardInfluence = GetInfluence(pi.text.ToString(), pi.phonemicInfluenceBack, pi.influence, Enum.IsDefined(typeof(LipHeavy), pi.text.ToString()));
                pi.forwardInfluence = GetInfluence(pi.text.ToString(), pi.phonemicInfluenceForw, pi.influence, Enum.IsDefined(typeof(LipHeavy), pi.text.ToString()));

                //print(pi.text + " offset " + pi.onset_offset + " influence " + pi.backwardInfluence + " apotelesma " + (pi.startingInterval - (pi.onset_offset * pi.backwardInfluence)));
                pi.startingInterval = pi.startingInterval - (pi.onset_offset * pi.backwardInfluence);

                //print(pi.text + " prosthetw " + (pi.onset_offset * pi.forwardInfluence) + " apo " + pi.endingInterval + " apotelesma " + (pi.endingInterval + (pi.onset_offset * pi.forwardInfluence)));
                pi.endingInterval = pi.endingInterval + (pi.onset_offset * pi.forwardInfluence);
            }
        }

        AddCurveInformation();
    }

    /// <summary>
    /// Used for debugging
    /// </summary>
    /// <param name="text"></param>
    void DebugPrint(string text)
    {
        print(text);

        foreach (PhonemeInformation pi in phonemeInformation)
        {
            print(pi.text + " " + pi.startingInterval + " " + pi.endingInterval);
        }
    }

    /// <summary>
    /// Sets the curve type according to user's input
    /// </summary>
    /// <param name="curveTypeIndex"></param>
    public void SetCurveType(int curveTypeIndex)
    {
        if (curveTypeIndex == 1)
        {
            curve = CurveMode.Quadratic;
        }
        else 
        {
            curve = CurveMode.Exponential;
        }
    }

    /// <summary>
    /// Sets the considerFrequency value to true/false 
    /// If true frequency will be considered during animation
    /// If false frequency will not be taken into account during animation
    /// </summary>
    /// <param name="toggle"></param>
    public void SetFrequencyToggle(bool toggle)
    {
        considerFrequency = toggle;
    }

    /// <summary>
    /// Returns the influence weight for a respective phoneme
    /// </summary>
    /// <param name="phoneme"></param>
    /// <param name="influence"></param>
    /// <param name="initial"></param>
    /// <param name="isHeavy"></param>
    /// <returns></returns>
    float GetInfluence(string phoneme, Influence influence, float initial, bool isHeavy)
    {
        float weight = 1;
        if (isHeavy)
        {
            weight *= 1.2f;
        }

        if(influence.Equals(Influence.CC)) // consonant over consonant
        {
            return consonantOverConsonant * weight;
        }
        else if (influence.Equals(Influence.CV)) // consonant over vowel
        {
            return consonantOverVowel * weight;
        }
        else if (influence.Equals(Influence.VC)) // vowel over consonant
        {
            return vowelOverConsonant * weight;
        }
        else if (influence.Equals(Influence.VV)) // vowel over vowel
        {
            return vowelOverVowel * weight * initial;
        }
        
        return initial;
    }

    /// <summary>
    /// Used to print curve information. Can be used later to visualize the animation curves
    /// </summary>
    void AddCurveInformation()
    {
        indexInformation.Clear();
        graphInformation.Clear();

        foreach (PhonemeInformation pi in phonemeInformation)
        {
            if (!pi.text.Equals(Phoneme.Rest))
            {
                indexInformation.Add(pi);
                graphInformation.Add(new GraphInfo((Phoneme)pi.text, -1));
            }
        }

        foreach (PhonemeInformation pi in alveoralInformation)
        {
            if (!pi.text.Equals(Phoneme.Rest))
            {
                indexInformation.Add(pi);
                graphInformation.Add(new GraphInfo((Phoneme)pi.text, -1));
            }
        }
    }

    /// <summary>
    /// Sets the current phoneme to the desired one
    /// </summary>
    /// <param name="phoneme">The phoneme to be set as current</param>
    void SetCurrentPhoneme(PhonemeInformation phoneme)
    {
        currentPhoneme = phoneme;
    }

    /// <summary>
    /// Sets the current alveoral to the desired one
    /// </summary>
    /// <param name="phoneme">The phoneme to be set as current</param>
    void SetCurrentAlveoral(PhonemeInformation phoneme)
    {
        currentAlveoral = phoneme;
    }

    /// <summary>
    /// Reset current phonemes
    /// </summary>
    void RefreshCurrentPhonemes()
    {
        currentPhonemes.RemoveAll(x => x.animationEnded == true);
        currentAlveorals.RemoveAll(x => x.animationEnded == true);
    }

    /// <summary>
    /// Refreshes graph information used for curve visualization
    /// </summary>
    void RefreshGraphInformation()
    {
        string s = audioElapsedTimer + "";
        foreach (GraphInfo gi in graphInformation)
        {
            if (gi.weight > -1)
            {
                s += "\t" + gi.weight;
            }
            else
            {
                s += "\t";
            }
            gi.weight = -1;
        }
        timingsInformation.Add(s);
    }

    void PrintLog()
    {
        print("Average Vowel Pitch " + meanVowelPitch);
        print("Average Consonant Pitch " + meanConsonantPitch);
    }

    /// <summary>
    /// Writes curve data to a text file
    /// </summary>
    void WriteDataToFile()
    {
        using (StreamWriter sw = new StreamWriter(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + currentRecording.Replace(".wav","") + "_curveInfo" + this.name[this.name.Length -1] +".txt")))
        {
            sw.Write("Time");
            foreach (GraphInfo gi in graphInformation)
            {
                sw.Write("\t" + gi.phoneme);
            }
            sw.WriteLine();

            foreach (string s in timingsInformation)
            {
                sw.WriteLine(s);
            }
            sw.Close();
        }
    }

    /// <summary>
    /// Initializes the 3D models' blendshapes to default values
    /// </summary>
    void InitializeModel()
    {
        for (int i = 0; i < characterMesh.sharedMesh.blendShapeCount; i++)
        {
            characterMesh.SetBlendShapeWeight(i, 0);
        }
    }

    /// <summary>
    /// Sets minimum and maximum values for frequencies
    /// Changing these values will result to different animation results
    /// Values should be set accordingly depending on the audio's mean pitch
    /// </summary>
    void SetMinMaxFrequencyValues()
    {
        if (gender.Equals(Gender.Male))
        {
            //minFrequencyRange = 85f;
            minFrequencyRange = 45f;
            maxFrequencyRange = 180f;
        }
        else
        {
            minFrequencyRange = 80f;
            //minFrequencyRange = 165f;
            maxFrequencyRange = 220f;
        }
    }

    /// <summary>
    /// Interpolate all blendshapes of the model related to visemes
    /// from their current values to default ones
    /// </summary>
    /// <param name="phoneme"></param>
    /// <param name="timeStamp"></param>
    void InitializeBlendShapes(Phoneme phoneme, float timeStamp)
    {
        List<BlendShape> blendShapes = phonemeBlendShapes[phoneme];

        foreach (BlendShape bs in blendShapes)
        {
            characterMesh.SetBlendShapeWeight(bs.index, 0);
        }
    }

    /// <summary>
    /// Prints the audio's transcript on screen
    /// </summary>
    /// <param name="timeStamp"></param>
    void VisualizeTranscript(float timeStamp)
    {
        WordInformation word;

        if (audioMode.Equals(AudioMode.CereVoice))
        {
            word = wordInfo.Find(x => x.startingInterval <= timeStamp && x.endingInterval >= timeStamp);
        }
        else
        {
            word = generatedWordInfo.Find(x => x.startingInterval <= timeStamp && x.endingInterval >= timeStamp);
        }

        if (word == null)
        {
            return;
        }

        if (currentWord == null || !word.Equals(currentWord))
        {
            currentWord = word;
            transcript += currentWord.text + " ";
            transcriptText.text = transcript;
        }

        if (transcript.Length > maxTranscriptLength)
        {
            transcript = currentWord.text + " ";
        }

    }

    /// <summary>
    /// Sets the current recording to the selected one
    /// </summary>
    /// <param name="value"></param>
    public void SetCurrentRecording(int value)
    {
        currentRecording = AudioDatabase.GetAudioByName(value);
    }

    /// <summary>
    /// Returns the pitch value in the current timestamp
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    float GetPitchByTimestamp(float timeStamp)
    {
        timeStamp = (float)Math.Round(timeStamp, 2);
        float pitch = frequencies[timeStamp];
        return pitch;
    }

    /// <summary>
    /// Retruns the intensity value in the current timestamp
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    float GetIntensityByTimestamp(float timeStamp)
    {
        timeStamp = (float)Math.Round(timeStamp, 2);
        float intensity = decibels[timeStamp];
        return intensity;
    }

    /// <summary>
    /// Gets pitch and intensity values in the current timestamp
    /// </summary>
    void GetPitch()
    {
        try
        {
            timedPitch = GetPitchByTimestamp(audioElapsedTimer);
            timedIntensity = GetIntensityByTimestamp(audioElapsedTimer);
        }
        catch (KeyNotFoundException e)
        { }
    }

    /// <summary>
    /// Maps each phoneme to its corresponding blendshapes based on 
    /// either the XML or the inspector configuration
    /// </summary>
    void PhonemeBlendShapeMapping()
    {
        phonemeBlendShapes = new Dictionary<Phoneme, List<BlendShape>>();
        Phoneme p;

        // read the XML configuration file
        XDocument config = XDocument.Load(PathManager.GetXMLDataPath(characterMesh.transform.root.name + "-phonemeMapping.xml"));
        var attributes = config.Root.Descendants("ATTR");
        foreach (var attr in attributes)
        {
            p = (Phoneme)Enum.Parse(typeof(Phoneme), attr.Element("Phoneme").Value);
            phonemeBlendShapes.Add(p, new List<BlendShape>());

            var blendShapeAttr = attr.Elements("BlendShape");
            foreach (var bsa in blendShapeAttr)
            {
                int index = Int32.Parse(bsa.Element("Index").Value);
                float weight = float.Parse(bsa.Element("Weight").Value);
                BlendShape bs = new BlendShape();
                bs.index = index;
                bs.setBlendShapeWeight(weight);
                bs.name = blendShapes[index].name;
                phonemeBlendShapes[p].Add(bs);
            }
        }        
        
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
        XDocument config = XDocument.Load(PathManager.GetXMLDataPath(characterMesh.transform.root.name + "-diphoneMapping.xml"));
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
    /// Maps each emotion to its corresponding blendshapes based on 
    /// either the XML or the inspector configuration
    /// </summary>
    void EmotionBlendShapeMapping()
    {
        emotionBlendShapes = new Dictionary<Emotion, List<BlendShape>>();
        Emotion e;

        // read the XML configuration file
        XDocument config = XDocument.Load(PathManager.GetXMLDataPath(characterMesh.transform.root.name + "-emotionMapping.xml"));
        var attributes = config.Root.Descendants("ATTR");
        foreach (var attr in attributes)
        {
            e = (Emotion)Enum.Parse(typeof(Emotion), attr.Element("Emotion").Value);
            emotionBlendShapes.Add(e, new List<BlendShape>());

            var blendShapeAttr = attr.Elements("BlendShape");
            foreach (var bsa in blendShapeAttr)
            {
                int index = Int32.Parse(bsa.Element("Index").Value);
                float weight = float.Parse(bsa.Element("Weight").Value);
                BlendShape bs = new BlendShape();
                bs.index = index;
                bs.setBlendShapeWeight(weight);
                bs.name = blendShapes[index].name;
                emotionBlendShapes[e].Add(bs);
            }
        }
    }

    /// <summary>
    /// Converts a list into a dictionary - Phoneme BlendShapes
    /// </summary>
    void ListToDictionary(List<PhonemeBlendShape> phonemeMappings)
    {
        phonemeBlendShapes = new Dictionary<Phoneme, List<BlendShape>>();

        foreach (PhonemeBlendShape pbs in phonemeMappings)
        {
            phonemeBlendShapes.Add(pbs.phoneme, pbs.blendShapes);
        }
    }

    /// <summary>
    /// Converts a list into a dictionary - Emotion BlendShapes
    /// </summary>
    void ListToDictionary(List<EmotionBlendShape> phonemeMappings)
    {
        emotionBlendShapes = new Dictionary<Emotion, List<BlendShape>>();

        foreach (EmotionBlendShape ebs in phonemeMappings)
        {
            emotionBlendShapes.Add(ebs.emotion, ebs.blendShapes);
        }
    }

    /// <summary>
    /// Returns the current character mesh of the 3D model
    /// </summary>
    /// <returns></returns>
    public SkinnedMeshRenderer getCharacterMesh()
    {
        return characterMesh;
    }

    /// <summary>
    /// Sets min, mean and max pitch values for vowels depending on the audio analysis of OpenSmile
    /// For more information see SpeechAnalysis script
    /// </summary>
    /// <param name="min"></param>
    /// <param name="mean"></param>
    /// <param name="max"></param>
    public static void SetVowelPitches(float min, float mean, float max)
    {
        minVowelPitch = min;
        meanVowelPitch = mean;
        maxVowelPitch = max;
        vowelPitchRatio = GetRatio(minFrequencyRange, meanVowelPitch, maxFrequencyRange);
    }

    /// <summary>
    /// Sets min, mean and max intensity values for vowels depending on the audio analysis of OpenSmile
    /// For more information see SpeechAnalysis script
    /// </summary>
    /// <param name="min"></param>
    /// <param name="mean"></param>
    /// <param name="max"></param>
    public static void setVowelIntensities(float min, float mean, float max)
    {
        minVowelIntensity = min;
        meanVowelIntensity = mean;
        maxVowelIntensity = max;
    }

    /// <summary>
    /// Sets min, mean and max pitch values for consonants depending on the audio analysis of OpenSmile
    /// For more information see SpeechAnalysis script
    /// </summary>
    /// <param name="min"></param>
    /// <param name="mean"></param>
    /// <param name="max"></param>
    public static void SetConsonantPitches(float min, float mean, float max)
    {
        minConsonantPitch = min;
        meanConsonantPitch = mean;
        maxConsonantPitch = max;
        consonantPitchRatio = GetRatio(minFrequencyRange, meanConsonantPitch, maxFrequencyRange);
    }

    /// <summary>
    /// Returns the ratio of mean value compared to min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="mean"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    static float GetRatio(float min, float mean, float max)
    {
        float ratio = RangeTransformation.Transform(mean, min, max, 0, 1);
        if (ratio <= 0)
        {
            return 0.2f;
        }
        else if (ratio > 1)
        {
            return 1;
        }
        else
        {
            return ratio;
        }
    }

    /// <summary>
    /// Sets mean pitches for specified window intervals 
    /// </summary>
    /// <param name="mean"></param>
    public static void SetMeanPitches(List<float> meanVowel, List<float> meanConsonant)
    {
        vowelPitchPerWindow = meanVowel;
        consonantPitchPerWindow = meanConsonant;
    }

    /// <summary>
    /// Sets min, mean and max intensity values for consonants depending on the audio analysis of OpenSmile
    /// For more information see SpeechAnalysis script
    /// </summary>
    /// <param name="min"></param>
    /// <param name="mean"></param>
    /// <param name="max"></param>
    public static void SetConsonantIntensities(float min, float mean, float max)
    {
        minConsonantIntensity = min;
        meanConsonantIntensity = mean;
        maxConsonantIntensity = max;
    }

    /// <summary>
    /// Sets mean intensities for specified window intervals 
    /// </summary>
    /// <param name="mean"></param>
    public static void SetMeanIntensities(List<float> meanVowel, List<float> meanConsonant)
    {
        vowelIntensityPerWindow = meanVowel;
        consonantIntensityPerWindow = meanConsonant;
    }

}
