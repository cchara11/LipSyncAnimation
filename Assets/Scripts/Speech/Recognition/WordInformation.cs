using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordInformation
{
    public string text;
    public float startingInterval;
    public float endingInterval;
    public List<float> intervals;
    public List<float> frequencies;
    public string pronunciation;
    public List<PhonemeInformation> phonemes;

    public WordInformation(float startingInterval, float endingInterval, string text)
    {
        this.text = text;
        this.startingInterval = startingInterval;
        this.endingInterval = endingInterval;
        intervals = new List<float>();
        frequencies = new List<float>();
        phonemes = new List<PhonemeInformation>();
    }

    public void setPronunciation(string pronunciation)
    {
        this.pronunciation = pronunciation;
    }

    public void AddPhoneme(PhonemeInformation pi)
    {
        phonemes.Add(pi);
    }

    public List<Phoneme?> GetPhonemes()
    {
        List<Phoneme?> phonemesIncluded = new List<Phoneme?>();
        foreach(PhonemeInformation pi in phonemes)
        {
            phonemesIncluded.Add(pi.text);
        }
        return phonemesIncluded;
    }

    public PhonemeInformation GetPhonemeInformation(Phoneme? phone)
    {
        foreach (PhonemeInformation pi in phonemes)
        {
            if(pi.text.Equals(phone))
            {
                return (pi);
            }
        }
        return null;
    }

}
