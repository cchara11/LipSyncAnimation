using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        PhonemeInformation.cs
///   Description:  This class contains all the information for each
///                 phoneme. It includes intervals of starting and
///                 ending phonemes, as well as the phoneme name
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Phoneme Information
///-----------------------------------------------------------------

public class PhonemeInformation
{
    public float startingInterval { get; set; }
    public float endingInterval { get; set; }
    public Phoneme? text { get; set; }
    public bool apex { get; set; }
    public bool animationEnded { get; set; }
    public float influence { get; set; }
    public float backwardInfluence { get; set; }
    public float forwardInfluence { get; set; }
    public float meanPitch { get; set; }
    public float meanIntensity { get; set; }
    public float onset_offset { get; set; }
    public Influence phonemicInfluenceBack { get; set; }
    public Influence phonemicInfluenceForw { get; set; }



    public PhonemeInformation(float starting_time, float ending_time, string phoneme)
    {
        this.startingInterval = (float)Math.Round(starting_time, 2); 
        this.endingInterval = (float)Math.Round(ending_time, 2);
        this.text = MapPhoneme(phoneme);
        apex = false;
        animationEnded = false;
        influence = GetPhonemicInfluence(text);
        meanPitch = 0;
        meanIntensity = 0;
        onset_offset = 0;
    }

    public PhonemeInformation(float starting_time, float ending_time, Phoneme phoneme)
    {
        this.startingInterval = (float)Math.Round(starting_time, 2);
        this.endingInterval = (float)Math.Round(ending_time, 2);
        this.text = phoneme;
        apex = false;
        animationEnded = false;
        influence = GetPhonemicInfluence(text);
        meanPitch = 0;
        meanIntensity = 0;
        onset_offset = 0;
    }

    /// <summary>
    /// Apply individual influence weights on different phoneme categories
    /// </summary>
    /// <param name="phoneme"></param>
    /// <returns></returns>
    public float GetPhonemicInfluence(Phoneme? phoneme)
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
            return 0.5f;
        }
        else if (phoneme.Equals(Phoneme.Schwa))
        {
            // strong influence on lexically stressed vowels
            return 1f;
        }
        else if (Enum.IsDefined(typeof(Vowel), phoneme.ToString()))
        {
            // based on sonority the hierarchical degree of influence, return different weights for each vowel
            switch(phoneme)
            {
                case Phoneme.UUU:
                    return 0.9f;
                case Phoneme.OHH:
                    return 0.85f;
                case Phoneme.IEE:
                    return 0.8f;
                case Phoneme.EHH:
                    return 0.75f;
                case Phoneme.AAA:
                    return 0.7f;
                case Phoneme.AHH:
                    return 0.7f;
                default:
                    return 0.7f;
            }
        }
        else if (Enum.IsDefined(typeof(Consonant), phoneme.ToString()))
        {
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
    public static Phoneme? MapPhoneme(string phoneme)
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
                return Phoneme.GK; 
            case "h":
                return Phoneme.GK; 
            case "i":
                return Phoneme.IEE;
            case "ii":
                return Phoneme.IEE;
            case "jh":
                return Phoneme.SSH;
            case "k":
                return Phoneme.GK; 
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
            // X-SAMPA additional annotation for MAUS
            // consonants
            case "D":
                return Phoneme.TH;
            case "dZ":
                return Phoneme.SSH;
            case "h\\":
                return Phoneme.GK;
            case "j":
                return Phoneme.UUU; 
            case "l=":
                return Phoneme.L;
            case "m=":
                return Phoneme.MMM;
            case "n=":
                return Phoneme.N;
            case "N":
                return Phoneme.GK;
            case "S":
                return Phoneme.SSH;
            case "r\\":
                return Phoneme.RRR;
            case "R":
                return Phoneme.RRR;
            case "tS":
                return Phoneme.SSH;
            case "T":
                return Phoneme.T;
            case "Z":
                return Phoneme.SSH;
            //vowels
            case "@U":
                return Phoneme.DiphoneOU;
            case "{":
                return Phoneme.AAA;
            case "aI":
                return Phoneme.DiphoneAI;
            case "aU":
                return Phoneme.DiphoneAU;
            case "A:":
                return Phoneme.AAA;
            case "eI":
                return Phoneme.DiphoneEI;
            case "e@":
                return Phoneme.DiphoneEA;
            case "3:":
                return Phoneme.EHH;
            case "6":
                return Phoneme.Schwa;
            case "E":
                return Phoneme.EHH;
            case "3`":
                return Phoneme.EHH;
            case "i:":
                return Phoneme.IEE;
            case "I":
                return Phoneme.IEE; 
            case "I@":
                return Phoneme.DiphoneIA;
            case "O:":
                return Phoneme.OHH;
            case "OI":
                return Phoneme.DiphoneOI;
            case "Q":
                return Phoneme.AAA;
            case "u:":
                return Phoneme.UUU;
            case "U":
                return Phoneme.UUU;
            case "U@":
                return Phoneme.DiphoneUA;
            case "V":
                return Phoneme.Schwa;
            default:
                return Phoneme.Rest;
        }


    }

}
