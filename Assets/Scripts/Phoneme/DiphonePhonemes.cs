using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        DiphonePhonemes.cs
///   Description:  This class is used to map diphones into phonemes
///   Author:       Constantinos Charalambous     Date: 20/10/2017
///   Notes:        One diphone is mapped to just two phonemes
///-----------------------------------------------------------------
///
[System.Serializable]
public class DiphonePhonemes
{
    public Diphone diphone;
    public List<Phoneme> phonemes;

    public DiphonePhonemes()
    {
        phonemes = new List<Phoneme>();
    }

}
