using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        PathManager.cs
///   Description:  This class serves as a path provider helper 
///                 for several files included in the Assets 
///                 folder of the project
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Generic Path Provider
///-----------------------------------------------------------------

public static class PathManager
{ 
    public static string getDataPath(string file)
    {
        return Application.streamingAssetsPath + "/" + file;
    }

    public static string getXMLDataPath(string file)
    {
        return Application.streamingAssetsPath + "/XML/" + file;
    }

    public static string getAudioPath(string file)
    {
        return Application.streamingAssetsPath + "/Audio/" + file;
    }

    public static string getSEMAINPath(string file)
    {
        return Application.streamingAssetsPath + "/SEMAINE/Sessions/2/" + file;
    }

    public static string getCereVoicePath(string file)
    {
        return Application.streamingAssetsPath + "/CereVoice/" + file;
    }

    public static string getOpenSmilePath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/" + file;
    }

    public static string getOpenSmileConfigPath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/config/" + file;
    }

    public static string getOpenSmileDataPath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/data/" + file;
    }

    public static string getRogoDigitalPath(string file)
    {
        return Application.dataPath + "/Rogo Digital/LipSync Pro/Examples/Audio/" + file;
    }

    public static string getEmotionPath(string file)
    {
        return Application.streamingAssetsPath + "/Emotions/" + file;
    }
}
