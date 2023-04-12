using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public enum SkitBeatType {Dialogue, Camera, PawnCall, ChoicePrompt}

[XmlRoot]
public class SkitBeatData
{
    [XmlAttribute]
    public SkitBeatType beatType = SkitBeatType.Dialogue;

    [XmlAttribute]
    public string text;                                     // NOTE: Limit this to 3 lines per dialogue box
    
    [XmlAttribute]
    public string characterID;

    [XmlAttribute]
    public int emote;

    [XmlAttribute]
    public float readDelay = 1.0F;

    // xml end

    public SkitCharacterData speakingCharacter;

    public bool complete = false;

    private string outputRichText = "";

    public virtual IEnumerator Execute()
    {
        complete = false;

        switch (beatType)
        {
            case SkitBeatType.Dialogue:
                for (int c = 0; c < text.Length; c++)
                {
                    outputRichText = text.Substring(0, c);
                    outputRichText += "<color=#00000000>";
                    outputRichText += text.Substring(c);
                    outputRichText += "</color>";
                    SkitRunner.DialogueWrite(characterID, outputRichText);
                    yield return new WaitForSeconds(readDelay);
                }
                // ## END OF THIS DIALOGUE BOX ##
                break;

            case SkitBeatType.Camera:
                // TODO: interpolate camera to a target point
                break;

            case SkitBeatType.PawnCall:
                // TODO: Create a way to send commands to different pawns through the XML
                // NOTE: do NOT use Invoke. It always breaks
                break;

            case SkitBeatType.ChoicePrompt:
                // TODO: Simple multiple choice prompt
                // NOTE: structure it to start a new cutscene to keep it organized
                break;
        }

        complete = true;
        yield break;
    }
}