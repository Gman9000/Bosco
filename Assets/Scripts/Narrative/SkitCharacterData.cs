using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("character")]
public class SkitCharacterData
{
    [XmlAttribute]
    public string name;
    [XmlAttribute]
    public string id;
    [XmlAttribute]
    public string color;
    public string resourcePath => "Characters/" + id + "/";
    [XmlIgnore]
    private Sprite[] emotes;

    public void LoadEmotes()
    {
        emotes = Resources.LoadAll<Sprite>(resourcePath);
    } 
    public Sprite GetEmote(int emoteIndex) => emotes[emoteIndex];


}