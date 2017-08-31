using System;
using System.Xml;
using System.Collections.Generic;

///-----------------------------------------------------------------
///   Class:        XMLGenerator.cs
///   Description:  This class is used to generate XML files 
///                 required for the lip sync animation process
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Generates XML file
///-----------------------------------------------------------------

public static class XMLGenerator
{
    /// <summary>
    /// Generates an XML file containing the current phoneme information 
    /// together with the current blendshape mappings. The current information
    /// is based on the configuration specified with Unity's inspector.
    /// </summary>
    /// <param name="mappingList">List containing phoneme to blendshape mappings</param>
    /// <param name="includeWeight">Whether to include weight or not</param>
    public static void GeneratePhonemeXML(List<PhonemeBlendShape> mappingList, bool includeWeight, string modelName)
    {
        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode mainNode = xmlDoc.CreateElement("PhonemeMapping");
        xmlDoc.AppendChild(mainNode);

        foreach (PhonemeBlendShape pbs in mappingList)
        {
            XmlNode attrNode = xmlDoc.CreateElement("ATTR");
            mainNode.AppendChild(attrNode);

            // phonemes
            XmlNode phonemeNode = xmlDoc.CreateElement("Phoneme");
            phonemeNode.AppendChild(xmlDoc.CreateTextNode(pbs.phoneme.ToString()));
            attrNode.AppendChild(phonemeNode);

            // blendShapes 
            foreach (BlendShape bs in pbs.blendShapes)
            {
                XmlNode blendShapeNode = xmlDoc.CreateElement("BlendShape");
                XmlNode indexNode = xmlDoc.CreateElement("Index");
                indexNode.AppendChild(xmlDoc.CreateTextNode(bs.index.ToString()));
                XmlNode weightNode = xmlDoc.CreateElement("Weight");
                
                if (includeWeight)
                {
                    weightNode.AppendChild(xmlDoc.CreateTextNode(bs.weight.ToString()));
                }
                else
                {
                    weightNode.AppendChild(xmlDoc.CreateTextNode("100"));
                }
                
                blendShapeNode.AppendChild(indexNode);
                blendShapeNode.AppendChild(weightNode);
                mainNode.AppendChild(blendShapeNode);

                attrNode.AppendChild(blendShapeNode);
            }
            
        }

        xmlDoc.Save(PathManager.getXMLDataPath(modelName + "-phonemeMapping.Xml"));
    }

    /// <summary>
    /// Generates an XML file containing the current diphones information.
    /// The current information is based on the configuration specified
    /// with Unity's inspector.
    /// </summary>
    /// <param name="mappingList">List containing diphone to phoneme mappings</param>
    public static void GenerateDiphonesXML(List<DiphonePhonemes> mappingList, string modelName)
    {
        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode mainNode = xmlDoc.CreateElement("DiphoneMapping");
        xmlDoc.AppendChild(mainNode);

        foreach (DiphonePhonemes dp in mappingList)
        {
            XmlNode attrNode = xmlDoc.CreateElement("ATTR");
            mainNode.AppendChild(attrNode);

            // diphones
            XmlNode diphoneNode = xmlDoc.CreateElement("Diphone");
            diphoneNode.AppendChild(xmlDoc.CreateTextNode(dp.diphone.ToString()));
            attrNode.AppendChild(diphoneNode);

            // phonemes 
            foreach (Phoneme p in dp.phonemes)
            {
                XmlNode phonemeNode = xmlDoc.CreateElement("Phoneme");
                XmlNode typeNode = xmlDoc.CreateElement("Type");
                typeNode.AppendChild(xmlDoc.CreateTextNode(p.ToString()));

                phonemeNode.AppendChild(typeNode);
                mainNode.AppendChild(phonemeNode);

                attrNode.AppendChild(phonemeNode);
            }

        }

        xmlDoc.Save(PathManager.getXMLDataPath(modelName + "-diphoneMapping.Xml"));
    }

    /// <summary>
    /// Generates an XML file containing the current emotions information. 
    /// The current information is based on the configuration specified 
    /// with Unity's inspector.
    /// </summary>
    /// <param name="mappingList"></param>
    /// <param name="includeWeight"></param>
    public static void GenerateEmotionsXML(List<EmotionBlendShape> mappingList, bool includeWeight, string modelName)
    {
        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode mainNode = xmlDoc.CreateElement("EmotionMapping");
        xmlDoc.AppendChild(mainNode);

        foreach (EmotionBlendShape pbs in mappingList)
        {
            XmlNode attrNode = xmlDoc.CreateElement("ATTR");
            mainNode.AppendChild(attrNode);

            // phonemes
            XmlNode emotionNode = xmlDoc.CreateElement("Emotion");
            emotionNode.AppendChild(xmlDoc.CreateTextNode(pbs.emotion.ToString()));
            attrNode.AppendChild(emotionNode);

            // blendShapes 
            foreach (BlendShape bs in pbs.blendShapes)
            {
                XmlNode blendShapeNode = xmlDoc.CreateElement("BlendShape");
                XmlNode indexNode = xmlDoc.CreateElement("Index");
                indexNode.AppendChild(xmlDoc.CreateTextNode(bs.index.ToString()));
                XmlNode weightNode = xmlDoc.CreateElement("Weight");

                if (includeWeight)
                {
                    weightNode.AppendChild(xmlDoc.CreateTextNode(bs.weight.ToString()));
                }
                else
                {
                    weightNode.AppendChild(xmlDoc.CreateTextNode("100"));
                }

                blendShapeNode.AppendChild(indexNode);
                blendShapeNode.AppendChild(weightNode);
                mainNode.AppendChild(blendShapeNode);

                attrNode.AppendChild(blendShapeNode);
            }

        }

        xmlDoc.Save(PathManager.getXMLDataPath(modelName + "-emotionMapping.Xml"));
    }
    

}
