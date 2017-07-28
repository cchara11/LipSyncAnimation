using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProsodyComponent
{
    public float timestamp;
    public float voicingProbability;
    public float F0; // fundamental frequency
    public float loudness;

    public ProsodyComponent(float timestamp, float voicingProbability, float F0, float loudness)
    {
        this.timestamp = timestamp;
        this.voicingProbability = voicingProbability;
        this.F0 = F0;
        this.loudness = loudness;
    }

}

