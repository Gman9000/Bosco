using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("skit")]
public class SkitData
{
    [XmlArray]
    public SkitBeatData[] beats;

    [XmlAttribute]
    public string id;

    [XmlIgnore]
    public List<string> cast = null;

    // xml end
    public SkitBeatData currentBeat = null;

    public void IndexCast()
    {
        cast = new List<string>();
        foreach (SkitBeatData beat in beats)    // quick read through all the beats and find all the speaking characters to index
            if (!cast.Contains(beat.characterID))
            {
                cast.Add(beat.characterID);
            }
    }

    public IEnumerator ReadBeats()
    {
        IndexCast();

        for (int i = 0; i < beats.Length; i++)
        {
            currentBeat = beats[i];
            yield return currentBeat.Execute();
            if ((currentBeat.beatType == SkitBeatType.Dialogue && currentBeat.waitForInput) || currentBeat.beatType == SkitBeatType.ChoicePrompt)
                yield return new WaitUntil(() => PlayerInput.Pressed(Button.A) || PlayerInput.Pressed(Button.Start) || PlayerInput.Pressed(Button.B));
        }
        yield break;
    }
}