using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Godot.Collections;
using Rubicon.Core.Data;
using Array = Godot.Collections.Array;
using Range = System.Range;

namespace Rubicon.Core.Chart;

/// <summary>
/// The general chart format for this engine.
/// </summary>
[GlobalClass]
public partial class RubiChart : RefCounted
{
    /// <summary>
    /// The current chart version.
    /// </summary>
    public readonly static VersionInfo ChartVersion = new(1, 0, 0, 0);
    
    #region Public Variables
    /// <summary>
    /// The name of the people who helped with this chart.
    /// </summary>
    [Export] public string Charter = "";
    
    /// <summary>
    /// How hard the chart really is.
    /// </summary>
    [Export] public uint Difficulty = 0;

    /// <summary>
    /// The Rubicon Engine version this chart was created on.
    /// </summary>
    [Export] public uint Version = ChartVersion.Raw;

    /// <summary>
    /// The default scroll speed for this chart.
    /// </summary>
    [Export] public float ScrollSpeed = 1.6f;

    /// <summary>
    /// The individual charts (or "strum lines") that each contain its own notes.
    /// </summary>
    [Export] public IndividualChart[] Charts = [];
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets the current version of the RubiChart format.
    /// </summary>
    /// <returns>The current version of the RubiChart format</returns>
    public VersionInfo GetVersion() => ChartVersion;
    
    /// <summary>
    /// Converts everything in this chart to millisecond format.
    /// </summary>
    /// <returns>Itself</returns>
    public RubiChart ConvertData(BpmInfo[] bpmInfo)
    {
        foreach (IndividualChart curChart in Charts)
        {
            for (int n = 0; n < curChart.Notes.Length; n++)
                curChart.Notes[n].ConvertData(bpmInfo, curChart.SvChanges);

            if (curChart.SvChanges.Length <= 1)
                continue;

            for (int s = 1; s < curChart.SvChanges.Length; s++)
                curChart.SvChanges[s].ConvertData(bpmInfo, curChart.SvChanges[s - 1]);
        }
        
        return this;
    }

    /// <summary>
    /// Sorts the notes properly and attempts to get rid of any duplicate notes and notes inside holds.
    /// </summary>
    public void Format()
    {
        for (int c = 0; c < Charts.Length; c++)
        {
            List<NoteData> notes = new List<NoteData>();

            for (int l = 0; l < Charts[c].Lanes; l++)
            {
                List<NoteData> lane = Charts[c].Notes.Where(x => x.Lane == l).ToList();
                lane.Sort((x, y) =>
                {
                    if (x.Time < y.Time)
                        return -1;
                    if (x.Time > y.Time)
                        return 1;

                    return 0;
                });

                for (int i = 0; i < lane.Count - 1; i++)
                {
                    if (lane[i].Length > 0)
                    {
                        double start = lane[i].Time;
                        double end = lane[i].Time + lane[i].Length;
                        while (i < lane.Count - 1 && lane[i + 1].Time >= start && lane[i + 1].Time < end)
                        {
                            GD.Print($"Removed note inside hold note area at {lane[i + 1].Time} in lane {l} ({start}-{end})");
                            lane.RemoveAt(i + 1);
                        }
                    }

                    while (i < lane.Count - 1 && lane[i + 1].Time == lane[i].Time)
                    {
                        GD.Print($"Removed duplicate note at {lane[i + 1].Time} in lane {l}");
                        lane.RemoveAt(i + 1);
                    }
                }

                notes.AddRange(lane);
            }

            notes.Sort((x, y) =>
            {
                if (x.Time < y.Time)
                    return -1;
                if (x.Time > y.Time)
                    return 1;

                if (x.Lane < y.Lane)
                    return -1;
                if (x.Lane > y.Lane)
                    return 1;

                return 0;
            });

            Charts[c].Notes = notes.ToArray();
        }
    }
    #endregion

    #region Serialization

    /// <summary>
    /// Loads RubiChart data from an array of bytes.
    /// </summary>
    /// <param name="bytes">An array of bytes</param>
    public void LoadBytes(byte[] bytes)
    {
        Version = BitConverter.ToUInt32(bytes.Take(new Range(0, 4)).ToArray());
        switch (Version)
        {
            default:
                this.SetupFromVersionOne(bytes);
                break;
        }
    }

    /// <summary>
    /// Serializes this chart into <see cref="byte"/>[] form.
    /// </summary>
    /// <returns>An array of bytes containing information about this chart</returns>
    public byte[] ToBytes()
    {
        // Cache note types
        List<StringName> noteTypes = new List<StringName>();
        System.Collections.Generic.Dictionary<StringName, int> noteTypeIndexes = new();
        for (int i = 0; i < Charts.Length; i++)
        {
            for (int j = 0; j < Charts[i].Notes.Length; j++)
            {
                NoteData note = Charts[i].Notes[j];
                if (noteTypes.Contains(note.Type))
                    continue;
                
                noteTypes.Add(note.Type);
                noteTypeIndexes.Add(note.Type, noteTypes.Count - 1);
            }
        }
        
        // Creation of bytes
        List<byte> bytes = new List<byte>();
        
        // Version
        bytes.AddRange(BitConverter.GetBytes(ChartVersion.Raw));
        
        // Difficulty
        bytes.AddRange(BitConverter.GetBytes(Difficulty));
        
        // Scroll Speed
        bytes.AddRange(BitConverter.GetBytes(ScrollSpeed));
        
        // Charter
        byte[] cName = Encoding.UTF8.GetBytes(Charter);
        bytes.AddRange(BitConverter.GetBytes(cName.Length));
        bytes.AddRange(cName);
        
        // Note Types
        bytes.AddRange(BitConverter.GetBytes(noteTypes.Count));
        for (int i = 0; i < noteTypes.Count; i++)
        {
            byte[] tBytes = Encoding.UTF8.GetBytes(noteTypes[i]);
            bytes.AddRange(BitConverter.GetBytes(tBytes.Length));
            bytes.AddRange(tBytes);
        }
        
        // Charts
        bytes.AddRange(BitConverter.GetBytes(Charts.Length));
        for (int i = 0; i < Charts.Length; i++)
        {
            IndividualChart chart = Charts[i];
            
            // Name
            byte[] nameBytes = Encoding.UTF8.GetBytes(chart.Name.ToString());
            bytes.AddRange(BitConverter.GetBytes(nameBytes.Length));
            bytes.AddRange(nameBytes);
            
            // Lanes
            bytes.AddRange(BitConverter.GetBytes(chart.Lanes));
            
            // Target Switching
            bytes.AddRange(BitConverter.GetBytes(chart.Switches.Length));
            for (int j = 0; j < chart.Switches.Length; j++)
            {
                TargetSwitch tSwitch = chart.Switches[j];
                bytes.AddRange(BitConverter.GetBytes(tSwitch.Time));

                byte[] sNameBytes = Encoding.UTF8.GetBytes(tSwitch.Name);
                bytes.AddRange(BitConverter.GetBytes(sNameBytes.Length));
                bytes.AddRange(sNameBytes);
            }
            
            // SV Changes
            bytes.AddRange(BitConverter.GetBytes(chart.SvChanges.Length));
            for (int j = 0; j < chart.SvChanges.Length; j++)
            {
                SvChange svChange = chart.SvChanges[j];
                bytes.AddRange(BitConverter.GetBytes(svChange.Time));
                bytes.AddRange(BitConverter.GetBytes(svChange.Multiplier));
            }
            
            // Notes
            bytes.AddRange(BitConverter.GetBytes(chart.Notes.Length));
            for (int j = 0; j < chart.Notes.Length; j++)
            {
                NoteData note = chart.Notes[j];
                bytes.AddRange(BitConverter.GetBytes(note.Time));
                bytes.AddRange(BitConverter.GetBytes(note.Length));
                bytes.AddRange(BitConverter.GetBytes(note.Lane));
                bytes.AddRange(BitConverter.GetBytes(noteTypeIndexes[note.Type])); // Number that points to note type
                
                // Parameters
                bytes.AddRange(BitConverter.GetBytes(note.Parameters.Count));
                foreach (KeyValuePair<StringName, Variant> pair in note.Parameters)
                {
                    byte[] paramName = Encoding.UTF8.GetBytes(pair.Key);
                    bytes.AddRange(BitConverter.GetBytes(paramName.Length));
                    bytes.AddRange(paramName);
                    
                    byte[] valueBytes = GD.VarToBytes(pair.Value);
                    bytes.AddRange(BitConverter.GetBytes(valueBytes.Length));
                    bytes.AddRange(valueBytes);
                }
            }
        }
        
        return bytes.ToArray();
    }

    #endregion
}