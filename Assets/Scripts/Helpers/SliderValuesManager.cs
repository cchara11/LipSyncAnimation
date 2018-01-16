using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///--------------------------------------------------------------------------
///   Class:        SliderValuesManager.cs
///   Description:  Changes slider values for phonemic influence dynamically 
///   Author:       Constantinos Charalambous     Date: 28/11/2017
///   Notes:        Lip Sync Animation
///--------------------------------------------------------------------------

public class SliderValuesManager : MonoBehaviour
{
    Text vowelOverVowelText, vowelOverConsonantText, consonantOverConsonantText, consonantOverVowelText;
    MyLipSync lipsync;

    private void Start()
    {
        lipsync = (MyLipSync)GameObject.FindObjectOfType(typeof(MyLipSync));
        vowelOverVowelText = GameObject.Find("VowelOverVowelIndicator").GetComponent<Text>();
        vowelOverConsonantText = GameObject.Find("VowelOverConsonantIndicator").GetComponent<Text>();
        consonantOverConsonantText = GameObject.Find("ConsonantOverConsonantIndicator").GetComponent<Text>();
        consonantOverVowelText = GameObject.Find("ConsonantOverVowelIndicator").GetComponent<Text>();
    }
    
    public void SetVovSliderValue(float sliderValue)
    {
        vowelOverVowelText.text = sliderValue.ToString();
        lipsync.vowelOverVowel = sliderValue;
    }

    public void SetVocSliderValue(float sliderValue)
    {
        vowelOverConsonantText.text = sliderValue.ToString();
        lipsync.vowelOverConsonant = sliderValue;
    }

    public void SetCocSliderValue(float sliderValue)
    {
        consonantOverConsonantText.text = sliderValue.ToString();
        lipsync.consonantOverConsonant = sliderValue;
    }

    public void SetCovSliderValue(float sliderValue)
    {
        consonantOverVowelText.text = sliderValue.ToString();
        lipsync.consonantOverVowel = sliderValue;
    }
}
