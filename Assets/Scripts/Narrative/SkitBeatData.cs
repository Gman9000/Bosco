using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public enum SkitBeatType {Dialogue, EmoteOnly, Camera, PawnCall, ChoicePrompt}

[XmlRoot("beat")]
public class SkitBeatData
{
    [XmlAttribute]
    public SkitBeatType beatType = SkitBeatType.Dialogue;

    [XmlAttribute]
    public string text;                                     // NOTE: Limit this to 3 lines per dialogue box
    
    [XmlAttribute]
    public string characterID;
    [XmlAttribute]
    public string alias = null;

    [XmlAttribute]
    public int emote;

    [XmlAttribute]
    public int emotePosition = -3; // -3 is leftmost, 0 is center, 3 is rightmost

    [XmlAttribute]
    public float readDelay = 1;

    [XmlAttribute]
    public bool punctuationPause = true;

    [XmlAttribute]
    public bool skippable = true;
    [XmlAttribute]
    public bool waitForInput = true;

    // xml end

    [XmlIgnore]
    public SkitCharacterData speakingCharacter;

    [XmlIgnore]
    public bool complete = false;

    [XmlIgnore]
    private string outputRichText = "";

    public IEnumerator Execute()
    {
        complete = false;

        switch (beatType)
        {
            case SkitBeatType.Dialogue:
                SkitRunner.EmoteSet(characterID, emotePosition, emote);

                for (int c = 0; c < text.Length; c++)
                {
                    outputRichText = text.Substring(0, c+1);
                    outputRichText += "<color=#00000000>";
                    outputRichText += text.Substring(c+1);
                    outputRichText += "</color>";

                    if (complete)
                    {
                        SkitRunner.DialogueWrite(this, text);
                        yield return null;
                    }
                    else
                    {
                        SkitRunner.DialogueWrite(this, outputRichText);                    
                        if (punctuationPause)
                        {
                            if (c < text.Length - 2 && text[c] == '.' && text[c+1] != '.')    // handle periods or ellipses
                            {
                                yield return new WaitForSeconds(readDelay * Game.FRAME_TIME * 5);     // extra delay
                            }
                            else if (text[c] == ',' || text[c] == '.' || text[c] == ':' || text[c] == ';' || text[c] == '?' || text[c] == '!')    // handle punctuation
                            {
                                yield return new WaitForSeconds(readDelay * Game.FRAME_TIME * 5);     // extra delay
                            }
                        }
                        
                        yield return new WaitForSeconds(readDelay * Game.FRAME_TIME);
                    }
                }


                // ## END OF THIS DIALOGUE BOX ##
                break;

            case SkitBeatType.EmoteOnly:
                SkitRunner.EmoteSet(characterID, emotePosition, emote);
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

        yield return null;
        complete = true;
        yield break;
    }
}