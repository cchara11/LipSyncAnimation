using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEmotion
{
    public string name;
    public float arousal;
    public float valence;

    public BaseEmotion(string name, float arousal, float valence)
    {
        this.name = name;
        this.arousal = arousal;
        this.valence = valence;
    }
    

}
