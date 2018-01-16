using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        Influence.cs
///   Description:  Phonemic influence indicator enumeration 
///   Author:       Constantinos Charalambous     Date: 20/10/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public enum Influence
{
    VC, // vowel over consonant
    VV, // vowel over vowel
    CV, // consonant over vowel
    CC, // consonant over consonant
    NA
}
