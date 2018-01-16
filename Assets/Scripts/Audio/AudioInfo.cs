using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInfo
{
    private static int frequency = 48000;

    public static float getExactDuration(int sampleDuration)
    {
        return (float)sampleDuration / (float)frequency;
    }

}
