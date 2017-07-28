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
    // if two consecutive phonemes are the same, merge them into a long one
    // check for lexically stressed words
    // lip-heavy visemes
    // neighboring visemes influence each other (tongue-only visemes)
    
    /// <summary>
    /// Converts diphones (dynamic visemes) into their corresponding phonemes 
    /// </summary>
    /// <param name="phonemes">Phoneme List</param>
    /// <param name="diphonePhonemes">Dictionary containing the corresponding mapping between diphones and phonemes</param>
    /// <returns></returns>
    public static List<PhonemeInfo> AddDynamicVisemes(List<PhonemeInfo> phonemes, Dictionary<Phoneme, List<Phoneme>> diphonePhonemes)
    {
        List<PhonemeInfo> enhancedList = new List<PhonemeInfo>();
        float weight = 0; // check for dominance model

        for (int i = 0; i < phonemes.Count; i++)
        {
            if (!phonemes[i].phoneme.ToString().Contains("Diphone"))
            {
                enhancedList.Add(phonemes[i]);
            }
            else
            {
                // phoneme segment duration
                float duration = phonemes[i].ending_time - phonemes[i].starting_time;
                
                // check next phoneme segment
                if (i+1 != phonemes.Count)
                {
                    string phoneme = phonemes[i + 1].phoneme.ToString();
                    if (Enum.IsDefined(typeof(Consonant), phoneme))
                    {
                        weight = 0.75f;
                    }
                    else
                    {
                        weight = 0.50f;
                    }
                    List<Phoneme> diphones = diphonePhonemes[(Phoneme)phonemes[i].phoneme];
                    float firstDiphoneDuration = duration * weight;
                    enhancedList.Add(new PhonemeInfo(phonemes[i].starting_time, phonemes[i].starting_time + firstDiphoneDuration, diphones[0]));
                    enhancedList.Add(new PhonemeInfo(phonemes[i].starting_time + firstDiphoneDuration, phonemes[i].ending_time, diphones[1]));
                }
                
            }
        }

        return enhancedList;
    }
    

}
