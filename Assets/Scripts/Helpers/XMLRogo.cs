using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Text;

///-----------------------------------------------------------------
///   Class:        XMLRogo.cs
///   Description:  Class responsible for rogo input generation 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public class XMLRogo
{
    public InputField inputText;
    AudioClip currentClip;

    public XMLRogo(AudioClip clip)
    {
        currentClip = clip;
    }

    /// <summary>
    /// Generates XML file used for Rogo input
    /// </summary>
    public void GenerateRogoXML(List<WordInformation> words, List<PhonemeInformation> phonemeInfo)
    {
        StringBuilder sb = new StringBuilder();
        foreach (WordInformation w in words)
        {
            sb.Append(w.text);
            if (!w.Equals(words[words.Count - 1]))
            {
                sb.Append(" ");
            }
        }

        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode mainNode = xmlDoc.CreateElement("LipSyncData");
        xmlDoc.AppendChild(mainNode);

        XmlNode versionNode = xmlDoc.CreateElement("version");
        versionNode.AppendChild(xmlDoc.CreateTextNode("1.3"));
        mainNode.AppendChild(versionNode);

        XmlNode transcriptNode = xmlDoc.CreateElement("transcript");
        transcriptNode.AppendChild(xmlDoc.CreateTextNode(sb.ToString()));
        mainNode.AppendChild(transcriptNode);


        XmlNode phonemeNode = xmlDoc.CreateElement("phonemes");

        foreach (PhonemeInformation pi in phonemeInfo)
        {
            XmlNode markerNode = xmlDoc.CreateElement("marker");
            XmlAttribute time = xmlDoc.CreateAttribute("time");
            //time.Value = pi.startingInterval + "";
            time.Value = (pi.startingInterval + pi.endingInterval) / 2 + "";
            markerNode.Attributes.Append(time);
            XmlAttribute intensity = xmlDoc.CreateAttribute("intensity");
            intensity.Value = "1";
            markerNode.Attributes.Append(intensity);
            XmlAttribute useRandomness = xmlDoc.CreateAttribute("useRandomness");
            useRandomness.Value = "False";
            markerNode.Attributes.Append(useRandomness);
            XmlAttribute intensityRandomness = xmlDoc.CreateAttribute("intensityRandomness");
            intensityRandomness.Value = "0.1";
            markerNode.Attributes.Append(intensityRandomness);
            XmlAttribute blendableRandomness = xmlDoc.CreateAttribute("blendableRandomness");
            blendableRandomness.Value = "0.3";
            markerNode.Attributes.Append(blendableRandomness);
            XmlAttribute bonePositionRandomness = xmlDoc.CreateAttribute("bonePositionRandomness");
            bonePositionRandomness.Value = "0.3";
            markerNode.Attributes.Append(bonePositionRandomness);
            XmlAttribute boneRotationRandomness = xmlDoc.CreateAttribute("boneRotationRandomness");
            boneRotationRandomness.Value = "0.3";
            markerNode.Attributes.Append(boneRotationRandomness);
            XmlAttribute sustain = xmlDoc.CreateAttribute("sustain");
            sustain.Value = "False";
            markerNode.Attributes.Append(sustain);
            XmlAttribute phoneme = xmlDoc.CreateAttribute("phoneme");
            string phonemeString = pi.text.ToString();
            phoneme.Value = RogoPhonemeInfo.MapRogoPhoneme(phonemeString).ToString();
            markerNode.Attributes.Append(phoneme);

            phonemeNode.AppendChild(markerNode);
        }

        XmlNode lengthNode = xmlDoc.CreateElement("length");
        lengthNode.AppendChild(xmlDoc.CreateTextNode(currentClip.length + ""));
        mainNode.AppendChild(lengthNode);

        mainNode.AppendChild(phonemeNode);

        XmlNode emotionNode = xmlDoc.CreateElement("emotions");
        mainNode.AppendChild(emotionNode);
        XmlNode gesturesNode = xmlDoc.CreateElement("gestures");
        mainNode.AppendChild(gesturesNode);

        xmlDoc.Save(PathManager.GetResourcesRogoPath(currentClip.name + ".xml"));

        // copy audio file to rogodigital path
        if (File.Exists(PathManager.GetRogoDigitalPath("audio.wav")))
        {
            File.Delete(PathManager.GetRogoDigitalPath("audio.wav"));
        }
#if UNITY_EDITOR
        FileUtil.CopyFileOrDirectory(PathManager.GetAudioPath("audio.wav"), PathManager.GetRogoDigitalPath("audio.wav"));
#endif
    }

    /// <summary>
    /// Generates XML file used for Rogo input
    /// Old implementation
    /// </summary>
    public void GenerateRogoXML()
    {
        string output_phonemes_path = PathManager.GetDataPath("phonemes.txt");

        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode mainNode = xmlDoc.CreateElement("LipSyncData");
        xmlDoc.AppendChild(mainNode);

        XmlNode versionNode = xmlDoc.CreateElement("version");
        versionNode.AppendChild(xmlDoc.CreateTextNode("1.3"));
        mainNode.AppendChild(versionNode);

        XmlNode transcriptNode = xmlDoc.CreateElement("transcript");
        transcriptNode.AppendChild(xmlDoc.CreateTextNode(inputText.text));
        mainNode.AppendChild(transcriptNode);
        

        XmlNode phonemeNode = xmlDoc.CreateElement("phonemes");

        float partAudioDuration = 0;
        float totalAudioDuration = 0;

        // should be added to Application.streamingAssetsPath when build
        using (StreamReader reader = new StreamReader(output_phonemes_path))
        {
            string line;
            int audioParts = 0;
            float audioDuration = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("wav"))
                {
                    float prevDuration = audioDuration;
                    string[] line_contents = line.Split(null);
                    int audioSampleDuration = int.Parse(line_contents[4]);
                    audioDuration = AudioInfo.getExactDuration(audioSampleDuration);
                    audioParts += 1;
                    partAudioDuration += prevDuration;
                    totalAudioDuration += audioDuration;
                }

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

                    XmlNode markerNode = xmlDoc.CreateElement("marker");
                    XmlAttribute time = xmlDoc.CreateAttribute("time");
                    time.Value = startOffset+"";
                    //time.Value = (startOffset + endOffset)/2 + "";
                    markerNode.Attributes.Append(time);
                    XmlAttribute intensity = xmlDoc.CreateAttribute("intensity");
                    intensity.Value = "1";
                    markerNode.Attributes.Append(intensity);
                    XmlAttribute sustain = xmlDoc.CreateAttribute("sustain");
                    sustain.Value = "False";
                    markerNode.Attributes.Append(sustain);
                    XmlAttribute phoneme = xmlDoc.CreateAttribute("phoneme");
                    phoneme.Value = RogoPhonemeInfo.MapRogoPhoneme(phoneme_info[3]).ToString();
                    markerNode.Attributes.Append(phoneme);

                    phonemeNode.AppendChild(markerNode);
                }

            }

            XmlNode lengthNode = xmlDoc.CreateElement("length");
            lengthNode.AppendChild(xmlDoc.CreateTextNode(totalAudioDuration + ""));
            mainNode.AppendChild(lengthNode);
            
            mainNode.AppendChild(phonemeNode);

            XmlNode emotionNode = xmlDoc.CreateElement("emotions");
            mainNode.AppendChild(emotionNode);
            XmlNode gesturesNode = xmlDoc.CreateElement("gestures");
            mainNode.AppendChild(gesturesNode);

            xmlDoc.Save(PathManager.GetDataPath("audio.xml"));
        }

        // copy audio file to rogodigital path
        if (File.Exists(PathManager.GetRogoDigitalPath("audio.wav")))
        {
            File.Delete(PathManager.GetRogoDigitalPath("audio.wav"));
        }
#if UNITY_EDITOR
        FileUtil.CopyFileOrDirectory(PathManager.GetAudioPath("audio.wav"), PathManager.GetRogoDigitalPath("audio.wav"));
#endif
    }

}
