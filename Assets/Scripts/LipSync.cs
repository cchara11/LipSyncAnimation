using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using System.IO;

///-----------------------------------------------------------------
///   Class:        LipSync.cs
///   Description:  The base class of the lip sync component 
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

[Serializable]
public class LipSync : MonoBehaviour {
    
	public enum BlendState
    {
        None,
        Waiting,
        Resting,
        Blending,
        Stopping
    }

    public enum BlendMode
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }
    
    public enum ReadMode
    {
        Inspector,
        XML
    }

    public SkinnedMeshRenderer characterMesh; // character on which lip sync will be performed
    public float blendSpeed;

    [Header("Read Mode")]
    public ReadMode readMode;

    public bool IsPlaying { get; private set; } // check if any animation is playing
    public bool IsPaused { get; private set; } // check if the animation is in a pause state
    
    private int currentPhonemeIndex;

    // BlendShapes
    List<BlendShape> blendShapes; // all blendShapes of the model
    public Dictionary<Phoneme, List<BlendShape>> phonemeBlendShapes; // map phonemes to blendshapes
    public Dictionary<Phoneme, List<Phoneme>> diphonePhonemes; // map phonemes to diphones (dynamic visemes)
    public Dictionary<Emotion, List<BlendShape>> emotionBlendShapes; // map emotions to blendshapes
    
    // Audio
    AudioSource audio;
    float audioCurrentTime;
    Source audioSource;

    // Phonemes
    [HideInInspector]
    public List<PhonemeBlendShape> staticVisemes;
    [HideInInspector]
    public List<DiphonePhonemes> dynamicVisemes;
    private List<PhonemeInfo> phonemeInformation;
    string phonemeFile;
    Phoneme currentPhoneme;
    PhonemeInfo newCurrentPhoneme;
    int currentIndex = 0; // current phoneme index

    // Emotions
    [HideInInspector]
    public List<EmotionBlendShape> emotions;
    public static List<ProsodyComponent> prosody;
    EmotionBlendShape currentEmotion;

    // Time
    float audioElapsedTimer = 0.0f;

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
            EmotionBlendShapeMapping();
        }
        else
        {
            ListToDictionary(staticVisemes);
            ListToDictionary(emotions);
        }
    }

    void Start()
    {
        // initialize 3D model
        InitializeModel();

        // phonemes
        ReadPhonemes();
        phonemeInformation = CoarticulationEnhancement.AddDynamicVisemes(phonemeInformation, diphonePhonemes);

        // emotions
        currentEmotion = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Neutral));

        // audio
        audio = GetComponent<AudioSource>();
        audioSource = Source.Listener;

        if (staticVisemes.Count == 0)
        {
            return;
        }

        // set current phoneme to first phoneme which corresponds to the phoneme Rest
        SetCurrentPhoneme(Phoneme.Rest);
        if (phonemeInformation.Count > 0)
        {
            newCurrentPhoneme = phonemeInformation[currentIndex];
        }
        else
        {
            newCurrentPhoneme = null;
        }
        
    }

    /// <summary>
    /// Plays audio from the generated audio source
    /// </summary>
    public void PlayAudio()
    {
        // change source to listener
        audioSource = Source.Speaker;

        string audioUrl = PathManager.getAudioPath("audio.wav");
        ReadPhonemes();
        phonemeInformation = CoarticulationEnhancement.AddDynamicVisemes(phonemeInformation, diphonePhonemes);

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
        
        if (!audio.isPlaying)
        {
            currentIndex = 0;

            // change source to listener
            audioSource = Source.Listener;
        }
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
                // audio timer
                audioElapsedTimer += Time.deltaTime;
                Animate(audioElapsedTimer);
                
                //// convert timeSamples to time
                audioCurrentTime = audio.timeSamples / (float)audio.clip.frequency; // audio current time

            }
            else
            {
                // re-initialize current index of phoneme
                currentIndex = 0;
                newCurrentPhoneme = phonemeInformation[currentIndex];

                // initialize emotions blendshapes
                InitializeEmotions();

                // change source to listener
                audioSource = Source.Listener;

                // initialize audio timer
                audioElapsedTimer = 0.0f;
            }
        }
        
    }
    
    /// <summary>
    /// Animate function that is used to drive the character's animation 
    /// based on the lip sync condiguration and the generated audio information
    /// </summary>
    /// <param name="timeStamp">The current audio timestamp</param>
    public void Animate(float timeStamp)
    {
        // blending
        float blendWeight;

        // emotions
        EmotionBlendShape emotionBlendShape;

        foreach (PhonemeInfo pi in phonemeInformation)
        {
            if (timeStamp > pi.starting_time && timeStamp < pi.ending_time)
            {
                // check if phoneme exists
                if (!pi.phoneme.Equals(Phoneme.Rest) && !(pi.phoneme.Equals(null)))
                { 
                    if (!currentPhoneme.Equals(pi.phoneme))
                    {
                        InitializeBlendShapes((Phoneme)pi.phoneme, timeStamp);
                    }
                    SetCurrentPhoneme((Phoneme)pi.phoneme);

                    List<BlendShape> blendShapes = phonemeBlendShapes[(Phoneme)pi.phoneme];

                    float alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

                    foreach (BlendShape bs in blendShapes)
                    {
                        blendWeight = Mathf.Lerp(0, bs.weight, timeStamp * blendSpeed);
                        characterMesh.SetBlendShapeWeight(bs.index, blendWeight);
                        bs.currentWeight = blendWeight;
                    }
                }
                break;
            }

            InitializeBlendShapes(currentPhoneme, timeStamp);

            if (SpeechAnalysis.pitchMedian > 0)
            {
                if (SpeechAnalysis.pitchMedian > 220)
                {
                    emotionBlendShape = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Angry));
                }
                else if (SpeechAnalysis.pitchMedian < 180)
                {
                    emotionBlendShape = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Sad));
                }
                else
                {
                    emotionBlendShape = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Happy));
                }
            }
            else
            {
                emotionBlendShape = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Neutral));
            }

            currentEmotion = emotionBlendShape;

            foreach (BlendShape bs in currentEmotion.blendShapes)
            {
                blendWeight = Mathf.Lerp(0, bs.weight, timeStamp * blendSpeed);
                characterMesh.SetBlendShapeWeight(bs.index, blendWeight);
            }
        }
    }

    /// <summary>
    /// Sets the current phoneme to the desired one
    /// </summary>
    /// <param name="phoneme">The phoneme to be set as current</param>
    void SetCurrentPhoneme(Phoneme phoneme)
    {
        currentPhoneme = phoneme;
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
    /// Interpolate all blendshapes of the model related to visemes
    /// from their current values to default ones
    /// </summary>
    /// <param name="phoneme"></param>
    /// <param name="timeStamp"></param>
    void InitializeBlendShapes(Phoneme phoneme, float timeStamp)
    {
        float blendWeight;
        List<BlendShape> blendShapes = phonemeBlendShapes[phoneme];

        foreach (BlendShape bs in blendShapes)
        {
            blendWeight = Mathf.Lerp(bs.currentWeight, 0, timeStamp * blendSpeed);
            characterMesh.SetBlendShapeWeight(bs.index, blendWeight);
        }
    }

    /// <summary>
    /// Interpolate all blendshapes of the model related to emotions
    /// from their current values to default ones
    /// </summary>
    void InitializeEmotions()
    {
        foreach (BlendShape bs in currentEmotion.blendShapes)
        {
            characterMesh.SetBlendShapeWeight(bs.index, 0);
        }
        currentEmotion = emotions.FirstOrDefault(s => s.emotion.Equals(Emotion.Neutral));
    }

    /// <summary>
    /// Stops the lip sync component 
    /// </summary>
    /// <param name="stop"></param>
    public void StopLipSync(bool stop)
    {
        if (IsPlaying)
        {
            IsPlaying = false;
        }
    }

    /// <summary>
    /// Reads the phoneme information that was produced by the text-to-speech engine
    /// This information is mapped to the synthesized audio
    /// </summary>
    void ReadPhonemes()
    {
        phonemeFile = PathManager.getDataPath("phonemes.txt");
        if (File.Exists(phonemeFile))
        {
            phonemeInformation = PhonemeReader.ReadPhonemeTimings(phonemeFile);
        }
        
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
        XDocument config = XDocument.Load(PathManager.getDataPath("phonemeMapping.xml"));
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
                BlendShape bs = blendShapes[index];
                bs.setBlendShapeWeight(weight);
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
        XDocument config = XDocument.Load(PathManager.getDataPath("diphoneMapping.xml"));
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
        XDocument config = XDocument.Load(PathManager.getDataPath("emotionMapping.xml"));
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
                BlendShape bs = blendShapes[index];
                bs.setBlendShapeWeight(weight);
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

}
