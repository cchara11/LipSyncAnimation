using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CircumplexModel : MonoBehaviour {
    
    static List<BaseEmotion> Emotions;

    public Transform circumplexModel;
    public GameObject center;

    void Awake()
    {
        Emotions = fillEmotions();

        foreach (BaseEmotion be in Emotions)
        {
            GameObject emotion = (GameObject)Instantiate(Resources.Load("DimensionPlaceholder"));
            emotion.transform.name = be.name;
            emotion.transform.parent = circumplexModel;
            emotion.transform.localPosition = new Vector3(be.valence * (-0.5f), -1f, be.arousal * (-0.5f));
            TextMesh timerText = (TextMesh)emotion.GetComponentInChildren(typeof(TextMesh));
            timerText.text = be.name;
        }
    }

    private void Update()
    {
        GameObject[] emotions = GameObject.FindGameObjectsWithTag("Emotion");

        foreach (GameObject emotion in emotions)
        {
            Debug.DrawLine(emotion.transform.position, center.transform.position, Color.red);   
        }
    }

    private List<BaseEmotion> fillEmotions()
    {
        List <BaseEmotion> emotions = new List<BaseEmotion>();
        string emotionsData = PathManager.getEmotionPath("dimensions.txt");
        
        using (StreamReader reader = new StreamReader(emotionsData))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split('\t');
                emotions.Add(new BaseEmotion(data[0], float.Parse(data[2]), float.Parse(data[1])));
            }
        }

        return emotions;
    }

    public static List<BaseEmotion> GetEmotionDimensions()
    {
        return Emotions;
    }
}
