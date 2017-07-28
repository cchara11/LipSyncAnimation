using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        EmotionBlendShape.cs
///   Description:  The class used for mapping emotions to the
///                 desired blendshapes
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Emotions to blendshapes mapping
///-----------------------------------------------------------------

[System.Serializable]
public class EmotionBlendShape
{
    public Emotion emotion; 
    public List<BlendShape> blendShapes;

    public EmotionBlendShape()
    {
        blendShapes = new List<BlendShape>();
    }

}

