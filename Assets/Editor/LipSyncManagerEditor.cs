using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

///-----------------------------------------------------------------
///   Class:        LipSyncManagerEditor.cs
///   Description:  This class is a helper of the lip sync 
///                 unity component, providing ease of the component
///                 customization through unity's inspector
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Editor Class
///-----------------------------------------------------------------
///
[CustomEditor(typeof(LipSync))]
public class LipSyncManagerEditor : Editor
{
    private LipSync lipSyncInstance;
    List<bool> staticPhonemeFoldout, dynamicPhonemeFoldout, emotionFoldout;
    List<string> blendShapeList;
    bool includeWeights = false;
    string[] genericToolbar;
    string[] phonemeToolbar;
    int genericToolbarIndex, phonemeToolbarIndex = 0;

    enum InteractionType
    {
        Phonemes,
        Emotions
    }

    // Initializations
    private void Awake()
    {
        lipSyncInstance = (LipSync)target;
        staticPhonemeFoldout = new List<bool>();
        dynamicPhonemeFoldout = new List<bool>();
        emotionFoldout = new List<bool>();
        blendShapeList = FillBlendShapeList(lipSyncInstance.characterMesh);
        InstantiateFoldout(InteractionType.Phonemes);
        InstantiateFoldout(InteractionType.Emotions);
        genericToolbar = new string[] { "Phonemes", "Emotions" };
        phonemeToolbar = new string[] { "Static", "Dynamic" };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);
        genericToolbarIndex = GUILayout.Toolbar(genericToolbarIndex, genericToolbar);

        if (genericToolbar[genericToolbarIndex].Equals("Phonemes"))
        {
            GeneratePhonemesGUI();
        }
        else
        {
            GenerateEmotionsGUI();
        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("XML Export", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Export configuration to XML"))
        {
            ExportXML();
        }

        includeWeights = EditorGUILayout.Toggle("Include Weights", includeWeights, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Phonemes GUI and mapping
    /// </summary>
    void GeneratePhonemesGUI()
    {
        phonemeToolbarIndex = GUILayout.Toolbar(phonemeToolbarIndex, phonemeToolbar);
        
        if (phonemeToolbar[phonemeToolbarIndex].Equals("Static"))
        {
            GenerateStaticVisemesGUI();
        }
        else
        {
            GenerateDynamicVisemesGUI();
        }
        
    }

    /// <summary>
    /// Emotions GUI and mapping
    /// </summary>
    void GenerateEmotionsGUI()
    {
        GUILayout.BeginHorizontal();
        AddControlButtons(InteractionType.Emotions);
        GUILayout.EndHorizontal();

        for (int i = 0; i < lipSyncInstance.emotions.Count; i++)
        {
            emotionFoldout[i] = EditorGUILayout.Foldout(emotionFoldout[i], "Emotion Mapping - " + lipSyncInstance.emotions[i].emotion);

            if (emotionFoldout[i])
            {
                UnfoldRest(InteractionType.Emotions, i, emotionFoldout);

                GUILayout.BeginHorizontal();

                // Emotion Popup
                GUILayout.Label("Emotion:");
                lipSyncInstance.emotions[i].emotion = (Emotion)EditorGUILayout.EnumPopup(lipSyncInstance.emotions[i].emotion);

                // remove button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GetRemoveButtonStyle()))
                {
                    RemoveMapping(InteractionType.Emotions, i);
                    continue;
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();

                // BlendShape Popup
                if (GUILayout.Button("Add BlendShape Mapping"))
                {
                    AddBlendShapeMapping(InteractionType.Emotions, i);
                }

                for (int j = 0; j < lipSyncInstance.emotions[i].blendShapes.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    int prevBlendShapeIndex = lipSyncInstance.emotions[i].blendShapes[j].index;
                    GUILayout.Label("BlendShape:");
                    lipSyncInstance.emotions[i].blendShapes[j].index = EditorGUILayout.Popup(lipSyncInstance.emotions[i].blendShapes[j].index, blendShapeList.ToArray(), EditorStyles.popup);

                    // remove button
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GetRemoveButtonStyle()))
                    {
                        RemoveBlendShapeMapping(InteractionType.Emotions, i, j);
                        continue;
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    lipSyncInstance.emotions[i].blendShapes[j].weight = GUILayout.HorizontalSlider(lipSyncInstance.emotions[i].blendShapes[j].weight, 0, 100);
                    GUILayout.Label(lipSyncInstance.emotions[i].blendShapes[j].weight.ToString(), GUILayout.Width(50));

                    if (GUI.changed)
                    {
                        lipSyncInstance.characterMesh.SetBlendShapeWeight(prevBlendShapeIndex, 0);
                        lipSyncInstance.characterMesh.SetBlendShapeWeight(lipSyncInstance.emotions[i].blendShapes[j].index, lipSyncInstance.emotions[i].blendShapes[j].weight);
                    }

                    GUILayout.EndHorizontal();

                }
                GUILayout.Space(20);
            }
        }
    }

    /// <summary>
    /// Static Visemes GUI and mapping
    /// </summary>
    void GenerateStaticVisemesGUI()
    {
        GUILayout.BeginHorizontal();
        AddControlButtons(InteractionType.Phonemes);
        GUILayout.EndHorizontal();

        for (int i = 0; i < lipSyncInstance.staticVisemes.Count; i++)
        {
            staticPhonemeFoldout[i] = EditorGUILayout.Foldout(staticPhonemeFoldout[i], "Static Viseme - " + lipSyncInstance.staticVisemes[i].phoneme);

            if (staticPhonemeFoldout[i])
            {
                UnfoldRest(InteractionType.Phonemes, i, staticPhonemeFoldout);
                //InitializeModel(i);
                GUILayout.BeginHorizontal();
                // Phoneme Popup
                GUILayout.Label("Phoneme:");
                lipSyncInstance.staticVisemes[i].phoneme = (Phoneme)EditorGUILayout.EnumPopup(lipSyncInstance.staticVisemes[i].phoneme);

                // remove button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GetRemoveButtonStyle()))
                {
                    RemoveMapping(InteractionType.Phonemes, i);
                    continue;
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();

                // BlendShape Popup
                if (GUILayout.Button("Add BlendShape Mapping"))
                {
                    AddBlendShapeMapping(InteractionType.Phonemes, i);
                }

                for (int j = 0; j < lipSyncInstance.staticVisemes[i].blendShapes.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    List<string> blendShapeList = FillBlendShapeList(lipSyncInstance.characterMesh);
                    int prevBlendShapeIndex = lipSyncInstance.staticVisemes[i].blendShapes[j].index;
                    GUILayout.Label("BlendShape:");
                    lipSyncInstance.staticVisemes[i].blendShapes[j].index = EditorGUILayout.Popup(lipSyncInstance.staticVisemes[i].blendShapes[j].index, blendShapeList.ToArray(), EditorStyles.popup);

                    // remove button
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GetRemoveButtonStyle()))
                    {
                        RemoveBlendShapeMapping(InteractionType.Phonemes, i, j);
                        continue;
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    lipSyncInstance.staticVisemes[i].blendShapes[j].weight = GUILayout.HorizontalSlider(lipSyncInstance.staticVisemes[i].blendShapes[j].weight, 0, 100);
                    GUILayout.Label(lipSyncInstance.staticVisemes[i].blendShapes[j].weight.ToString(), GUILayout.Width(50));

                    if (GUI.changed)
                    {
                        lipSyncInstance.characterMesh.SetBlendShapeWeight(prevBlendShapeIndex, 0);
                        lipSyncInstance.characterMesh.SetBlendShapeWeight(lipSyncInstance.staticVisemes[i].blendShapes[j].index, lipSyncInstance.staticVisemes[i].blendShapes[j].weight);
                    }

                    GUILayout.EndHorizontal();

                }
                GUILayout.Space(20);
            }
        }
    }

    /// <summary>
    /// Dynamic Visemes GUI and mapping
    /// </summary>
    void GenerateDynamicVisemesGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New"))
        {
            AddDynamicViseme();
        }
        if (GUILayout.Button("Reset Mapping"))
        {
            ResetDynamicVisemes();
        }
        GUILayout.EndHorizontal();

        for (int i = 0; i < lipSyncInstance.dynamicVisemes.Count; i++)
        {
            dynamicPhonemeFoldout[i] = EditorGUILayout.Foldout(dynamicPhonemeFoldout[i], "Dynamic Viseme - " + lipSyncInstance.dynamicVisemes[i].diphone);

            if (dynamicPhonemeFoldout[i])
            {
                UnfoldRest(InteractionType.Phonemes, i, dynamicPhonemeFoldout);
                //InitializeModel(i);
                GUILayout.BeginHorizontal();
                // Diphone Popup
                lipSyncInstance.dynamicVisemes[i].diphone = (Diphone)EditorGUILayout.EnumPopup(lipSyncInstance.dynamicVisemes[i].diphone);

                // remove button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GetRemoveButtonStyle()))
                {
                    RemoveMapping(InteractionType.Phonemes, i);
                    continue;
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();

                // Phoneme Popup
                if (GUILayout.Button("Add Phoneme Mapping"))
                {
                    AddPhonemeMapping(i);
                }
                
                for (int j = 0; j < lipSyncInstance.dynamicVisemes[i].phonemes.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    
                    lipSyncInstance.dynamicVisemes[i].phonemes[j] = (Phoneme)EditorGUILayout.EnumPopup("Phoneme:", lipSyncInstance.dynamicVisemes[i].phonemes[j]);

                    // remove button
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GetRemoveButtonStyle()))
                    {
                        RemovePhonemeMapping(i, lipSyncInstance.dynamicVisemes[i].phonemes[j]);
                        continue;
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(20);
            }
        }
    }

    /// <summary>
    /// Control Buttons in the inspector
    /// </summary>
    void AddControlButtons(InteractionType type)
    {
        if (GUILayout.Button("Add New"))
        {
            if (type.Equals(InteractionType.Phonemes))
            {
                AddStaticVisemeMapping();
                staticPhonemeFoldout.Add(false);
            }
            else
            {
                AddEmotionMapping();
                emotionFoldout.Add(false);
            }
        }

        if (GUILayout.Button("Reset Model"))
        {
            ResetMapping(type, false);
        }

        if (GUILayout.Button("Reset Mapping"))
        {
            ResetMapping(type, true);
        }
    }

    /// <summary>
    /// Style of the remove button of the inspector
    /// </summary>
    GUIStyle GetRemoveButtonStyle()
    {
        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.fontStyle = FontStyle.Bold;

        return buttonStyle;
    }

    /// <summary>
    /// Generates blendshape list from the current model
    /// </summary>
    List<string> FillBlendShapeList(SkinnedMeshRenderer mesh)
    {
        List<string> blendShapes = new List<string>();
        Mesh sharedMesh = mesh.sharedMesh;
        for (int i = 0; i < sharedMesh.blendShapeCount; i++)
        {
            string blendShapeName = sharedMesh.GetBlendShapeName(i);
            blendShapes.Add(blendShapeName + "(" + i + ")");
        }

        return blendShapes;
    }

    /// <summary>
    /// Custom phoneme popup
    /// </summary>
    List<string> GetCustomPhonemePopup()
    {
        List<string> phonemes = new List<string>();
        foreach (Phoneme p in Enum.GetValues(typeof(Phoneme)))
        {
            if (p.ToString().Contains("Diphone"))
            {
                continue;
            }

            phonemes.Add(p.ToString());
        }

        return phonemes;
    }

    /// <summary>
    /// Adds a new static viseme mapping
    /// </summary>
    void AddStaticVisemeMapping()
    {
        lipSyncInstance.staticVisemes.Add(new PhonemeBlendShape());
    }

    /// <summary>
    /// Adds a new dynamic viseme 
    /// </summary>
    void AddDynamicViseme()
    {
        lipSyncInstance.dynamicVisemes.Add(new DiphonePhonemes());
        dynamicPhonemeFoldout.Add(false);
    }

    /// <summary>
    /// Adds a new emotion mapping
    /// </summary>
    void AddEmotionMapping()
    {
        lipSyncInstance.emotions.Add(new EmotionBlendShape());
    }

    /// <summary>
    /// Adds a new blendshape mapping
    /// </summary>
    void AddBlendShapeMapping(InteractionType type, int index)
    {
        if (type.Equals(InteractionType.Phonemes))
        {
            lipSyncInstance.staticVisemes[index].blendShapes.Add(new BlendShape());                
        }
        else
        {
            lipSyncInstance.emotions[index].blendShapes.Add(new BlendShape());
        }
    }

    /// <summary>
    /// Adds a new dynamic viseme mapping
    /// </summary>
    void AddPhonemeMapping(int index)
    {
        lipSyncInstance.dynamicVisemes[index].phonemes.Add(new Phoneme());
    }

    /// <summary>
    /// Resets the mapping of the current configuration
    /// </summary>
    void RemoveMapping(InteractionType type, int index)
    {
        if (type.Equals(InteractionType.Phonemes))
        {
            lipSyncInstance.staticVisemes.RemoveAt(index);
            staticPhonemeFoldout.RemoveAt(index);
        }
        else
        {
            lipSyncInstance.emotions.RemoveAt(index);
            emotionFoldout.RemoveAt(index);
        }

    }

    /// <summary>
    /// Resets all blendshape mappings
    /// </summary>
    void RemoveBlendShapeMapping(InteractionType type, int typeIndex, int blendShapeIndex)
    {
        int currentBlendShapeIndex;
        if (type.Equals(InteractionType.Phonemes))
        {
            currentBlendShapeIndex = lipSyncInstance.staticVisemes[typeIndex].blendShapes[blendShapeIndex].index;
            lipSyncInstance.staticVisemes[typeIndex].blendShapes.RemoveAt(blendShapeIndex);          
        }
        else
        {
            currentBlendShapeIndex = lipSyncInstance.emotions[typeIndex].blendShapes[blendShapeIndex].index;
            lipSyncInstance.emotions[typeIndex].blendShapes.RemoveAt(blendShapeIndex);
        }

        // initialize blendshape weight
        lipSyncInstance.characterMesh.SetBlendShapeWeight(currentBlendShapeIndex, 0);
    }

    /// <summary>
    /// Resets the phoneme mapping configuration
    /// </summary>
    void RemovePhonemeMapping(int typeIndex, Phoneme currentPhoneme)
    {
        int currentPhonemeIndex;

        currentPhonemeIndex = lipSyncInstance.dynamicVisemes[typeIndex].phonemes.IndexOf(currentPhoneme);
        lipSyncInstance.dynamicVisemes[typeIndex].phonemes.RemoveAt(currentPhonemeIndex);
    }

    /// <summary>
    /// Resets the current configuration of the component
    /// </summary>
    void ResetMapping(InteractionType type, bool clearAll)
    {
        // re-initialize model
        Mesh sharedMesh = lipSyncInstance.characterMesh.sharedMesh;
        for (int i = 0; i < sharedMesh.blendShapeCount; i++)
        {
            lipSyncInstance.characterMesh.SetBlendShapeWeight(i, 0);
        }

        // initialize foldouts (inspector)
        for (int i = 0; i < staticPhonemeFoldout.Count; i++)
        {
            staticPhonemeFoldout[i] = false;
        }

        for (int i = 0; i < emotionFoldout.Count; i++)
        {
            emotionFoldout[i] = false;
        }

        if (clearAll)
        {
            if (type.Equals(InteractionType.Phonemes))
            {
                lipSyncInstance.staticVisemes.Clear();
                staticPhonemeFoldout.Clear();
            }
            else
            {
                lipSyncInstance.emotions.Clear();
                emotionFoldout.Clear();
            }
        }
    }

    /// <summary>
    /// Resets the dynamic visemes' configuration
    /// </summary>
    void ResetDynamicVisemes()
    {
        for (int i = 0; i < dynamicPhonemeFoldout.Count; i++)
        {
            dynamicPhonemeFoldout[i] = false;
        }

        lipSyncInstance.dynamicVisemes.Clear();
        dynamicPhonemeFoldout.Clear();
    }

    /// <summary>
    /// Initializes the 3D models' blendshapes to default
    /// </summary>
    void InitializeModel(int index)
    {
        Mesh sharedMesh = lipSyncInstance.characterMesh.sharedMesh;
        for (int i = 0; i < lipSyncInstance.staticVisemes.Count; i++)
        {
            if (i != index)
            {
                List<BlendShape> blendShapes = lipSyncInstance.staticVisemes[i].blendShapes;
                foreach (BlendShape bs in blendShapes)
                {
                    lipSyncInstance.characterMesh.SetBlendShapeWeight(bs.index, 0);
                }
            }
        }
    }

    /// <summary>
    /// Exports XML files of the current lip sync component configuration
    /// </summary>
    void ExportXML()
    {
        XMLGenerator.GeneratePhonemeXML(lipSyncInstance.staticVisemes, includeWeights);
        XMLGenerator.GenerateDiphonesXML(lipSyncInstance.dynamicVisemes);
        XMLGenerator.GenerateEmotionsXML(lipSyncInstance.emotions, includeWeights);
    }

    /// <summary>
    /// Instantiates the inepsctor foldouts
    /// </summary>
    void InstantiateFoldout(InteractionType type)
    {
        if (type.Equals(InteractionType.Phonemes))
        {
            for (int i = 0; i < lipSyncInstance.staticVisemes.Count; i++)
            {
                staticPhonemeFoldout.Add(true);
            }

            for (int i = 0; i < lipSyncInstance.dynamicVisemes.Count; i++)
            {
                dynamicPhonemeFoldout.Add(true);
            }
        }
        else
        {
            for (int i = 0; i < lipSyncInstance.emotions.Count; i++)
            {
                emotionFoldout.Add(true);
            }
        }

    }

    /// <summary>
    /// Helper for the inspector foldout
    /// </summary>
    void UnfoldRest(InteractionType type, int index, List<bool> list)
    {
        if (type.Equals(InteractionType.Phonemes))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i != index)
                {
                    list[i] = false;
                }
            }
        }
    }
}
