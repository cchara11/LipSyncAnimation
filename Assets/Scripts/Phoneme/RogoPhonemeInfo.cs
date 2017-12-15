using System.Collections;
using System.Collections.Generic;

///--------------------------------------------------------------------
///   Class:        RogoPhonemeInfo.cs
///   Description:  Class responsible to map our phonemes to Rogo ones 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///--------------------------------------------------------------------

public static class RogoPhonemeInfo
{
    /// <summary>
    /// Returns the respective rogo phoneme
    /// </summary>
    /// <param name="phoneme"></param>
    /// <returns></returns>
    public static RogoPhoneme? MapRogoPhoneme(string phoneme)
    {
        switch (phoneme)
        {
            case "AHH":
                return RogoPhoneme.AI;
            case "AAA":
                return RogoPhoneme.AI;
            case "UUU":
                return RogoPhoneme.U;
            case "RRR":
                return RogoPhoneme.L;
            case "T":
                return RogoPhoneme.CDGKNRSThYZ;
            case "TH":
                return RogoPhoneme.CDGKNRSThYZ;
            case "FFF":
                return RogoPhoneme.FV;
            case "EHH":
                return RogoPhoneme.E;
            case "OHH":
                return RogoPhoneme.O;
            case "IEE":
                return RogoPhoneme.CDGKNRSThYZ;
            case "SSS":
                return RogoPhoneme.CDGKNRSThYZ;
            case "SSH":
                return RogoPhoneme.CDGKNRSThYZ;
            case "MMM":
                return RogoPhoneme.MBP;
            case "Schwa":
                return RogoPhoneme.E;
            case "L":
                return RogoPhoneme.L;
            case "N":
                return RogoPhoneme.CDGKNRSThYZ;
            case "GK":
                return RogoPhoneme.CDGKNRSThYZ;
            default:
                return RogoPhoneme.Rest;
        }
    }
}
