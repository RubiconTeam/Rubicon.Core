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
    [Export] public byte Lane;
        
    /// <summary>
    /// The note's type.
    /// </summary>
    [Export] public StringName Type = "Normal";

    /// <summary>
    /// Any extra parameters will be stored here.
    /// </summary>
    [Export] public Godot.Collections.Dictionary<StringName, Variant> Parameters = new();
        
    /// <summary>
    /// The starting point of the note in milliseconds.
    /// </summary>
    [ExportGroup("Internals"), Export] public float MsTime;
        
    /// <summary>
    /// Length of the note in milliseconds.
    /// </summary>
    [Export] public float MsLength;

    /// <summary>
    /// The starting point of the note in measures.
    /// </summary>
    [Export] public float MeasureTime;
    
    /// <summary>
    /// Length of the note in measures.
    /// </summary>
    [Export] public float MeasureLength;

    /// <summary>
    /// The starting scroll velocity this note is associated with.
    /// </summary>
    [Export] public int StartingScrollVelocity = 0;

    /// <summary>
    /// The ending scroll velocity for this note, if its length is more than 0.
    /// </summary>
    [Export] public int EndingScrollVelocity = 0;

    [Export] public RowData StartingRow;
    
    [Export] public RowData EndingRow;

    /// <summary>
    /// Basically tells the autoplay whether to miss this note or not. Should be ignored when serialized.
    /// </summary>
    [Export] public bool ShouldMiss = false;

    /// <summary>
    /// Whether this note should count in the score tracker or not.
    /// </summary>
    [Export] public bool CountsTowardScore = true;
    
    /// <summary>
    /// Converts data into time used by the game and also scroll velocity changes.
    /// </summary>
    /// <param name="bpmInfo">An Array of BpmInfos</param>
    public void ConvertData(TimeChange[] bpmInfo, SvChange[] svChangeList)
    {
        if (StartingRow != null)
        {
            MeasureTime = StartingRow.Section.Measure + ((float)StartingRow.Offset / StartingRow.Quant);
            MeasureLength = 0f;
            if (EndingRow != null)
            {
                float endingTime = EndingRow.Section.Measure + ((float)EndingRow.Offset / EndingRow.Quant);
                MeasureLength = endingTime - MeasureTime;
            }
        }
        
        TimeChange bpm = bpmInfo.Last();
        for (int i = 0; i < bpmInfo.Length; i++)
        {
            if (bpmInfo[i].Time > MeasureTime)
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
            if (svChangeList[i].Time > MeasureTime && !foundStart)
            {
                StartingScrollVelocity = i - 1;
                foundStart = true;
            }

            if (svChangeList[i].Time > MeasureTime + MeasureLength && !foundEnd)
            {
                EndingScrollVelocity = i - 1;
                foundEnd = true;
            }

            if (foundStart && foundEnd)
                break;
        }

        MsTime = ConductorUtility.MeasureToMs(MeasureTime - bpm.Time, bpm.Bpm, bpm.TimeSignatureNumerator) + bpm.MsTime;
        MsLength = ConductorUtility.MeasureToMs(MeasureLength, bpm.Bpm, bpm.TimeSignatureNumerator);
    }
}