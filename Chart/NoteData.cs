using System.Collections.Generic;
using System.Linq;
using System.Text;
using Array = Godot.Collections.Array;

namespace Rubicon.Core.Chart;

/// <summary>
/// Used for storing a note's data values. Is not the actual Note object.
/// </summary>
[GlobalClass]
public partial class NoteData : Resource
{
    /// <summary>
    /// The note's lane.
    /// </summary>
    [Export] public int Lane;
        
    /// <summary>
    /// The note's type.
    /// </summary>
    [Export] public StringName Type = "normal";

    /// <summary>
    /// Starting point of the note. Stored on disk in measures.
    /// </summary>
    [Export] public float Time;

    /// <summary>
    /// Length of the note. Stored on disk in measures.
    /// </summary>
    [Export] public float Length;

    /// <summary>
    /// Any extra parameters will be stored here.
    /// </summary>
    [Export] public Godot.Collections.Dictionary<StringName, Variant> Parameters = new();
        
    /// <summary>
    /// Length of the note converted to milliseconds. Should be ignored when serialized.
    /// </summary>
    public float MsTime;
        
    /// <summary>
    /// Length of the note converted to milliseconds. Should be ignored when serialized.
    /// </summary>
    public float MsLength;

    /// <summary>
    /// The starting scroll velocity this note is associated with.
    /// </summary>
    public int StartingScrollVelocity = 0;

    /// <summary>
    /// The ending scroll velocity for this note, if its length is more than 0.
    /// </summary>
    public int EndingScrollVelocity = 0;

    /// <summary>
    /// Basically tells the autoplay whether to miss this note or not. Should be ignored when serialized.
    /// </summary>
    [Export] public bool ShouldMiss = false;
    
    /// <summary>
    /// True if the note has already been hit.
    /// </summary>
    public bool WasHit = false;

    /// <summary>
    /// True if the note was spawned.
    /// </summary>
    public bool WasSpawned = false;

    public byte GetSerializedType()
    {
        if (Length > 0.0)
        {
            if (Type != "normal")
            {
                if (Parameters.Count > 0)
                    return 7; // Typed hold note with params

                return 5; // Typed hold note
            }

            if (Parameters.Count  > 0)
                return 6; // Normal hold note with params
            
            return 4; // Normal hold note
        }
        
        if (Type != "normal")
        {
            if (Parameters.Count > 0)
                return 3; // Typed tap note with params

            return 1; // Typed tap note
        }

        if (Parameters.Count > 0)
            return 2; // Tap note with params
        
        return 0; // Normal tap note
    }
    
    /// <summary>
    /// Converts data into time used by the game and also scroll velocity changes.
    /// </summary>
    /// <param name="bpmInfo">An Array of BpmInfos</param>
    public void ConvertData(BpmInfo[] bpmInfo, SvChange[] svChangeList)
    {
        BpmInfo bpm = bpmInfo.Last();
        for (int i = 0; i < bpmInfo.Length; i++)
        {
            if (bpmInfo[i].Time > Time)
            {
                bpm = bpmInfo[i - 1];
                break;
            }
        }

        bool foundStart = false;
        bool foundEnd = false;
        StartingScrollVelocity = EndingScrollVelocity = svChangeList.Length - 1;
        for (int i = 0; i < svChangeList.Length; i++)
        {
            if (svChangeList[i].Time > Time && !foundStart)
            {
                StartingScrollVelocity = i - 1;
                foundStart = true;
            }

            if (svChangeList[i].Time > Time + Length && !foundEnd)
            {
                EndingScrollVelocity = i - 1;
                foundEnd = true;
            }

            if (foundStart && foundEnd)
                break;
        }

        MsTime = ConductorUtility.MeasureToMs(Time - bpm.Time, bpm.Bpm, bpm.TimeSignatureNumerator) + bpm.MsTime;
        MsLength = ConductorUtility.MeasureToMs(Length, bpm.Bpm, bpm.TimeSignatureNumerator);
    }
}