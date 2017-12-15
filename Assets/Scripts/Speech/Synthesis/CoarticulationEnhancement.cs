using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        CoarticulationEnhancement.cs
///   Description:  This class uses several methods to enhance 
///                 coarticulation during lip sync animation. The 
///                 remaining functions will be implemented within 
///                 the following week
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Introduce and enhance coarticulation
///-----------------------------------------------------------------

public class CoarticulationEnhancement
{
    // TODO 
    // check for pauses
    // check for lexically stressed words - Schwa
    // lip-heavy visemes
    // neighboring visemes influence each other (tongue-only visemes)
    
    /// <summary>
    /// Converts diphones (dynamic visemes) into their corresponding phonemes 
    /// </summary>
    /// <param name="phonemes">Phoneme List</param>
    /// <param name="diphonePhonemes">Dictionary containing the corresponding mapping between diphones and phonemes</param>
    /// <returns>The new list with the diphone visemes included</returns>
    public List<PhonemeInformation> AddDiphoneVisemes(List<PhonemeInformation> phonemes, Dictionary<Phoneme, List<Phoneme>> diphonePhonemes)
    {
        List<PhonemeInformation> enhancedList = new List<PhonemeInformation>();
        float weight = 0; // check for dominance model

        for (int i = 0; i < phonemes.Count; i++)
        {
            if (!phonemes[i].text.ToString().Contains("Diphone"))
            {
                enhancedList.Add(phonemes[i]);
            }
            else
            {
                // phoneme segment duration
                float duration = phonemes[i].endingInterval - phonemes[i].startingInterval;
                
                // check next phoneme segment
                if (i != phonemes.Count)
                {
                    string phoneme = phonemes[i].text.ToString();
                    if (Enum.IsDefined(typeof(Consonant), phoneme))
                    {
                        weight = 0.75f;
                    }
                    else
                    {
                        weight = 0.50f;
                    }
                    
                    List<Phoneme> diphones = diphonePhonemes[(Phoneme)phonemes[i].text];
                    float firstDiphoneDuration = duration * weight;
                    enhancedList.Add(new PhonemeInformation(phonemes[i].startingInterval, phonemes[i].startingInterval + firstDiphoneDuration, diphones[0]));
                    enhancedList.Add(new PhonemeInformation(phonemes[i].startingInterval + firstDiphoneDuration, phonemes[i].endingInterval, diphones[1]));
                }
                
            }
        }

        //foreach (PhonemeInfo pi in enhancedList)
        //{
        //    Debug.Log(pi.phoneme);
        //}

        return enhancedList;
    }

    /// <summary>
    /// merges duplicated continuous phoneme in to a big one for cohesion
    /// </summary>
    /// <param name="phonemes">Phoneme List</param>
    /// <returns>the new list without the duplicated phonemes</returns>
    public List<PhonemeInformation> RemoveDuplicates(List<PhonemeInformation> phonemes)
    {
        List<PhonemeInformation> distinctPhonemes = new List<PhonemeInformation>();

        float threshold = 1f;

        foreach (PhonemeInformation p in phonemes)
        {
            if (distinctPhonemes.Count == 0 || !distinctPhonemes[distinctPhonemes.Count - 1].text.Equals(p.text))
            {
                distinctPhonemes.Add(p);
            }
            else
            {
                // check time interval between duplicated phonemes
                if ((p.startingInterval - distinctPhonemes[distinctPhonemes.Count - 1].endingInterval) > threshold)
                {
                    distinctPhonemes.Add(p);
                }
                else
                {
                    // replace duplicated phoneme bu adjusting its ending time
                    distinctPhonemes[distinctPhonemes.Count - 1].endingInterval = p.endingInterval;
                }
                
            }
        }

        return distinctPhonemes;
    }

    public Phoneme GetPhonemeText(float duration, string text)
    {
        float vowelThreshold = 0.03f;
        float consonantThreshold = 0.03f;
        Phoneme? textToPhoneme = PhonemeInformation.MapPhoneme(text);

        if (Enum.IsDefined(typeof(Vowel), textToPhoneme.ToString()))
        {
            if (duration <= vowelThreshold)
            {
                // if duration of vowel is lower than threshold reduce vowel to Schwa
                return Phoneme.Schwa;
            }
            return (Phoneme)textToPhoneme;
        }
        else
        {
            if (duration <= consonantThreshold)
            {
                // if duration of consonant is lower than threshold drop phonemes h and t
                if (text.Equals("h") || text.Equals("h\\") || text.Equals("t") || text.Equals("T"))
                {
                    return Phoneme.Rest;
                }
                else
                {
                    return (Phoneme)textToPhoneme;
                }
            }
            return (Phoneme)textToPhoneme;
        }
    }

}
