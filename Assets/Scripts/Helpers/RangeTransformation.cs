using System.Collections;
using System.Collections.Generic;

public class RangeTransformation
{
    public static float Transform(float x, float a, float b, float c, float d)
    {
        return (x - a) * ((d - c) / (b - a)) + c;
    }
}
