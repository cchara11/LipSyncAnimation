using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

///-----------------------------------------------------------------
///   Class:        PhonemeSet.cs
///   Description:  This enum includes the action units that are
///                 triggered when animating the corresponding 
///                 phoneme/viseme. This configuration may replace
///                 the current viseme configuration in the future
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Phoneme Action Units
///-----------------------------------------------------------------

public enum PhonemeSet
{
    [DescriptionWithValue("AE EY", "10C 12C 14C 16D 17A 20B 22uB 27C")]
    AAA,
    [DescriptionWithValue("AA AO AY AW", "16D 17A 27D")]
    AHH,
    [DescriptionWithValue("UW w", "17A 18D 28A 27A")]
    UUU,
    [DescriptionWithValue("r", "17A 18C 22C 27A")]
    RRR,
    [DescriptionWithValue("D T", "10C 17A 22uD 27B")]
    TTH,
    [DescriptionWithValue("f v", "17D 23uA 27A 28B")]
    FFF,
    [DescriptionWithValue("UX UH EH h", "10A 12B 16B 17A 20B 22B 27B")]
    EHH,
    [DescriptionWithValue("OW OY", "17A 18D 22A 27B")]
    OHH,
    [DescriptionWithValue("IY IH y", "10C 12C 16D 17A 20B 22B 27A")]
    IEE,
    [DescriptionWithValue("s z", "16E 17A 22C")]
    SSS,
    [DescriptionWithValue("J C S Z", "10B 16B 18C 20A 22C 27A")]
    SSH,
    [DescriptionWithValue("m b p", "17E 23uC 27A")]
    MMM,
    [DescriptionWithValue("AX IX", "27B")]
    Schwa
}

// code to get description and value from enums
/*
var temp = PhonemeSet.AAA.GetType().GetField(PhonemeSet.AAA.ToString());
var attribute = (DescriptionWithValueAttribute[])temp.GetCustomAttributes(typeof(DescriptionWithValueAttribute), false);
var description = (attribute.Length > 0 ? attribute[0].Description : PhonemeSet.AAA.ToString());
var value = (attribute.Length > 0 ? attribute[0].Value : PhonemeSet.AAA.ToString());
*/
