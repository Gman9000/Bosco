using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System.Xml;
using System.IO;
using System.Xml.Serialization;

static public class SkitRunner
{
    static public bool active;
    static public Dictionary<string, SkitCharacterData> characters;
    static public Dictionary<string, SkitData> skits;
    static public SkitData currentSkit;
    static private Coroutine skitCoroutine;
    static private TMP_Text dialogueObject;


    static public void Init()
    {
        ReadCharacterDataAll();
        ReadSkitDataAll();
        skitCoroutine = null;
        active = false;
        
        dialogueObject = HUD.Instance.texts["Dialogue Text"];


        // temp
        GenerateTemplate();
    }


    static public void ReadCharacterDataAll()
    {
        // todo: read all character data from xml
        
        // todo: put them all in a dictionary
    }

    static public void ReadSkitDataAll()
    {
        // todo: read all skit data from xml

        // todo: put them all in a dictionary
    }

    static public void GenerateTemplate()
    {
        SkitData data = new SkitData();
        data.id = "Template";
        data.beats = new SkitBeatData[4];
        
        data.beats[0] = new SkitBeatData();
        data.beats[0].beatType = SkitBeatType.Dialogue;
        data.beats[0].characterID = "Herringer";
        data.beats[0].text = "Things deemed sacred by the living,\nare cradled by beings unseen...\nyet not unfelt. ";
        
        data.beats[1] = new SkitBeatData();
        data.beats[1].beatType = SkitBeatType.Dialogue;
        data.beats[1].characterID = "Herringer";
        data.beats[1].text = "The spiritual feeling you get when standing at a place of great importance...\nare creatures called\nWISPS.";

        data.beats[2] = new SkitBeatData();
        data.beats[2].beatType = SkitBeatType.Dialogue;
        data.beats[2].characterID = "Herringer";
        data.beats[2].text = "You,\nmy perennial light,\nhave been born as one of them.";

        data.beats[3] = new SkitBeatData();
        data.beats[3].beatType = SkitBeatType.Dialogue;
        data.beats[3].characterID = "Herringer";
        data.beats[3].text = "May your path meet an end;\nAnd may your arm stay noble.\n- Herringer ca.525";


        string pth = Application.dataPath + "/Skits/Template.skit";
        XmlSerializer serializer = new XmlSerializer(typeof(SkitData));
        FileStream fs = new FileStream(pth, FileMode.Create);
        serializer.Serialize(fs, data);
        fs.Close();
    }

    static public void BeginSkit(string skitID)
    {
        // TODO: Actually have data to read, otherwise it'll bust


        currentSkit = skits[skitID];
        if (skitCoroutine == null)
            Game.Instance.StopCoroutine(skitCoroutine);
        skitCoroutine = Game.Instance.StartCoroutine(currentSkit.ReadBeats());
    }

    static public void DialogueWrite(string characterID, string richtext)
    {
        SkitCharacterData character = characters.ContainsKey(characterID) ? characters[characterID] : null;

        string characterName = character != null ? character.name : characterID;
        string characterColor = character != null ? character.color : "#FFCC00";

        dialogueObject.text = "<color=" + characterColor + ">" + characterName + ":</color> " + richtext;
    }
}