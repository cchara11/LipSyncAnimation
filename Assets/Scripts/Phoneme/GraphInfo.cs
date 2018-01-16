using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        GraphInfo.cs
///   Description:  Class used for curve information
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///-----------------------------------------------------------------

public class GraphInfo
{ 
    public Phoneme phoneme { get; private set; }
    public float weight { get; set; }

    public GraphInfo(Phoneme phoneme, float weight)
    {
        this.phoneme = phoneme;
        this.weight = weight;
    }
}
