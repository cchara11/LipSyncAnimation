using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

///---------------------------------------------------------------------
///   Class:        MAUS.cs
///   Description:  The base class of the maus audio analyzer component 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///---------------------------------------------------------------------

public class MAUS : MonoBehaviour
{
    public Language language;
    MAUSService service;
    PhonemeAnalyzer analyzer;
    SpeechAnalysis speechAnalysis;
    string audioName, transcriptName, textgridName;
    AudioClip clip;
    List<PhonemeInformation> phonemeTimings;

    /// <summary>
    /// Analyzes the audio using MAUS
    /// For more information see MAUSService script
    /// </summary>
    public void AnalyzeAudio()
    {
        string plainName = audioName.Replace(".wav", "");
        transcriptName = plainName + ".txt";

        // check if corresponding transcript exists
        if (!File.Exists(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + transcriptName)))
        {
            Debug.Log("No transcript provided from file " + audioName + ". Please provide corresponding transcript");
            return;
        }

        // check if corresponding textgrid exists
        // if not use MAUSService to generate the textgrid file
        textgridName = plainName + ".TextGrid";

        if (!File.Exists(PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + textgridName)))
        {
            service = new MAUSService();
            service.AnalyzeAudio(plainName, language);
            StartCoroutine(DownloadFile(plainName));
        }
        else
        {
            analyzer = new PhonemeAnalyzer(AudioMode.Natural, clip);
            phonemeTimings = analyzer.ManageTextGridInfo(textgridName);
            speechAnalysis = new SpeechAnalysis(audioName, phonemeTimings);
            speechAnalysis.AnalyzeAudio();
        }
        
    }

    /// <summary>
    /// Sets current audio name depending on the selected audio
    /// </summary>
    /// <param name="index"></param>
    public void SetAudioName(int index)
    {
        audioName = AudioDatabase.GetAudioByName(index);
        clip = AudioDatabase.GetAudioByClip(index);
    }

    /// <summary>
    /// Downloads the output file of MAUS service
    /// </summary>
    /// <param name="audioName"></param>
    /// <returns></returns>
    IEnumerator DownloadFile(string audioName)
    {
        string resultFileName = PathManager.GetResourcesPath("response.xml");
        XDocument response = XDocument.Load(resultFileName);
        string downloadLink = response.Root.Element("downloadLink").Value;

        WWW www = new WWW(downloadLink);
        yield return www;

        while (!www.isDone)
        {
            UnityEngine.Debug.Log(www.progress);
            yield return null;
        }
        
        //PathManager.GetResourcesPath(audioName)
        string savePath = PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + audioName + ".TextGrid");

        byte[] bytes = www.bytes;

        File.WriteAllBytes(savePath, bytes);
        
        analyzer = new PhonemeAnalyzer(AudioMode.Natural, clip);
        phonemeTimings = analyzer.ManageTextGridInfo(textgridName);
        speechAnalysis = new SpeechAnalysis(audioName, phonemeTimings);
        speechAnalysis.AnalyzeAudio();
    }
}
