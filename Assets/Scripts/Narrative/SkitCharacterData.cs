using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot]
public class SkitCharacterData
{
    [XmlAttribute]
    public string name;
    [XmlAttribute]
    public string unknownAlias;
    [XmlAttribute]
    public string id;
    [XmlAttribute]
    public string color;

    public string uniquePath => "pathgoeshere/" + id + "/";
}