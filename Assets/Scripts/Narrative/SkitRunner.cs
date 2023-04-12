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
    static private TMP_Text dialogueObject;


    static public void Init()
    {
        GenerateTemplate();

        ReadCharacterDataAll();
        ReadSkitDataAll();
        active = false;
        
        dialogueObject = HUD.Instance.texts["Dialogue Text"];
    }


    static public void ReadCharacterDataAll()
    {
        characters = new Dictionary<string, SkitCharacterData>();
        
        // todo: read all character data from xml        
        // todo: put them all in a dictionary
    }

    static public void ReadSkitDataAll()
    {
        skits = new Dictionary<string, SkitData>();

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Gamedata/Skits/");
        FileInfo[] files = info.GetFiles("*.skit");

        string[] skitFilePaths = Directory.GetFiles(Application.dataPath + "/Gamedata/Skits/");
        foreach (string skitFilePath in skitFilePaths)
        {
            for (int i = 0; i < files.Length; i++)
            {
                FileStream fs = files[i].OpenRead();
                XmlSerializer serializer = new XmlSerializer(typeof(SkitData));
                SkitData skitData = serializer.Deserialize(fs) as SkitData;
                fs.Close();
                if (skits.ContainsKey(skitData.id))
                    skits.Remove(skitData.id);
                skits.Add(skitData.id, skitData);
            }
        }
    }

    static public void GenerateTemplate()
    {
        SkitData data = new SkitData();
        data.id = "Template";
        data.beats = new SkitBeatData[5];
        
        data.beats[0] = new SkitBeatData();
        data.beats[0].beatType = SkitBeatType.Dialogue;
        data.beats[0].readDelay = 2;
        data.beats[0].characterID = "Herringer";
        data.beats[0].text = "Items deemed sacred by the living,\nare cradled by beings unseen,\nthough not unfelt. ";
        
        data.beats[1] = new SkitBeatData();
        data.beats[1].beatType = SkitBeatType.Dialogue;
        data.beats[1].readDelay = 2;
        data.beats[1].characterID = "Herringer";
        data.beats[1].text = "That enchantment they sense,\nin places of grand importance,\nare creatures known as...";

        data.beats[2] = new SkitBeatData();
        data.beats[2].beatType = SkitBeatType.Dialogue;
        data.beats[2].readDelay = 2;
        data.beats[2].characterID = "Herringer";
        data.beats[2].text = "WISPS.";

        data.beats[3] = new SkitBeatData();
        data.beats[3].beatType = SkitBeatType.Dialogue;
        data.beats[3].readDelay = 2;
        data.beats[3].characterID = "Herringer";
        data.beats[3].text = "You, my perennial light,\nhave been born such a spirit.";

        data.beats[4] = new SkitBeatData();
        data.beats[4].beatType = SkitBeatType.Dialogue;
        data.beats[4].readDelay = 2;
        data.beats[4].characterID = "Herringer";
        data.beats[4].text = "May your path meet a proper end;\nAnd may your arm stay true.\n- Herringer ca.525";


        string pth = Application.dataPath + "/Gamedata/Skits/Template.skit";
        XmlSerializer serializer = new XmlSerializer(typeof(SkitData));
        FileStream fs = new FileStream(pth, FileMode.Create);
        serializer.Serialize(fs, data);
        fs.Close();
    }

    static public IEnumerator BeginSkit(string skitID)
    {        
        currentSkit = skits[skitID];
        yield return currentSkit.ReadBeats();
        active = false;

        yield break;
    }

    static public void DialogueWrite(string characterID, string richtext)
    {
        SkitCharacterData character = characters.ContainsKey(characterID) ? characters[characterID] : null;

        string characterName = character != null ? character.name : characterID;
        string characterColor = character != null ? character.color : "#FFCC00";

        dialogueObject.text = "<color=" + characterColor + ">" + characterName + ":</color> \n" + richtext;
    }
}