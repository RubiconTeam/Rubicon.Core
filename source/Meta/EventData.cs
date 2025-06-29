using System.Linq;
using Godot.Collections;
using Rubicon.Core.Chart;
using Array = Godot.Collections.Array;

namespace Rubicon.Core.Meta;

[GlobalClass]
public partial class EventData : Resource
{
    /// <summary>
    /// The time to trigger this event. Stored on disk in measures.
    /// </summary>
    [Export] public float Time = 0f;

    /// <summary>
    /// The event's name.
    /// </summary>
    [Export] public StringName Name;

    /// <summary>
    /// The event's arguments.
    /// </summary>
    [Export] public Dictionary<StringName, Variant> Arguments = new();
        
    /// <summary>
    /// The time converted from measures to milliseconds. Should be ignored when serialized.
    /// </summary>
    public float MsTime = 0f;

    /// <summary>
    /// Converts the pre-existing Time and Length variables to milliseconds and stores them in MsTime and MsLength, using the provided BpmInfo.
    /// </summary>
    /// <param name="bpmInfo">An Array of BpmInfos</param>
    public void ConvertData(TimeChange[] bpmInfo)
    {
        TimeChange bpm = bpmInfo.Last();
        for (int i = 0; i < bpmInfo.Length; i++)
        {
            if (bpmInfo[i].Time > Time)
            {
                bpm = bpmInfo[i - 1];
                break;
            }
        }

        MsTime = ConductorUtility.MeasureToMs(Time - bpm.Time, bpm.Bpm, bpm.TimeSignatureNumerator) + bpm.MsTime;
    }
}