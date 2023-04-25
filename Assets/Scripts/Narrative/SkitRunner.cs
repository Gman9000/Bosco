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
        GenerateSkitTemplate();
        GenerateCharacterTemplate();

        ReadCharacterDataAll();
        ReadSkitDataAll();
        active = false;
        
        dialogueObject = HUD.Instance.texts["Dialogue Text"];
    }


    static public void ReadCharacterDataAll()
    {
        characters = new Dictionary<string, SkitCharacterData>();

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Gamedata/Characters/");
        FileInfo[] files = info.GetFiles("*.chr");
        
        foreach (FileInfo file in files)
        {
            FileStream fs = file.OpenRead();
            XmlSerializer serializer = new XmlSerializer(typeof(SkitCharacterData));
            SkitCharacterData characterData = serializer.Deserialize(fs) as SkitCharacterData;
            fs.Close();

            if (characters.ContainsKey(characterData.id))
                characters.Remove(characterData.id);
            characterData.LoadEmotes();
            characters.Add(characterData.id, characterData);
        }
    }

    static public void ReadSkitDataAll()
    {
        skits = new Dictionary<string, SkitData>();

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Gamedata/Skits/");
        FileInfo[] files = info.GetFiles("*.skit");

        foreach (FileInfo file in files)
        {
            FileStream fs = file.OpenRead();
            XmlSerializer serializer = new XmlSerializer(typeof(SkitData));
            SkitData skitData = serializer.Deserialize(fs) as SkitData;
            fs.Close();
            if (skits.ContainsKey(skitData.id))
                skits.Remove(skitData.id);
            skits.Add(skitData.id, skitData);
        }        
    }

    static public void GenerateCharacterTemplate()
    {
        SkitCharacterData data = new SkitCharacterData();
        data.id = "temp";
        data.name = "Default Guy";
        data.color = "#00CCEE";

        string pth = Application.dataPath + "/Gamedata/Characters/Template.chr";
        XmlSerializer serializer = new XmlSerializer(typeof(SkitCharacterData));
        FileStream fs = new FileStream(pth, FileMode.Create);
        serializer.Serialize(fs, data);
        fs.Close();
    }

    static public void GenerateSkitTemplate()
    {
        SkitData data = new SkitData();
        data.id = "Template";
        data.beats = new SkitBeatData[5];
        
        data.beats[0] = new SkitBeatData();
        data.beats[0].beatType = SkitBeatType.Dialogue;
        data.beats[0].readDelay = 2;
        data.beats[0].characterID = "Herringer";
        data.beats[0].text = "Items the living deem sacred,\nare cradled by beings unseen,\nthough not unfelt. ";
        
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

    static public void EmoteSet(string characterID, int emotePosition, int emoteID)
    {
        SkitCharacterData character = characters.ContainsKey(characterID) ? characters[characterID] : null;

        SpriteRenderer ren = HUD.Instance.renderers["Character " + currentSkit.cast.IndexOf(characterID)];
        
        ren.sprite = character.GetEmote(emoteID);
        ren.enabled = true;

        ren.flipX = emotePosition >= 0;
        
        float x = emotePosition == 0 ? 0 : Mathf.Sign(emotePosition) * 3F + emotePosition;

        Game.Instance.StartCoroutine(MoveEmote(ren.transform, new Vector3(x, ren.transform.localPosition.y, ren.transform.localPosition.z), .1F));
    }

    static public void HighlightSpeaker(string speakerID)
    {
        foreach (string characterID in currentSkit.cast)
        {
            SpriteRenderer ren = HUD.Instance.renderers["Character " + currentSkit.cast.IndexOf(characterID)];
            if (characterID == speakerID)
                ren.color = Color.white;
            else
                ren.color = Color.gray;
        }
    }

    static public void DialogueWrite(SkitBeatData data, string richtext)
    {
        string characterID = data.characterID;
        SkitCharacterData character = characters.ContainsKey(characterID) ? characters[characterID] : null;
        
        string characterName = character != null ? character.name : characterID;
        
        if (data.alias != null && data.alias != "")
            characterName = data.alias;
        string characterColor = character != null ? character.color : "#00CCCC";

        dialogueObject.text = "<color=" + characterColor + ">" + characterName + ":</color> \n" + richtext;
    }


    static public IEnumerator MoveEmote(Transform t, Vector3 moveTo, float speed)
    {
        float progress = 0;
        Vector3 start = t.localPosition;
        Vector3 difference = moveTo - start; 
        while (progress < 1)
        {
            t.localPosition = start + difference * progress;
            progress += speed;
            yield return new WaitForSeconds(Game.FRAME_TIME);
        }

        t.localPosition = moveTo;
        yield break;
    }
}