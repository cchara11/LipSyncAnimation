using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

///-----------------------------------------------------------------
///   Class:        TTS.cs
///   Description:  This class uses CereVoice C# wrapper to generate
///                 audio from a supporting SSML input file 
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Text to speech using CereVoice API
///-----------------------------------------------------------------

public class TTS : MonoBehaviour
{
    public static string output_audio_path;
    public static string output_phonemes_path;
    public InputField inputText;
    public string inputFileName = "textToSpeak.xml";
    SSMLGenerator ssml;

    /// <summary>
    /// generates input file in SSML format according to the input text
    /// </summary>
    void GenerateInputFile()
    {
        string inputFileUrl = PathManager.getDataPath(inputFileName);

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
            Console.WriteLine("ERROR: Please enter text");
        }

    }

    /// <summary>
    /// generates audio using CereVoice C# wrapper, and generates phonemic information
    /// </summary>
    /// <param name="args"></param>
    public void GenerateAudio()
    {
        // generate input file first
        GenerateInputFile();

        output_audio_path = PathManager.getAudioPath("audio.wav");
        if (File.Exists(output_audio_path))
        {
            File.Delete(output_audio_path);
        }

        output_phonemes_path = PathManager.getDataPath("phonemes.txt");
        if (File.Exists(output_phonemes_path))
        {
            File.Delete(output_phonemes_path);
        }

        int chan, ret;
        CPRC_VOICE_LOAD_TYPE load_mode;
        string license_file = PathManager.getCereVoicePath("license.lic");
        string voice_file = PathManager.getCereVoicePath("cerevoice_heather_3.2.0_48k.voice");

        // create the TTS engine
        SWIGTYPE_p_CPRCEN_engine eng = cerevoice_eng.CPRCEN_engine_new();

        // load voice
        load_mode = CPRC_VOICE_LOAD_TYPE.CPRC_VOICE_LOAD_EMB_AUDIO;
        ret = cerevoice_eng.CPRCEN_engine_load_voice(eng, license_file, "", voice_file, load_mode);
        if (ret == 0)
        {
            Console.WriteLine("ERROR: could not load voice " + voice_file + " with license file " + license_file);
            Environment.Exit(0);
        }

        // load user custom lexicon
        cerevoice_eng.CPRCEN_engine_load_user_lexicon(eng, 0, PathManager.getCereVoicePath("lexicon.lex"));

        // open channel
        chan = cerevoice_eng.CPRCEN_engine_open_channel(eng, "", "", "", "");
        if (chan == 0)
        {
            Console.WriteLine("ERROR: failed to open channel, exiting");
            Environment.Exit(0);
        }

        // voice and channel information
        Console.WriteLine("INFO: using voice " + cerevoice_eng.CPRCEN_channel_get_voice_info(eng, chan, "VOICE_NAME") +
            " with sampling rate " +
            cerevoice_eng.CPRCEN_channel_get_voice_info(eng, chan, "SAMPLE_RATE"));

        cerevoice_eng.SetChannelCallback(eng, chan, CallbackHandler);

        // synthesis
        foreach (string l in File.ReadAllLines(PathManager.getDataPath(inputFileName)))
        {
            cerevoice_eng.CPRCEN_engine_channel_speak(eng, chan, l, l.Length, 0);
        }
        cerevoice_eng.CPRCEN_engine_channel_speak(eng, chan, "", 0, 1);

        // clean
        cerevoice_eng.CPRCEN_engine_delete(eng);

    }

    static void CallbackHandler(IntPtr abufp, IntPtr userdatap)
    {
        try
        {
            // write phoneme information output to file
            using (StreamWriter sw = new StreamWriter(output_phonemes_path, true))
            {
                SWIGTYPE_p_CPRC_abuf_trans trans;
                string name;
                float start, end;

                // convert abuf pointer into a C# audio buffer
                SWIGTYPE_p_CPRC_abuf abuf = new SWIGTYPE_p_CPRC_abuf(abufp, false);

                // audio duration for each part
                sw.WriteLine(String.Format("INFO: wav_mk {0}, wav_done {1}", cerevoice_eng.CPRC_abuf_wav_mk(abuf), cerevoice_eng.CPRC_abuf_wav_done(abuf)));

                foreach (int i in Enumerable.Range(0, cerevoice_eng.CPRC_abuf_trans_sz(abuf)))
                {
                    trans = cerevoice_eng.CPRC_abuf_get_trans(abuf, i);
                    name = cerevoice_eng.CPRC_abuf_trans_name(trans);
                    start = (float)Math.Round(cerevoice_eng.CPRC_abuf_trans_start(trans), 3);
                    end = (float)Math.Round(cerevoice_eng.CPRC_abuf_trans_end(trans), 3);

                    if (cerevoice_eng.CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_TYPE.CPRC_ABUF_TRANS_PHONE)
                    {
                        sw.WriteLine(String.Format("INFO: phoneme: {0} {1} {2}", start, end, name));
                    }
                    else if (cerevoice_eng.CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_TYPE.CPRC_ABUF_TRANS_WORD)
                    {
                        sw.WriteLine(String.Format("INFO: word: {0} {1} {2}", start, end, name));
                    }
                    else if (cerevoice_eng.CPRC_abuf_trans_type(trans) == CPRC_ABUF_TRANS_TYPE.CPRC_ABUF_TRANS_MARK)
                    {
                        sw.WriteLine(String.Format("INFO: marker: {0} {1} {2}", start, end, name));
                    }
                    else
                    {
                        sw.WriteLine(String.Format("WARNING: transcription type: '{0}' not known", cerevoice_eng.CPRC_abuf_trans_type(trans)));
                    }
                }

                // Append generated audio to output file
                cerevoice_eng.CPRC_riff_append(abuf, output_audio_path);
            }
            
        }
        catch (Exception e)
        {
            print(e);
        }

        
        
    }
}
