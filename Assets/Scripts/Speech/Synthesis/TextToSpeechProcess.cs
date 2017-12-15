using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

///-----------------------------------------------------------------
///   Class:        TextToSpeech.cs
///   Description:  This class uses CereVoice tts_callback.exe to
///                 synthesize audio from text
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Text to speech using tts_callback.exe
///-----------------------------------------------------------------

public class TextToSpeechProcess : MonoBehaviour {
    string output_file_path;
    public InputField inputText;
    public string inputFileName = "textToSpeak.xml";
    SSMLGenerator ssml;

    /// <summary>
    /// Configure arguments for ttscallback process
    /// </summary>
    List<string> ConfigureArguments()
    {
        List<string> cmd_arguments = new List<string>();

        // add command arguments for tts_callback proces
        cmd_arguments = new List<string>();
        cmd_arguments.Add("-o"); // optional argument to write audio to fill
        cmd_arguments.Add(PathManager.GetAudioPath("audio.wav")); // output audio path
        cmd_arguments.Add(PathManager.GetCereVoicePath("cerevoice_heather_3.2.0_48k.voice")); // voice path
        cmd_arguments.Add(PathManager.GetCereVoicePath("license.lic")); // license path
        cmd_arguments.Add(PathManager.GetDataPath(inputFileName)); // input text path

        output_file_path = PathManager.GetDataPath("phonemes.txt"); // output phonemes path

        return cmd_arguments;
    }

    /// <summary>
    /// Generates input file in SSML format according to the input text
    /// </summary>
    void GenerateInputFile()
    {
        string inputFileUrl = PathManager.GetDataPath(inputFileName);
        
        // check if an old version of the file exists
        if (File.Exists(inputFileUrl))
        {
            File.Delete(inputFileUrl);
        }

        // check if input field is not null
        if (!String.IsNullOrEmpty(inputText.text))
        {
            // generate new input file
            ssml = new SSMLGenerator();
            ssml.GenerateSSML(inputText.text);
        }
        else
        {
            print("ERROR: Please enter text");
        }
        
    }

    /// <summary>
    /// Generates audio using CereVoice process (ttscallback.exe), and generates phonemic information
    /// </summary>
    /// <param name="args"></param>
    public void GenerateAudio()
    {
        GenerateInputFile();
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
            var tts_callback = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = PathManager.GetCereVoicePath("tts_callback.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            // call tts_callback process
            tts_callback.Start();
            try
            {
                if (File.Exists(output_file_path))
                {
                    File.Delete(output_file_path);
                }

                // write phoneme information output to file
                using (FileStream file_stream = File.Create(output_file_path))
                {
                    while (!tts_callback.StandardOutput.EndOfStream)
                    {
                        string cmd_line = tts_callback.StandardOutput.ReadLine();
                        Byte[] line = new UTF8Encoding(true).GetBytes(cmd_line + "\n");
                        file_stream.Write(line, 0, line.Length);
                    }
                }
            }
            catch (Exception e)
            {
                print(e);
            }
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    
    
}
