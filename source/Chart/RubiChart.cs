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
    /// The name of the people who helped with this chart.
    /// </summary>
    [Export(PropertyHint.MultilineText)] public string Charter = "";
    
    /// <summary>
    /// How hard the chart really is.
    /// </summary>
    [Export] public uint Difficulty = 0;

    /// <summary>
    /// The Rubicon Engine version this chart was created on.
    /// </summary>
    [Export] public uint Version = RubiChartConstants.ChartVersion.Raw;

    /// <summary>
    /// The default scroll speed for this chart.
    /// </summary>
    [Export] public float ScrollSpeed = 1.6f;

    /// <summary>
    /// The individual charts (or "strum lines") that each contain its own notes.
    /// </summary>
    [Export] public ChartData[] Charts = [];
    
    /// <summary>
    /// Converts everything in this chart to millisecond format.
    /// </summary>
    /// <returns>Itself</returns>
    public RubiChart ConvertData(TimeChange[] bpmInfo)
    {
        foreach (ChartData curChart in Charts)
        {
            curChart.ConvertData(bpmInfo);

            if (curChart.SvChanges.Length <= 1)
                continue;

            for (int s = 1; s < curChart.SvChanges.Length; s++)
                curChart.SvChanges[s].ConvertData(bpmInfo, curChart.SvChanges[s - 1]);
        }
        
        return this;
    }

    public string[] GetAllNoteTypes()
    {
        List<string> noteTypes = new List<string>();
        for (int c = 0; c < Charts.Length; c++)
        {
            ChartData curChart = Charts[c];
            for (int s = 0; s < curChart.Sections.Length; s++)
            {
                SectionData curSection = curChart.Sections[s];
                for (int r = 0; r < curSection.Rows.Length; r++)
                {
                    RowData curRow = curSection.Rows[r];
                    string[] noteTypesInRow = curRow.GetNoteTypes();
                    for (int n = 0; n < noteTypesInRow.Length; n++)
                    {
                        if (noteTypes.Contains(noteTypesInRow[n]))
                            continue;
                        
                        noteTypes.Add(noteTypesInRow[n]);
                    }
                }
            }
        }
                        
        return noteTypes.ToArray();
    }
}