using System.Collections;
using System.Collections.Generic;

public static class RogoPhonemeInfo
{
    public static RogoPhoneme? MapRogoPhoneme(string phoneme)
    {
        switch (phoneme)
        {
            case "@":
                return RogoPhoneme.E;
            case "@@":
                return RogoPhoneme.E;
            case "a":
                return RogoPhoneme.AI;
            case "aa":
                return RogoPhoneme.AI;
            case "ai":
                return RogoPhoneme.AI;
            case "au":
                return RogoPhoneme.AI;
            case "b":
                return RogoPhoneme.MBP;
            case "ch":
                return RogoPhoneme.CDGKNRSThYZ;
            case "d":
                return RogoPhoneme.CDGKNRSThYZ;
            case "dh":
                return RogoPhoneme.CDGKNRSThYZ;
            case "e":
                return RogoPhoneme.E;
            case "ei":
                return RogoPhoneme.E;
            case "f":
                return RogoPhoneme.FV;
            case "g":
                return RogoPhoneme.CDGKNRSThYZ;
            case "h":
                return RogoPhoneme.CDGKNRSThYZ;
            case "i":
                return RogoPhoneme.CDGKNRSThYZ;
            case "ii":
                return RogoPhoneme.CDGKNRSThYZ;
            case "jh":
                return RogoPhoneme.CDGKNRSThYZ;
            case "k":
                return RogoPhoneme.CDGKNRSThYZ;
            case "l":
                return RogoPhoneme.L;
            case "m":
                return RogoPhoneme.MBP;
            case "n":
                return RogoPhoneme.CDGKNRSThYZ;
            case "ng":
                return RogoPhoneme.CDGKNRSThYZ;
            case "o":
                return RogoPhoneme.O;
            case "oi":
                return RogoPhoneme.O;
            case "oo":
                return RogoPhoneme.O;
            case "ou":
                return RogoPhoneme.U;
            case "p":
                return RogoPhoneme.MBP;
            case "r":
                return RogoPhoneme.CDGKNRSThYZ;
            case "s":
                return RogoPhoneme.CDGKNRSThYZ;
            case "sh":
                return RogoPhoneme.CDGKNRSThYZ;
            case "t":
                return RogoPhoneme.CDGKNRSThYZ;
            case "th":
                return RogoPhoneme.CDGKNRSThYZ;
            case "u":
                return RogoPhoneme.U;
            case "uh":
                return RogoPhoneme.U;
            case "uu":
                return RogoPhoneme.U;
            case "v":
                return RogoPhoneme.FV;
            case "w":
                return RogoPhoneme.WQ;
            case "x":
                return RogoPhoneme.CDGKNRSThYZ;
            case "y":
                return RogoPhoneme.CDGKNRSThYZ;
            case "z":
                return RogoPhoneme.CDGKNRSThYZ;
            case "zh":
                return RogoPhoneme.CDGKNRSThYZ;
            default:
                return RogoPhoneme.Rest;
        }
    }
}
