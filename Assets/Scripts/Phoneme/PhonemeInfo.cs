using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        PhonemeInfo.cs
///   Description:  This class contains all the information for each
///                 phoneme. It includes intervals of starting and
///                 ending phonemes, as well as the phoneme name
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Phoneme Information
///-----------------------------------------------------------------

public class PhonemeInfo
{
    public float starting_time { get; set; }
    public float ending_time { get; set; }
    public Phoneme? phoneme { get; set; }
    public bool apex { get; set; } 
    public float influence { get; set; }

    public PhonemeInfo(float starting_time, float ending_time, string phoneme)
    {
        this.starting_time = starting_time;
        this.ending_time = ending_time;
        this.phoneme = MapPhoneme(phoneme);
        apex = false;
        influence = GetPhonemicInfluence();
    }

    public PhonemeInfo(float starting_time, float ending_time, Phoneme phoneme)
    {
        this.starting_time = starting_time;
        this.ending_time = ending_time;
        this.phoneme = phoneme;
        apex = false;
        influence = GetPhonemicInfluence();
    }

    public float GetPhonemicInfluence()
    {
        // calculate influence
        if (Enum.IsDefined(typeof(LipHeavy), phoneme.ToString()))
        {
            // stronger influence on lip-heavy visemes
            return 1;
        }
        else if (Enum.IsDefined(typeof(LipLight), phoneme.ToString()))
        {
            // lower influence on lip-light visemes (nasals, tongue-only, obsturents)
            return 0.3f;
        }
        else if (phoneme.Equals(Phoneme.Schwa))
        {
            // strong influence on lexically stressed vowels
            return 1f;
        }
        else if (Enum.IsDefined(typeof(Vowel), phoneme.ToString()))
        {
            // middle influence on rest
            return 0.8f;
        }
        else if (Enum.IsDefined(typeof(Consonant), phoneme.ToString()))
        {
            // middle influence on rest
            return 0.1f;
        }
        else
        {
            return 0;
        }
    }
    /// <summary>
    /// Maps CereVoice phonemes to the defined phonemes of the lip sync component
    /// </summary>
    /// <param name="phoneme">The phoneme to be mapped</param>
    /// <returns></returns>
    Phoneme? MapPhoneme(string phoneme)
    {
        switch (phoneme)
        {
            case "@":
                return Phoneme.Schwa;
            case "@@":
                return Phoneme.Schwa;
            case "a":
                return Phoneme.AHH;
            case "aa":
                return Phoneme.AHH;
            case "ai":
                return Phoneme.DiphoneAI;
            case "au":
                return Phoneme.DiphoneAU;
            case "b":
                return Phoneme.MMM;
            case "ch":
                return Phoneme.SSH;
            case "d":
                return Phoneme.T;
            case "dh":
                return Phoneme.TH;
            case "e":
                return Phoneme.EHH;
            case "ei":
                return Phoneme.DiphoneEI;
            case "f":
                return Phoneme.FFF;
            case "g":
                return Phoneme.GK; // check again
            case "h":
                return Phoneme.GK; // was EHH previously
            case "i":
                return Phoneme.IEE;
            case "ii":
                return Phoneme.IEE;
            case "jh":
                return Phoneme.SSH;
            case "k":
                return Phoneme.GK; // check again
            case "l":
                return Phoneme.L; 
            case "m":
                return Phoneme.MMM;
            case "n":
                return Phoneme.N;
            case "ng":
                return Phoneme.GK;
            case "o":
                return Phoneme.OHH;
            case "oi":
                return Phoneme.DiphoneOI;
            case "oo":
                return Phoneme.OHH;
            case "ou":
                return Phoneme.DiphoneOU;
            case "p":
                return Phoneme.MMM;
            case "r":
                return Phoneme.RRR;
            case "s":
                return Phoneme.SSS;
            case "sh":
                return Phoneme.SSH;
            case "t":
                return Phoneme.T;
            case "th":
                return Phoneme.TH; 
            case "u":
                return Phoneme.UUU;
            case "uh":
                return Phoneme.UUU;
            case "uu":
                return Phoneme.UUU;
            case "v":
                return Phoneme.FFF;
            case "w":
                return Phoneme.UUU;
            case "x":
                return Phoneme.DiphoneX;
            case "y":
                return Phoneme.IEE;
            case "z":
                return Phoneme.SSS;
            case "zh":
                return Phoneme.SSH;
            default:
                return Phoneme.Rest;
        }


    }

}
