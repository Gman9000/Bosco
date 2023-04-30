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
    public string characterID;                              // character id string for denoting dialogue details and emotes
    [XmlAttribute]
    public string pawnID;                                   // pawn id string for animations and camera positioning

    [XmlAttribute]
    public string command;                                  // command string to be used to signal additional functionality and parameters

    [XmlAttribute]
    public string alias = null;                             // overwrite the character's nametag to be different than the .chr file

    [XmlAttribute]
    public int emote;                                       // emote index for a particular character

    [XmlAttribute]
    public int emotePosition = -3;                          // -3 is leftmost, 0 is center, 3 is rightmost

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
                SkitRunner.HighlightSpeaker(characterID);

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
                FollowType followType = FollowType.None;
                switch (command.ToLower())
                {
                    case "lerp":
                        followType = FollowType.Lerp;
                        break;
                    case "fixed":
                        followType = FollowType.Fixed;
                        break;
                    case "linear":
                        followType = FollowType.Linear;
                        break;
                }

                CameraController.Instance.SetTransformFollow(Pawn.Instances[pawnID].transform, followType, .1F);
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