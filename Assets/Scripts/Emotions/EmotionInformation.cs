using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionInformation
{
    public Emotion emotion;
    public float startingInterval;
    public float endingInterval;
    public bool apex;

    public EmotionInformation(float startingInterval, float endingInterval, Emotion emotion)
    {
        this.startingInterval = startingInterval;
        this.endingInterval = endingInterval;
        this.emotion = emotion;
        apex = false;
    }
}
