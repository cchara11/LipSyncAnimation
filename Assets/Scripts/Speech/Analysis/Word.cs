using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word
{
    public string text;
    public float startingInterval;
    public float endingInterval;
    public List<float> intervals;
    public List<float> frequencies;

    public Word(string text, float startingInterval, float endingInterval)
    {
        this.text = text;
        this.startingInterval = startingInterval;
        this.endingInterval = endingInterval;
        intervals = new List<float>();
        frequencies = new List<float>();
    }

}
