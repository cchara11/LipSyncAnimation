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
    public static string GetDataPath(string file)
    {
        return Application.streamingAssetsPath + "/" + file;
    }

    public static string GetXMLDataPath(string file)
    {
        return Application.streamingAssetsPath + "/XML/" + file;
    }

    public static string GetAudioPath(string file)
    {
        return Application.streamingAssetsPath + "/Audio/" + file;
    }

    public static string GetResourcesPath(string file)
    {
        return Application.dataPath + "/Resources/" + file;
    }

    public static string GetResourcesPath()
    {
        return Application.dataPath + "/Resources/";
    }

    public static string GetAudioResourcesPath(string file)
    {
        return Application.dataPath + "/Resources/Audio/" + file;
    }

    public static string GetSEMAINPath(string file)
    {
        return Application.streamingAssetsPath + "/SEMAINE/Sessions/2/" + file;
    }

    public static string GetCereVoicePath(string file)
    {
        return Application.streamingAssetsPath + "/CereVoice/" + file;
    }

    public static string GetOpenSmilePath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/" + file;
    }

    public static string GetOpenSmileConfigPath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/config/" + file;
    }

    public static string GetOpenSmileDataPath(string file)
    {
        return Application.streamingAssetsPath + "/OpenSmile/data/" + file;
    }

    public static string GetRogoDigitalPath(string file)
    {
        return Application.dataPath + "/Rogo Digital/LipSync Pro/Examples/Audio/" + file;
    }

    public static string GetResourcesRogoPath(string file)
    {
        return Application.dataPath + "/Resources/Rogo/" + file;
    }

    public static string GetEmotionPath(string file)
    {
        return Application.streamingAssetsPath + "/Emotions/" + file;
    }
}
