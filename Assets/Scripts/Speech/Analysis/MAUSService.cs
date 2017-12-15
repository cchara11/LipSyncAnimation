using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

///---------------------------------------------------------------------
///   Class:        MAUSService.cs
///   Description:  Base class of MAUS service 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Audio analysis
///---------------------------------------------------------------------

public class MAUSService
{
    string resultFileName;

    /// <summary>
    /// Configure arguments and call MAUS script from command line
    /// </summary>
    /// <param name="audioName"></param>
    /// <param name="language"></param>
    public void AnalyzeAudio(string audioName, Language language)
    {
        resultFileName = PathManager.GetResourcesPath("response.xml");
        string lang = "";
        if (language.Equals(Language.EnglishAU))
        {
            lang = "eng-AU";
        }
        else if (language.Equals(Language.EnglishGB))
        {
            lang = "eng-GB";
        }
        else if (language.Equals(Language.EnglishUS))
        {
            lang = "eng-US";
        }
        RunBatchProcess(audioName, lang);
    }

    /// <summary>
    /// Process to run MAUS web service
    /// Inputs given: transcript together with corresponding audio
    /// Outputs given: donwload link of word and phonemic transcription file 
    /// </summary>
    public void RunBatchProcess(string audioName, string language)
    {
        // start the child process
        Process process = new Process();

        // redirect the output stream of the child process
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = PathManager.GetResourcesPath();
        process.StartInfo.FileName = PathManager.GetResourcesPath("MAUSRequest.bat");
        process.StartInfo.Arguments = PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + audioName + ".txt") + " " + PathManager.GetAudioResourcesPath(SceneManager.GetActiveScene().name + "/" + audioName + ".wav") + " " + language;

        int exitCode = -1;
        string output = null;

        if (File.Exists(resultFileName))
        {
            File.Delete(resultFileName);
        }

        try
        {
            process.Start();

            // read the output stream first and then wait.
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Run error" + e.ToString() + " output is " + output); // or throw new Exception
        }
        finally
        {
            exitCode = process.ExitCode;
            process.Dispose();
            process = null;
        }
    }
    
}
