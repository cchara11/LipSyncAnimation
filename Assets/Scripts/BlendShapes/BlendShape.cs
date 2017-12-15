using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        BlendShape.cs
///   Description:  This class contains all the information for each
///                 blendshape. It includes the name of the
///                 blendshape, the corresponding index, as well as
///                 the target and the current weights
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        BlendShape information
///-----------------------------------------------------------------

[System.Serializable]
public class BlendShape
{
    public string name;
    public int index;
    public float weight;
    public float currentWeight;

    public BlendShape()
    {
        weight = 0;
        currentWeight = 0;
    }

    public BlendShape(string name, int index)
    {
        this.name = name;
        this.index = index;
        weight = 0;
        currentWeight = 0;
    }

    /// <summary>
    /// Sets the weight of the blendShape
    /// </summary>
    /// <param name="weight"></param>
    public void setBlendShapeWeight(float weight)
    {
        this.weight = weight;
    }

    /// <summary>
    /// Populates a list of the available blendShapes present in the model.
    /// List contains the name of the blendShape, a respective index and a weight value.
    /// </summary>
    /// <param name="characterMesh"></param>
    /// <returns></returns>
    public static List<BlendShape> populateBlendShapeList(SkinnedMeshRenderer characterMesh)
    {
        List<BlendShape> blendShapes = new List<BlendShape>();

        for (int i = 0; i < characterMesh.sharedMesh.blendShapeCount; i++)
        {
            string blendShapeName = characterMesh.sharedMesh.GetBlendShapeName(i);
            blendShapes.Add(new BlendShape(blendShapeName, i)); 
        }

        return blendShapes;
    }
}
