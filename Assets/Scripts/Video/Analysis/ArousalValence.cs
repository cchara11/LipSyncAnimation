using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ArousalValence
{
    // Arousal
    public static Dictionary<float, float> GetArousal(string file)
    {
        Dictionary<float, float> arousal = new Dictionary<float, float>();

        using (StreamReader reader = new StreamReader(file))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] lineContents = line.Split();
                Dimension d = new Dimension(float.Parse(lineContents[0]), float.Parse(lineContents[1]));
                arousal.Add(d.timeStamp, d.level);
            }
        }

        return arousal;
    }

    // Arousal
    public static Dictionary<float, float> GetValence(string file)
    {
        Dictionary<float, float> valence = new Dictionary<float, float>();

        using (StreamReader reader = new StreamReader(file))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] lineContents = line.Split();
                Dimension d = new Dimension(float.Parse(lineContents[0]), float.Parse(lineContents[1]));
                valence.Add(d.timeStamp, d.level);
            }
        }

        return valence;
    }

}


