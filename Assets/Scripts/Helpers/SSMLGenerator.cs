using System;
using System.Xml;

///-----------------------------------------------------------------
///   Class:        SSMLGenerator.cs
///   Description:  This class is used to generate the SSML file,
///                 that will be used as input to the text-to-speech
///                 engine. The SSML file will be generated 
///                 regarding the desired text which is given 
///                 as argument.
///   Author:       Constantinos Charalambous     Date: 28/06/2017
///   Notes:        Generates an SSML file
///-----------------------------------------------------------------

public class SSMLGenerator
{
    private string synthesisUrl = "http://www.w3.org/2001/10/synthesis";
    private string xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private string schemaLocation = "http://www.w3.org/TR/speech-synthesis/synthesis.xsd";
    public string language = "en-US";

    /// <summary>
    /// Generates the SSML file according to the input string
    /// </summary>
    /// <param name="text">Text to be synthesized</param>
    public void GenerateSSML(string text)
    {
        XmlDocument xmlDoc = new XmlDocument();

        // header node
        XmlNode headerNode = xmlDoc.CreateXmlDeclaration("1.0", null, null);
        xmlDoc.AppendChild(headerNode);

        // initial attributes
        XmlNode speakNode = xmlDoc.CreateElement("speak");
        xmlDoc.AppendChild(speakNode);
        XmlAttribute speakVersion = xmlDoc.CreateAttribute("version");
        speakVersion.Value = "1.0";
        speakNode.Attributes.Append(speakVersion);
        XmlAttribute speakXmlns = xmlDoc.CreateAttribute("xmlns");
        speakXmlns.Value = synthesisUrl;
        speakNode.Attributes.Append(speakXmlns);
        XmlAttribute speakXmlnsXsi = xmlDoc.CreateAttribute("xmlns:xsi");
        speakXmlnsXsi.Value = xsi;
        speakNode.Attributes.Append(speakXmlnsXsi);
        XmlAttribute speakSchemaLocation = xmlDoc.CreateAttribute("xsi:schemaLocation");
        speakSchemaLocation.Value = schemaLocation;
        speakNode.Attributes.Append(speakSchemaLocation);
        XmlAttribute speakLanguage = xmlDoc.CreateAttribute("xml:lang");
        speakLanguage.Value = language;
        speakNode.Attributes.Append(speakLanguage);

        // text to be spoken
        XmlNode textNode = xmlDoc.CreateElement("p");
        XmlNode prosodyNode = xmlDoc.CreateElement("prosody");
        XmlAttribute prosodyRate = xmlDoc.CreateAttribute("rate");
        prosodyRate.Value = "x-slow";
        prosodyNode.Attributes.Append(prosodyRate);
        prosodyNode.AppendChild(xmlDoc.CreateTextNode(text));
        textNode.AppendChild(prosodyNode);
        speakNode.AppendChild(textNode);

        xmlDoc.Save(PathManager.getDataPath("textToSpeak.Xml"));
    }
}
