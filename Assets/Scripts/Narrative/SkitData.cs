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


    // xml end 

    public SkitBeatData currentBeat = null;

    public IEnumerator ReadBeats()
    {
        for (int i = 0; i < beats.Length; i++)
        {
            currentBeat = beats[i];
            yield return currentBeat.Execute();
            if (currentBeat.beatType == SkitBeatType.Dialogue || currentBeat.beatType == SkitBeatType.ChoicePrompt)
                yield return new WaitUntil(() => PlayerInput.Pressed(Button.A) || PlayerInput.Pressed(Button.Start) || PlayerInput.Pressed(Button.B));
        }
        yield break;
    }
}