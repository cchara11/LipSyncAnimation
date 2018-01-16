using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        PhonemeBlendShape.cs
///   Description:  The class used for mapping phonemes to the
///                 desired blendshapes
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Phonemes to blendshapes mapping
///-----------------------------------------------------------------

[System.Serializable]
public class PhonemeBlendShape
{
    public Phoneme phoneme;
    public List<BlendShape> blendShapes;

    public PhonemeBlendShape()
    {
        blendShapes = new List<BlendShape>();
    }

}

