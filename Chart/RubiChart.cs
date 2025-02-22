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
public partial class RubiChart : Resource
{
    /// <summary>
    /// The current chart version.
    /// </summary>
    public readonly static VersionInfo ChartVersion = new(1, 1, 1, 0);
    
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
            List<NoteData> notes = [];

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
}